using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using IContextModeResolver = EPiServer.ContentApi.Core.IContextModeResolver;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    internal class CatalogContentLoadService : ContentLoaderService
    {
        private readonly IRelationRepository _relationRepository;

        public CatalogContentLoadService(
            IPermanentLinkMapper permanentLinkMapper,
            IRelationRepository relationRepository,
            IContentLoader contentLoader,
            IUrlResolver urlResolver,
            IContextModeResolver contextModeResolver,
            IContentProviderManager providerManager) 
            : base(contentLoader, permanentLinkMapper, urlResolver, contextModeResolver, providerManager)
        { 
            _relationRepository = relationRepository;
        }

        public override IEnumerable<IContent> GetChildren(ContentReference contentReference, string language)
        {
            var content = GetContentAndThrowIfNotExist(contentReference, language, CreateLoaderOptions(language, true));

            if (!(content is EntryContentBase))
            {
                return base.GetChildren(contentReference, language);
            }

            return GetChildrenForCatalogContent(content, CreateLoaderOptions(language), PagingConstants.DefaultStartIndex, PagingConstants.DefaultMaxRows).Where(ShouldContentBeExposed);
        }

        public override ContentDeliveryQueryRange<IContent> GetChildren(ContentReference contentReference, string language, PagingToken token, Func<IContent, bool> predicate = null)
        {
            var content = GetContentAndThrowIfNotExist(contentReference, language, CreateLoaderOptions(language, true));
            if (!(content is EntryContentBase))
            {
                return base.GetChildren(contentReference, language, token, predicate);
            }

            var totalCount = token.TotalCount ?? GetChildrenCount(content);

            var startIndex = token.LastIndex.HasValue ? token.LastIndex.Value + 1 : 0;
            return ContentPaginationHelper<IContent>.GetRange(startIndex, token.Top, (s, t) =>
            {
                var children = GetChildrenForCatalogContent(content, CreateLoaderOptions(language), s, t)
                    .Select((c, i) => new IndexedItem<IContent> { Item = c, Index = i + s });

                children = children.Where(x => ShouldContentBeExposed(x.Item)
                                               && (predicate == null || predicate(x.Item)));

                return new PagedResult<IndexedItem<IContent>>(children, totalCount);
            });
        }

        private IEnumerable<IContent> GetChildrenForCatalogContent(IContent content, LanguageSelector fallbackLanguageSelector,
            int startIndex, int maxRows)
        {
            IList<ContentReference> childrenReferences = new List<ContentReference>();

            if (content is ProductContent productContent)
            {
                childrenReferences = productContent.GetVariants(_relationRepository).ToList();
            }
            else if (content is BundleContent bundleContent)
            {
                childrenReferences = bundleContent.GetEntries(_relationRepository).ToList();
            }
            else if (content is PackageContent packageContent)
            {
                childrenReferences = packageContent.GetEntries(_relationRepository).ToList();
            }

            if (startIndex < 0)
            {
                return GetItems(childrenReferences, fallbackLanguageSelector.Language);
            }

            if (startIndex >= childrenReferences.Count)
            {
                return new List<IContent>();
            }

            if (maxRows < 1 || maxRows + startIndex >= childrenReferences.Count)
            {
                maxRows = childrenReferences.Count - startIndex;
            }

            return GetItems(new List<ContentReference>(childrenReferences.Skip(startIndex).Take(maxRows)),
                fallbackLanguageSelector.Language);
        }

        private int GetChildrenCount(IContent parent)
        {
            switch (parent)
            {
                case ProductContent _:
                    return _relationRepository.GetChildren<ProductVariation>(parent.ContentLink.ToReferenceWithoutVersion()).Count();
                case BundleContent _:
                    return _relationRepository.GetChildren<BundleEntry>(parent.ContentLink.ToReferenceWithoutVersion()).Count();
                case PackageContent _:
                    return _relationRepository.GetChildren<PackageEntry>(parent.ContentLink.ToReferenceWithoutVersion()).Count();
                default:
                    return 0;
            }
        }
    }
}
