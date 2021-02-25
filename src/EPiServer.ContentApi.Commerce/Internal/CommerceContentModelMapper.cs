using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Mapper to handle commerce content type
    /// The mapper transforms commerce data into Commerce Models for serialization.
    /// </summary>
    [ServiceConfiguration]
    internal class CommerceContentModelMapper : ContentModelMapperBase
    {
        protected IEnumerable<ICatalogContentModelBuilder> CatalogContentModelBuilders;

        public CommerceContentModelMapper(
            IContentTypeRepository contentTypeRepository,       
            ReflectionService reflectionService,
            IContentModelReferenceConverter contentModelService,
            IPropertyConverterResolver propertyConverterResolver,
            IContentVersionRepository contentVersionRepository,
            ContentLoaderService contentLoaderService,
            UrlResolverService urlResolverService,
            ContentApiConfiguration apiConfig,
            IEnumerable<ICatalogContentModelBuilder> catalogContentModelBuilders)
        : base(contentTypeRepository,
            reflectionService,
            contentModelService,
            contentVersionRepository,
            contentLoaderService,
            urlResolverService,
            apiConfig,
            propertyConverterResolver)
        {
            CatalogContentModelBuilders = catalogContentModelBuilders;
        }

        public override int Order => 300;

        public override bool CanHandle<T>(T content) => content is CatalogContentBase;

        /// <inheritdoc />
        protected override ContentApiModel CreateDefaultModel(IContent content)
        {
            if (!(content is CatalogContentBase catalogContent))
            {
                return base.CreateDefaultModel(content);
            }

            var builder = CatalogContentModelBuilders.Where(b => b.CanHandle(catalogContent)).OrderByDescending(b => b.SortOrder).FirstOrDefault();

            return builder?.CreateDefaultModel(catalogContent) ?? base.CreateDefaultModel(content);
        }
    }
}
