using System.Collections.Generic;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for PropertyContentReferenceList
    /// </summary>
    public partial class ContentReferenceListPropertyModel : ContentReferenceListPropertyModelBase<List<ContentModelReference>, PropertyContentReferenceList>
    {
        [JsonConstructor]
        internal ContentReferenceListPropertyModel()
        {
        }

        public ContentReferenceListPropertyModel(
              PropertyContentReferenceList PropertyContentReferenceList
            , ConverterContext convertercontext) : base(PropertyContentReferenceList, convertercontext)
        {

        }

        public ContentReferenceListPropertyModel(
          PropertyContentReferenceList propertyContentReferenceList,
          ConverterContext converterContext,
          IPermanentLinkMapper linkMapper,
          ContentLoaderService contentLoaderService,
          ContentConvertingService contentConvertingService,
          IContentAccessEvaluator accessEvaluator,
		  ISecurityPrincipal principalAccessor,
          UrlResolverService urlResolverService) 
            : base(
                  propertyContentReferenceList,
                  converterContext,
                  linkMapper,
                  contentLoaderService,
                  contentConvertingService,
                  accessEvaluator,
                  principalAccessor, 
                  urlResolverService)
        {}
    }
}
