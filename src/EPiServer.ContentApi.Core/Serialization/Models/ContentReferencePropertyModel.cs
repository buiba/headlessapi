using EPiServer.ContentApi.Core.Security;
using EPiServer.Core;
using EPiServer.Security;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyContentReference"/>
    /// </summary>
    public partial class ContentReferencePropertyModel : ContentReferencePropertyModelBase<ContentModelReference, PropertyContentReference>
    {
        [JsonConstructor]
        internal ContentReferencePropertyModel() { }

        public ContentReferencePropertyModel(
            PropertyContentReference propertyContentReference,
           ConverterContext converterContext): base(propertyContentReference, converterContext)
        {

        }

        public ContentReferencePropertyModel(PropertyContentReference propertyContentReference,
                                             ConverterContext converterContext,
                                             ContentLoaderService contentLoaderService,
                                             ContentConvertingService contentConvertingService,
                                             IContentAccessEvaluator accessEvaluator,
                                             ISecurityPrincipal principalAccessor,
                                             UrlResolverService urlResolverService)
            : base(propertyContentReference,
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
