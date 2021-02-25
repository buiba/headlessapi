using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    //plan is to obsolete class as [Obsolete("Replaced by IContentConverter")]

    /// <summary>
    /// Provides a default implementation of IContentModelMapper to map instance of IContent to ContentApiModel
    /// </summary>
    [ServiceConfiguration(typeof(IContentModelMapper), Lifecycle = ServiceInstanceScope.Singleton)]
    [ServiceConfiguration]
    public class DefaultContentConverter : ContentModelMapperBase
    {
        [Obsolete("Use alternative constructor")]
        public DefaultContentConverter(
            IContentTypeRepository contentTypeRepository,
            ReflectionService reflectionService,
            IContentModelReferenceConverter contentModelService,
            IEnumerable<IPropertyModelConverter> propertyModelConverters,
            IContentVersionRepository contentVersionRepository,
            ContentLoaderService contentLoaderService,
            UrlResolverService urlResolverService)
            : this(contentTypeRepository,
                   reflectionService,
                   contentModelService,
                   propertyModelConverters,
                   contentVersionRepository,
                   contentLoaderService,
                   urlResolverService,
                   ServiceLocator.Current.GetInstance<ContentApiConfiguration>())
        { }

        [Obsolete("Use alternative constructor")]
        public DefaultContentConverter(IContentTypeRepository contentTypeRepository,
            ReflectionService reflectionService,
            IContentModelReferenceConverter contentModelService,
            IEnumerable<IPropertyModelConverter> propertyModelConverters,
            IContentVersionRepository contentVersionRepository,
            ContentLoaderService contentLoaderService,
            UrlResolverService urlResolverService,
            ContentApiConfiguration apiConfig)
            : base(contentTypeRepository,
                  reflectionService,
                  contentModelService,
                  propertyModelConverters,
                  contentVersionRepository,
                  contentLoaderService,
                  urlResolverService,
                  apiConfig)
        {
        }

        public DefaultContentConverter(IContentTypeRepository contentTypeRepository,
           ReflectionService reflectionService,
           IContentModelReferenceConverter contentModelService,
           IContentVersionRepository contentVersionRepository,
           ContentLoaderService contentLoaderService,
           UrlResolverService urlResolverService,
           ContentApiConfiguration apiConfig,
           IPropertyConverterResolver propertyConverterResolver)
           : base(contentTypeRepository,
                 reflectionService,
                 contentModelService,
                 contentVersionRepository,
                 contentLoaderService,
                 urlResolverService,
                 apiConfig,
                 propertyConverterResolver)
        {
        }

        /// <inheritdoc />      
        public override int Order
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// This is the default model mapper, so it should be able to handle most kinds of IContent.
        /// </summary>
        public override bool CanHandle<T>(T content)
        {
            return content is IContent;
        }
    }
}
