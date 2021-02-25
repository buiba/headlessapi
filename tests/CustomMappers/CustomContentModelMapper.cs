using System;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Data.Entity;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace CustomMappers
{
    [ServiceConfiguration(typeof(IContentModelMapper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CustomContentModelMapper : ContentModelMapperBase
    {
        public const string HandledContentName = "CustomPropertyMapped";
        public const string ContentMapperAddedPrefix = "CustomContentMapper";

        public CustomContentModelMapper(IContentTypeRepository contentTypeRepository,
            ReflectionService reflectionService,
            IContentModelReferenceConverter contentModelService,
            IContentVersionRepository contentVersionRepository,
            ContentLoaderService contentLoaderService,
            UrlResolverService urlResolverService,
            ContentApiConfiguration apiConfig,
            IPropertyConverterResolver propertyConverterResolver)
            : base(
                contentTypeRepository,
                reflectionService,
                contentModelService,
                contentVersionRepository,
                contentLoaderService,
                urlResolverService,
                apiConfig,
                propertyConverterResolver)
        { }

        public override int Order => 5000;

        public override bool CanHandle<T>(T content) => HandledContentName.Equals((content as IContent).Name, StringComparison.OrdinalIgnoreCase);

        public override ContentApiModel TransformContent(IContent content, bool excludePersonalizedContent = false, string expand = "")
        {
            content = (content as IReadOnly).CreateWritableClone() as IContent;
            content.Property[HandledContentName].Value = ContentMapperAddedPrefix + content.Property[HandledContentName].Value;
            return base.TransformContent(content, excludePersonalizedContent, expand);
        }
    }
}
