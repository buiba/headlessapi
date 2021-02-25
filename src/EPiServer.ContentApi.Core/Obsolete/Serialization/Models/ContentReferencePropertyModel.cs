using EPiServer.ContentApi.Core.Security;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyContentReference"/>
    /// </summary>
    public partial class ContentReferencePropertyModel : ContentReferencePropertyModelBase<ContentModelReference, PropertyContentReference>
    {
        public ContentReferencePropertyModel(
            PropertyContentReference propertyContentReference,
            bool excludePersonalizedContent): base(propertyContentReference, excludePersonalizedContent)
        {

        }

        [Obsolete("Use alternative constructor")]
        public ContentReferencePropertyModel(
            PropertyContentReference propertyContentReference,
            bool excludePersonalizedContent,
            IContentLoader contentLoader,
            IContentModelMapper contentModelMapper,
            IContentAccessEvaluator accessEvaluator,
			ISecurityPrincipal principalAccessor) : this(
                propertyContentReference,
                excludePersonalizedContent,
                contentLoader,
                contentModelMapper,
                accessEvaluator,
                principalAccessor,
                ServiceLocator.Current.GetInstance<UrlResolverService>())
        {

        }

        public ContentReferencePropertyModel(PropertyContentReference propertyContentReference,
                                             bool excludePersonalizedContent,
                                             IContentLoader contentLoader,
                                             IContentModelMapper contentModelMapper,
                                             IContentAccessEvaluator accessEvaluator,
                                             ISecurityPrincipal principalAccessor,
                                             UrlResolverService urlResolverService)
            : base(propertyContentReference,
                   excludePersonalizedContent,
                   contentLoader,
                   contentModelMapper,
                   accessEvaluator,
                   principalAccessor,
                   urlResolverService) 
        {
        }

        public ContentReferencePropertyModel(PropertyContentReference propertyContentReference,
                                       ConverterContext converterContext,
                                       IContentLoader contentLoader,
                                       ContentConvertingService contentConvertingService,
                                       IContentAccessEvaluator accessEvaluator,
                                       ISecurityPrincipal principalAccessor,
                                       UrlResolverService urlResolverService)
      : base(propertyContentReference,
             converterContext,
             contentLoader,
             contentConvertingService,
             accessEvaluator,
             principalAccessor,
             urlResolverService)
        {
        }        
    }
}
