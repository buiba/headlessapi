using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Forms.Configuration;
using EPiServer.Forms.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Forms
{
    //plan is to obsolete this partial class as [Obsolete("Replaced by IContentConverter")]

    /// <summary>
    /// Mapper to handle form content type
    /// The mapper transforms form data into FormContentApiModel for serialization.
    /// </summary>
    [ServiceConfiguration(typeof(IContentModelMapper), Lifecycle = ServiceInstanceScope.Singleton)]
    public partial class FormContentModelMapper : ContentModelMapperBase
    {
        private const string FormInitScript = "FormInitScript";
        private readonly ContentApiConfiguration _apiConfiguration;
        private readonly IEPiServerFormsImplementationConfig _formConfig;
        private readonly FormRenderingService _formRenderingService;

        /// <summary>
        ///     Initialize a new instance of <see cref="FormContentModelMapper"/>
        /// </summary>
        [Obsolete("Use alternative constructor")]
        public FormContentModelMapper(IContentTypeRepository contentTypeRepository,
                                      ReflectionService reflectionService,
                                      IContentModelReferenceConverter contentModelService,
                                      IEnumerable<IPropertyModelConverter> propertyModelConverters,
                                      IContentVersionRepository contentVersionRepository,
                                      ContentLoaderService contentLoaderService,
                                      IEPiServerFormsImplementationConfig formConfig,
                                      FormRenderingService formRenderingService,
                                      UrlResolverService urlResolverService)
            : this(contentTypeRepository,
                   reflectionService,
                   contentModelService,
                   propertyModelConverters,
                   contentVersionRepository,
                   contentLoaderService,
                   formConfig,
                   formRenderingService,
                   urlResolverService,
                   ServiceLocator.Current.GetInstance<ContentApiConfiguration>())
        {

        }

        /// <summary>
        ///     Initialize a new instance of <see cref="FormContentModelMapper"/>
        /// </summary>
        [Obsolete("Use alternative constructor")]
        public FormContentModelMapper(IContentTypeRepository contentTypeRepository,
                                      ReflectionService reflectionService,
                                      IContentModelReferenceConverter contentModelService,
                                      IEnumerable<IPropertyModelConverter> propertyModelConverters,
                                      IContentVersionRepository contentVersionRepository,
                                      ContentLoaderService contentLoaderService,
                                      IEPiServerFormsImplementationConfig formConfig,
                                      FormRenderingService formRenderingService,
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
            _formConfig = formConfig;
            _formRenderingService = formRenderingService;
        }

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return 200;
            }
        }

        /// <inheritdoc />
        public override bool CanHandle<T>(T content)
        {
            return content is IFormContainerBlock;
        }

        /// <inheritdoc />
        public override ContentApiModel TransformContent(IContent content, bool excludePersonalizedContent = false, string expand = "")
        {
            return Convert(content, new ConverterContext(_apiConfiguration.Default(), string.Empty, expand, excludePersonalizedContent, (content as ILocale)?.Language));
        }
    }
}
