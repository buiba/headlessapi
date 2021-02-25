using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Building BundleContentApiModel when content is BundleContent.
    /// </summary>
    [ServiceConfiguration(typeof(ICatalogContentModelBuilder))]
    internal class BundleContentModelBuilder : CatalogContentModelBuilderBase<BundleContent>
    {
        public BundleContentModelBuilder(
            ContentLoaderService contentLoaderService, 
            IContentModelReferenceConverter contentModelService,
            UrlResolverService urlResolverService) 
            : base(contentLoaderService, 
                  contentModelService, 
                  urlResolverService)
        {
        }
        
        /// <inheritdoc />
        protected override ContentApiModel CreateCatalogContentModel(BundleContent content)
        {
            return new BundleContentApiModel
            {
                Assets = content.GetAssets(UrlResolverService)
            };
        }
    }
}
