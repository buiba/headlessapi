using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Building PackageContentModelBuilder when content is PackageContent.
    /// </summary>
    [ServiceConfiguration(typeof(ICatalogContentModelBuilder))]
    internal class PackageContentModelBuilder : CatalogContentModelBuilderBase<PackageContent>
    {
        public PackageContentModelBuilder(
            ContentLoaderService contentLoaderService, 
            IContentModelReferenceConverter contentModelService,
            UrlResolverService urlResolverService) 
            : base(contentLoaderService, 
                contentModelService, 
                urlResolverService)
        {
        }
        
        /// <inheritdoc />
        protected override ContentApiModel CreateCatalogContentModel(PackageContent content)
        {
            return new PackageContentApiModel
            {
                Assets = content.GetAssets(UrlResolverService)
            };
        }
    }
}
