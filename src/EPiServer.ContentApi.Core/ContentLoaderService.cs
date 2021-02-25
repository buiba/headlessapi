using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.ContentApi.Core.Internal;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// This service is an abstraction layer on top of content and content repositories.
    /// It mostly can be used to load content data from repository
    /// </summary>
    [ServiceConfiguration(typeof(ContentLoaderService))]
    public class ContentLoaderService
    {
        protected readonly IContentLoader _contentLoader;
        private readonly IPermanentLinkMapper _permanentLinkMapper;
        private readonly IContentProviderManager _providerManager;
        private readonly IUrlResolver _urlResolver;
        private readonly IContextModeResolver _contextModeResolver;
        protected static readonly ILogger _logger = LogManager.GetLogger(typeof(ContentLoaderService));

        // For easier mocking
        /// <internal-api/>
        protected ContentLoaderService() { }

        [Obsolete("This constructor is no longer used. Using the one with IContentProviderManager parameter instead")]
        public ContentLoaderService(IContentLoader contentLoader)
            : this(
                contentLoader,
                ServiceLocator.Current.GetInstance<IPermanentLinkMapper>(),
                ServiceLocator.Current.GetInstance<IUrlResolver>(),
                ServiceLocator.Current.GetInstance<IContextModeResolver>(),
                ServiceLocator.Current.GetInstance<IContentProviderManager>())
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="ContentLoaderService"/>
        /// </summary>
        public ContentLoaderService(
            IContentLoader contentLoader,
            IPermanentLinkMapper permanentLinkMapper,
            IUrlResolver urlResolver,
            IContextModeResolver contextModeResolver,
            IContentProviderManager providerManager)
        {
            _contentLoader = contentLoader;
            _providerManager = providerManager;
            _permanentLinkMapper = permanentLinkMapper;
            _urlResolver = urlResolver;
            _contextModeResolver = contextModeResolver;
        }

        /// <summary>
        /// Get content item represented by the provided reference in given language.
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned.
        /// </summary>
        /// <param name="contentReference">The reference to the content.</param>
        /// <param name="language">The language of content to get</param>
        /// <returns>The requested content item, as the specified type.</returns>
        public virtual IContent Get(ContentReference contentReference, string language) => Get(contentReference, language, false);

        /// <summary>
        /// Get content item represented by the provided reference in given language. 
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned. 
        /// </summary>        
        /// <param name="contentReference">The reference to the content.</param>
        /// <param name="language">The language of content to get</param>
        /// <param name="fallbackToMaster">Specifies if it should fallback to load content in master language</param>
        /// <returns>The requested content item, as the specified type.</returns>
        public virtual IContent Get(ContentReference contentReference, string language, bool fallbackToMaster = false)
        {
            if (ContentReference.IsNullOrEmpty(contentReference))
            {
                return null;
            }

            var fallbackLanguageSelector = CreateLoaderOptions(language, fallbackToMaster);
            var content = _contentLoader.Get<IContent>(contentReference, fallbackLanguageSelector);

            return ShouldContentBeExposed(content) ? content : null;
        }

        /// <summary>
        /// Get content item represented by the provided GUID in given language.
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned.
        /// </summary>
        /// <param name="guid">The reference to the content.</param>
        /// <param name="language">The language of content to get</param>
        /// <returns>The requested content item, as the specified type.</returns>
        public virtual IContent Get(Guid guid, string language)
        {
            return Get(guid, language, false);
        }

        /// <summary>
        /// Get content item represented by the provided GUID in given language.
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned.
        /// </summary>
        /// <param name="guid">The reference to the content.</param>
        /// <param name="language">The language of content to get</param>
        /// <param name="fallbackToMaster">Specifies if it should fallback to load content in master language</param>
        /// <returns>The requested content item, as the specified type.</returns>
        public virtual IContent Get(Guid guid, string language, bool fallbackToMaster)
        {
            if (guid == Guid.Empty)
            {
                return null;
            }

            var fallbackLanguageSelector = CreateLoaderOptions(language, fallbackToMaster);
            var content = _contentLoader.Get<IContent>(guid, fallbackLanguageSelector);

            return ShouldContentBeExposed(content) ? content : null;
        }

        /// <summary>
        /// Get content item represtended by the provided contentUrl
        /// </summary>
        /// <returns>The requested content item, as the specified type.</returns>
        [Obsolete("Will be removed in future version")]
        public virtual IContent GetByUrl(string contentUrl, bool matchExact, bool allowPreview)
        {
            var urlBuilder = new UrlBuilder(contentUrl);
            var contextMode = _contextModeResolver.Resolve(contentUrl, ContextMode.Default);
            if (contextMode.EditOrPreview() && !allowPreview)
            {
                return null;
            }

            var matched = _urlResolver.Route(urlBuilder, contextMode);
            if (matched == null && !matchExact && contextMode == ContextMode.Default)
            {
                while (matched == null && urlBuilder.Uri.Segments.Length > 1)
                {
                    urlBuilder.Path = string.Join(string.Empty, urlBuilder.Uri.Segments.Take(urlBuilder.Uri.Segments.Length - 1));
                    matched = _urlResolver.Route(urlBuilder, contextMode);
                }
            }
            return matched;
        }

        /// <summary>
        /// Get the children of the content represented by the provided reference in given language.
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned.
        /// </summary>
        /// <returns> all children of the given content </returns>
        public virtual IEnumerable<IContent> GetChildren(ContentReference contentReference, string language)
        {
            var content = GetContentAndThrowIfNotExist(contentReference, language, CreateLoaderOptions(language, true));

            return _contentLoader.GetChildren<IContent>(contentReference, CreateLoaderOptions(language), PagingConstants.DefaultStartIndex, PagingConstants.DefaultMaxRows)
                    .Where(ShouldContentBeExposed);
        }

        /// <summary>
        /// Get the children of the content represented by the provided reference in given language. 
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned. 
        /// </summary>
        /// <returns> all children of the given content </returns>
        public virtual ContentDeliveryQueryRange<IContent> GetChildren(Guid contentGuid, string language, PagingToken token, Func<IContent, bool> predicate = null)
        {
            var permanentLinkMap = _permanentLinkMapper.Find(contentGuid);
            if (permanentLinkMap == null)
            {
                throw new ContentNotFoundException(contentGuid);
            }
            return GetChildren(permanentLinkMap.ContentReference, language, token, predicate);
        }

        /// <summary>
        /// Get the children of the content represented by the provided reference in given language. 
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned. 
        /// </summary>
        /// <returns> all children of the given content </returns>
        public virtual ContentDeliveryQueryRange<IContent> GetChildren(ContentReference contentReference, string language, PagingToken token, Func<IContent, bool> predicate = null)
        {
            var content = GetContentAndThrowIfNotExist(contentReference, language, CreateLoaderOptions(language, true));
            var totalCount = token.TotalCount ?? GetChildrenCount(content);

            var startIndex = token.LastIndex.HasValue ? token.LastIndex.Value + 1 : 0;
            return ContentPaginationHelper<IContent>.GetRange(startIndex, token.Top, (s, t) =>
            {
                var children = _contentLoader.GetChildren<IContent>(contentReference, CreateLoaderOptions(language), s, t)
                                .Select((c, i) => new IndexedItem<IContent> { Item = c, Index = i + s });

                children = children.Where(x => ShouldContentBeExposed(x.Item)
                                && (predicate == null || predicate(x.Item)));

                return new PagedResult<IndexedItem<IContent>>(children, totalCount);
            });
        }

        /// <summary>
        /// Get ancestors of the content represented by the provided reference in given language. 
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned. 
        /// </summary>
        /// <param name="contentGuid">The guid based content identifier</param>
        /// <param name="language">The language to retieve parents in</param>
        /// <returns> all ancestors of the given content</returns>
        public virtual IEnumerable<IContent> GetAncestors(Guid contentGuid, string language)
        {
            var permanentLinkMap = _permanentLinkMapper.Find(contentGuid);
            if (permanentLinkMap == null)
            {
                throw new ContentNotFoundException(contentGuid);
            }

            return GetAncestors(permanentLinkMap.ContentReference, language);
        }

        /// <summary>
        /// Get ancestors of the content represented by the provided reference in given language. 
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned. 
        /// </summary>
        /// <returns> all ancestors of the given content</returns>
        public virtual IEnumerable<IContent> GetAncestors(ContentReference contentReference, string language)
        {
            if (ContentReference.IsNullOrEmpty(contentReference))
            {
                throw new ContentNotFoundException(contentReference);
            }
            var fallbackLanguageSelector = CreateLoaderOptions(language, true);

            // if the content itself does not exist or invalid, the ContentNotFoundException is thrown immediately before retrieving its ancestors
            var isContentFound = string.IsNullOrWhiteSpace(language) ? _contentLoader.TryGet(contentReference, out IContent content)
                                                                     : _contentLoader.TryGet(contentReference, fallbackLanguageSelector, out content);

            if (!isContentFound || !ShouldContentBeExposed(content))
            {
                throw new ContentNotFoundException(contentReference);
            }

            var ancestorContentReferences = _contentLoader.GetAncestors(contentReference).Select(x => x.ContentLink);
            var ancestors = _contentLoader.GetItems(ancestorContentReferences, fallbackLanguageSelector);

            return ancestors.Where(ShouldContentBeExposed);
        }

        /// <summary>
        /// Get all content items that is represented by the provided references given the language and loader options.
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned.
        /// </summary>
        /// <param name="contentGuids"></param>
        /// <param name="language"></param>
        /// <returns> A list of content for the specifed references </returns>
        public virtual IEnumerable<IContent> GetItemsWithOptions(IEnumerable<Guid> contentGuids, string language)
        {
            return GetItemsWithOptions(contentGuids.Select(g => _permanentLinkMapper.Find(g)?.ContentReference).Where(c => !ContentReference.IsNullOrEmpty(c)), language);
        }

        /// <summary>
        /// Get all content items that is represented by the provided references given the language and loader options.
        /// This method supports fallback language
        /// i.e: if content in the given language does not exist , so the content in fallback language is returned.
        /// </summary>
        /// <returns> A list of content for the specifed references </returns>
        public virtual IEnumerable<IContent> GetItemsWithOptions(IEnumerable<ContentReference> contentReferences, string language)
        {
            if (contentReferences == null || !contentReferences.Any())
            {
                return Enumerable.Empty<IContent>();
            }

            var fallbackLanguageSelector = CreateLoaderOptions(language);
            var items = _contentLoader.GetItems(contentReferences, fallbackLanguageSelector);

            return items.Where(ShouldContentBeExposed);
        }

        /// <summary>
        /// Get all content items that is represented by the provided references in the given language.
        /// For references associated with a specific version (that is where EPiServer.Core.ContentReference.WorkID  is set)
        ///	the language is ignored and that version is returned.
        ///	If contentReferences contain duplicate entries, this method returns all content including duplicated ones
        /// </summary>
        /// <returns> A list of content for the specifed references </returns>
        public virtual IEnumerable<IContent> GetItems(IEnumerable<ContentReference> contentReferences, CultureInfo language)
        {
            if (contentReferences == null || !contentReferences.Any())
            {
                return Enumerable.Empty<IContent>();
            }

            var contentList = new List<IContent>();
            foreach (var contentLink in contentReferences)
            {
                var content = this.Get(contentLink, language);
                if (content != null)
                {
                    contentList.Add(content);
                }
            }

            return contentList;
        }

        private IContent Get(ContentReference contentReference, CultureInfo language)
        {
            try
            {
                var content = _contentLoader.Get<IContent>(contentReference, language);
                return ShouldContentBeExposed(content) ? content : null;
            }
            catch (ContentNotFoundException ex)
            {
                _logger.Error("Content not found", ex);
                return null;
            }
        }

        /// <summary>
        /// Check if content is draft or expired or unpublished
        /// </summary>
        protected virtual bool ShouldContentBeExposed(IContent content)
        {
            var versionableContent = content as IVersionable;
            var shouldContentBeExposed = versionableContent == null ||
                                         (versionableContent.Status == VersionStatus.Published &&
                                          (versionableContent.StopPublish == null ||
                                           versionableContent.StopPublish >= DateTime.Now));

            return shouldContentBeExposed;
        }

        /// <summary>
        /// Create an instance of LanguageSelector with a given language along with fallback option.
        /// If language is null, create a language selector with Master language.
        /// If language is not null, create a language selector with LanguageSelector.Fallback
        /// </summary>
        protected virtual LanguageSelector CreateLoaderOptions(string language, bool shouldUseMasterIfFallbackNotExist = false)
        {
            //     (1)  LanguageSelector with Master means that content in master language should be returned as expected.
            //     (2)  LanguageSelector.Fallback(language, shouldUseMasterIfFallbackNotExist)
            //              means that the content in fallback language should be returned if content does not exist in the given language
            //              and in case (content does not exist in both given language and fallback), if shouldUseMasterIfFallbackNotExist = true,
            //              then, content in Master should be obtained.

            // Ex: CreateLoaderOptions("en") will return a selector with language is "en" with the fallback option enabled.
            // and then this selector is passed to ContentLoader.Get() (or other api callers), the ContentLoader
            // will try to get the content in "en". If the content does not have any version in "en" published, the loader will get
            // the content in fallback language of "en". If nothing exists and shouldUseMasterIfFallbackNotExist = true, ContentLoader will try to get the content
            // in master langage.
            return string.IsNullOrWhiteSpace(language) ? LanguageSelector.MasterLanguage() : LanguageSelector.Fallback(language, shouldUseMasterIfFallbackNotExist);
        }

        protected IContent GetContentAndThrowIfNotExist(ContentReference contentReference, string language, LanguageSelector fallbackLanguageSelector)
        {
            if (ContentReference.IsNullOrEmpty(contentReference))
            {
                throw new ContentNotFoundException(contentReference);
            }

            IContent content;
            // if the content itself does not exist or invalid, the ContentNotFoundException is thrown immediately before retrieving its children
            var isContentFound = string.IsNullOrWhiteSpace(language) ? _contentLoader.TryGet(contentReference, out content)
                                                                     : _contentLoader.TryGet(contentReference, fallbackLanguageSelector, out content);
            if (!isContentFound || !ShouldContentBeExposed(content))
            {
                throw new ContentNotFoundException(contentReference);
            }

            return content;
        }

        /// <summary>
        /// Get content item represented by the provided reference in given language. 
        /// </summary>
        /// <param name="contentReference">The reference to the content.</param>
        /// <param name="language">The language of content to get</param>
        /// <param name="alwaysExposeContent">Specifies if it should return draft or not.</param>
        /// <returns></returns>
        internal IContent Get(ContentReference contentReference, CultureInfo language, bool alwaysExposeContent)
        {
            if (ContentReference.IsNullOrEmpty(contentReference))
            {
                return null;
            }

            return alwaysExposeContent ? _contentLoader.Get<IContent>(contentReference, language) : Get(contentReference, language);
        }

        private int GetChildrenCount(IContent parent)
        {
            var noWorkId = parent.ContentLink.ToReferenceWithoutVersion();
            var provider = _providerManager.ProviderMap.GetProvider(noWorkId);
            var localizable = parent as ILocalizable;
            var parentLanguage = localizable?.Language?.Name;

            return provider.GetChildrenReferences<IContent>(noWorkId, parentLanguage, PagingConstants.DefaultStartIndex, PagingConstants.DefaultMaxRows).Count;
        }
    }
}
