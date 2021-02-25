using EPiServer.ContentApi.Core.Security;
using EPiServer.Core;
using EPiServer.Security;
using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyPageReference"/>
    /// </summary>
    public partial class PageReferencePropertyModel : ContentReferencePropertyModelBase<ContentModelReference, PropertyPageReference>
    {
        public PageReferencePropertyModel(
                    PropertyPageReference propertyContentReference,
                    bool excludePersonalizedContent
                    ) : base(propertyContentReference, excludePersonalizedContent)
        {
        }

        [Obsolete("Use alternative constructor")]
        public PageReferencePropertyModel(
                    PropertyPageReference propertyContentReference,
                    bool excludePersonalizedContent,
                    IContentLoader contentLoader,
                    IContentModelMapper contentModelMapper,
                    IContentAccessEvaluator accessEvaluator,
                    ISecurityPrincipal principalAccessor)
            : base(
                    propertyContentReference,
                    excludePersonalizedContent,
                    contentLoader,
                    contentModelMapper,
                    accessEvaluator,
                    principalAccessor)
        {
        }

        public PageReferencePropertyModel(
                    PropertyPageReference propertyContentReference,
                    bool excludePersonalizedContent,
                    IContentLoader contentLoader,
                    IContentModelMapper contentModelMapper,
                    IContentAccessEvaluator accessEvaluator,
                    ISecurityPrincipal principalAccessor,
                    UrlResolverService urlResolverService)
            : base(
                  propertyContentReference,
                  excludePersonalizedContent,
                  contentLoader,
                  contentModelMapper,
                  accessEvaluator,
                  principalAccessor,
                  urlResolverService)
        {
        }

        public PageReferencePropertyModel(
                   PropertyPageReference propertyContentReference,
                   ConverterContext converterContext,
                   IContentLoader contentLoader,
                   ContentConvertingService contentConvertingService,
                   IContentAccessEvaluator accessEvaluator,
                   ISecurityPrincipal principalAccessor,
                   UrlResolverService urlResolverService)
           : base(
                 propertyContentReference,
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
