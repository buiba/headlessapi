using EPiServer.ContentApi.Core.Security;
using EPiServer.Core;
using EPiServer.Security;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyPageReference"/>
    /// </summary>
    public partial class PageReferencePropertyModel : ContentReferencePropertyModelBase<ContentModelReference, PropertyPageReference>
    {
        [JsonConstructor]
        internal PageReferencePropertyModel() { }

        public PageReferencePropertyModel(
                    PropertyPageReference propertyContentReference,
                    ConverterContext converterContext) 
            : base(propertyContentReference, converterContext)
        {
        }

        public PageReferencePropertyModel(
                    PropertyPageReference propertyContentReference,
                    ConverterContext converterContext,
                    ContentLoaderService contentLoaderService,
                    ContentConvertingService contentConvertingService,
                    IContentAccessEvaluator accessEvaluator,
                    ISecurityPrincipal principalAccessor,
                    UrlResolverService urlResolverService)
            : base(
                  propertyContentReference,
                  converterContext,
                  contentLoaderService,
                  contentConvertingService,
                  accessEvaluator,
                  principalAccessor,
                  urlResolverService)
        {
        }
    }
}
