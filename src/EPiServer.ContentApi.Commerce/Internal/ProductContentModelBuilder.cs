using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Building ProductContentApiModel when content is ProductContent.
    /// </summary>
    [ServiceConfiguration(typeof(ICatalogContentModelBuilder))]
    internal class ProductContentModelBuilder : CatalogContentModelBuilderBase<ProductContent>
    {
        public ProductContentModelBuilder(
            ContentLoaderService contentLoaderService, 
            IContentModelReferenceConverter contentModelService, 
            UrlResolverService urlResolverService) 
            : base(contentLoaderService, 
                  contentModelService, 
                  urlResolverService)
        {}

        /// <inheritdoc />
        protected override ContentApiModel CreateCatalogContentModel(ProductContent content)
        {
            return new ProductContentApiModel
            {
                Assets = content.GetAssets(UrlResolverService)
            };
        }
    }
}
