using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Building VariationContentApiModel when content is VariationContent.
    /// </summary>
    [ServiceConfiguration(typeof(ICatalogContentModelBuilder))]
    internal class VariationContentModelBuilder : CatalogContentModelBuilderBase<VariationContent>
    {
        public VariationContentModelBuilder(
            ContentLoaderService contentLoaderService,
            IContentModelReferenceConverter contentModelService,
            UrlResolverService urlResolverService)
            : base(contentLoaderService,
                  contentModelService,
                  urlResolverService)
        {
        }

        /// <inheritdoc />
        protected override ContentApiModel CreateCatalogContentModel(VariationContent content)
        {
            return new VariationContentApiModel
            {
                Assets = content.GetAssets(UrlResolverService)
            };
        }
    }
}
