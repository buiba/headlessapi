using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Building NodeContentModelBuilder when content is NodeContent.
    /// </summary>
    [ServiceConfiguration(typeof(ICatalogContentModelBuilder))]
    internal class NodeContentModelBuilder : CatalogContentModelBuilderBase<NodeContent>
    {
        public NodeContentModelBuilder(
            ContentLoaderService contentLoaderService, 
            IContentModelReferenceConverter contentModelService, 
            UrlResolverService urlResolverService) 
            : base(contentLoaderService, 
                  contentModelService, 
                  urlResolverService)
        {
        }        

        /// <inheritdoc />
        protected override ContentApiModel CreateCatalogContentModel(NodeContent content)
        {
            return new NodeContentApiModel
            {
                Assets = content.GetAssets(UrlResolverService)
            };
        }
    }
}
