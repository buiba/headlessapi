using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Base class for Model builder
    /// </summary>
    internal abstract class CatalogContentModelBuilderBase<T> : ICatalogContentModelBuilder where T : CatalogContentBase
    {
        protected readonly UrlResolverService UrlResolverService;
        protected readonly ContentLoaderService ContentLoaderService;
        protected readonly IContentModelReferenceConverter ContentModelService;
        

        protected CatalogContentModelBuilderBase(
            ContentLoaderService contentLoaderService,
            IContentModelReferenceConverter contentModelService,
            UrlResolverService urlResolverService)
        {
            ContentLoaderService = contentLoaderService;
            ContentModelService = contentModelService;
            UrlResolverService = urlResolverService;
        }

        /// <summary>
        /// The builder with higher order is chosen to handle a specific content in different contexts. The Default implementation has Order 100.
        /// </summary>
        public virtual int SortOrder => 100;

        /// <summary>
        /// Whether a builder can build a model for a specific content
        /// </summary>       
        public virtual bool CanHandle(CatalogContentBase content)
        {
            return content is T;
        }

        /// <summary>
        /// Create default model corresponding with the content
        /// </summary>
        public virtual ContentApiModel CreateDefaultModel(CatalogContentBase content)
        {
            var model = CreateCatalogContentModel(content as T);
            var parent = ContentLoaderService.Get(content.ParentLink, (content as ILocalizable)?.Language?.Name, true);
            model.ContentLink = ContentModelService.GetContentModelReference(content);
            model.Name = content.Name;
            model.ParentLink = ContentModelService.GetContentModelReference(parent);
            
            return model;
        }

        /// <summary>
        /// Create catalog based content model corresponding with the content
        /// </summary>
        protected abstract ContentApiModel CreateCatalogContentModel(T content);
    }
}
