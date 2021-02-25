using System.Collections.Generic;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using System;
using EPiServer.ContentApi.Core.Obsolete.Serialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for PropertyContentReferenceList
    /// </summary>
    public partial class ContentReferenceListPropertyModel : ContentReferenceListPropertyModelBase<List<ContentModelReference>, PropertyContentReferenceList>
    {
        public ContentReferenceListPropertyModel(
              PropertyContentReferenceList PropertyContentReferenceList
            , bool excludePersonalizedContent) 
            : this(PropertyContentReferenceList, ConverterContextFactory.ForObsolete(excludePersonalizedContent))
        {

        }


        [Obsolete("Use altervative constructor")]
        public ContentReferenceListPropertyModel(
          PropertyContentReferenceList propertyContentReferenceList,
          bool excludePersonalizedContent,
          IPermanentLinkMapper linkMapper,
          ContentLoaderService contentLoaderService,
          IContentModelMapper contentModelMapper,
          IContentAccessEvaluator accessEvaluator,
		  ISecurityPrincipal principalAccessor) 
            : this(
                  propertyContentReferenceList,
                  excludePersonalizedContent,
                  linkMapper,
                  contentLoaderService,
                  contentModelMapper,
                  accessEvaluator,
                  principalAccessor,
                  ServiceLocator.Current.GetInstance<UrlResolverService>())
        {

        }

        public ContentReferenceListPropertyModel(PropertyContentReferenceList propertyContentReferenceList,
                                                 bool excludePersonalizedContent,
                                                 IPermanentLinkMapper linkMapper,
                                                 ContentLoaderService contentLoaderService,
                                                 IContentModelMapper contentModelMapper,
                                                 IContentAccessEvaluator accessEvaluator,
                                                 ISecurityPrincipal principalAccessor,
                                                 UrlResolverService urlResolverService)
            : this(propertyContentReferenceList,
                   ConverterContextFactory.ForObsolete(excludePersonalizedContent),
                   linkMapper,
                   contentLoaderService,
                   ServiceLocator.Current.GetInstance<ContentConvertingService>(),
                   accessEvaluator,
                   principalAccessor,
                   urlResolverService)
        {
        }        
    }
}
