using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization
{
    //Plan is to obsolete all in this part of the partial class

    /// <summary>
    /// Base class for model mappers
    /// </summary>
    public abstract partial class ContentModelMapperBase : IContentModelMapper
    {
        [Obsolete("use alternative constructor")]
        public ContentModelMapperBase(IContentTypeRepository contentTypeRepository,
            ReflectionService reflectionService,
            IContentModelReferenceConverter contentModelService,
            IEnumerable<IPropertyModelConverter> propertyModelConverters,
            IContentVersionRepository contentVersionRepository,
            ContentLoaderService contentLoaderService,
            UrlResolverService urlResolverService,
            ContentApiConfiguration apiConfig)
            :this(contentTypeRepository, reflectionService, contentModelService, contentVersionRepository, contentLoaderService, urlResolverService, apiConfig, ServiceLocator.Current.GetInstance<IPropertyConverterResolver>())
        {
            _propertyModelConverters = propertyModelConverters
                .OrderByDescending(x => x.SortOrder)
                .ToList();
        }

        [Obsolete("use alternative constructor")]
        public ContentModelMapperBase(
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
        {
        }

        [Obsolete("use alternative constructor")]
        public ContentModelMapperBase(IContentTypeRepository contentTypeRepository,
            ReflectionService reflectionService,
            IContentModelReferenceConverter contentModelService,
            IUrlResolver urlResolver,
            IEnumerable<IPropertyModelConverter> propertyModelConverters,
            IContentVersionRepository contentVersionRepository,
            ContentLoaderService contentLoaderService) : this(
                contentTypeRepository,
                reflectionService,
                contentModelService,
                propertyModelConverters,
                contentVersionRepository,
                contentLoaderService,
                ServiceLocator.Current.GetInstance<UrlResolverService>())
        {
            _urlResolver = urlResolver;
        }

        IEnumerable<IPropertyModelConverter> _propertyModelConverters;

        /// <summary>
        /// The mapper with higher order is chosen to handle a specific content in different contexts. The Default implementation has Order 100.
        /// </summary>
        public abstract int Order { get; }

        /// <summary>
        /// Whether a mapper can handle a specific content
        /// </summary>
        public abstract bool CanHandle<T>(T content) where T : class;

        /// <summary>
        /// Method generates a ContentModel based on the provided IContent.
        /// </summary>
        /// <param name="content">The IContent object that the ContentModel is generated from</param>
        /// <param name="excludePersonalizedContent">Boolean to indicate whether or not to return personalization data in the instance of the ContentApiModel</param>
        /// <param name="expand"> String contain properties need to be expand, seperated by comma. Eg: expand=MainContentArea,productPageLinks. Pass expand='*' to expand all properties</param>
        /// <returns>Instance of ContentModel</returns>
        public virtual ContentApiModel TransformContent(IContent content, bool excludePersonalizedContent = false, string expand = "")
        {
            return Convert(content, new ConverterContext(_apiConfig.Default(), string.Empty, expand, excludePersonalizedContent, (content as ILocale)?.Language));
        }

        /// <summary>
        /// Extract content properties to ContentApiModel
        /// </summary>
        protected virtual void ExtractContentProperties(IContent content, ContentApiModel contentModel) => ExtractContentProperties(content, contentModel, new ConverterContext(_apiConfig.Default(), string.Empty, string.Empty, false, (content as ILocale)?.Language));

        /// <summary>
        /// Extract property data collection from content
        /// </summary>
        protected virtual IDictionary<string, object> ExtractPropertyDataCollection(IContent content, string expand, bool excludePersonalizedContent) => ExtractCustomProperties(content, new ConverterContext(_apiConfig.Default(), string.Empty, string.Empty, false, (content as ILocale)?.Language));

        /// <inheritdoc />
        public virtual IEnumerable<IPropertyModelConverter> PropertyModelConverters
        {
            get
            {
                if (_propertyModelConverters == null)
                {
                    _propertyModelConverters = ServiceLocator.Current.GetAllInstances<IPropertyModelConverter>()
                        .OrderByDescending(x => x.SortOrder)
                        .ToList();
                }
                return _propertyModelConverters;
            }
        }

        /// <summary>
        /// Preview API: This method is current in preview state meaning it might change between minor versions
        /// </summary>
        [Obsolete("This preview method will be removed in an upcoming release")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual ContentTypeModel GetExtendedContentType(IContent content)
        {
            var contentType = GetContentTypeById(content.ContentTypeID);

            var model = new ContentTypeModel
            {
                Name = contentType.Name,
                GuidValue = contentType.GUID,
                Base = contentType.Base.ToString()
            };

            return model;
        }
    }
}
