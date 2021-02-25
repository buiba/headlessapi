using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Obsolete.Serialization;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// base class for mapping between property models and property data based on PropertyContentReferenceList 
    /// </summary>
    public partial class ContentReferenceListPropertyModelBase<T, U> : PersonalizablePropertyModel<T, U>, IExpandableProperty<IEnumerable<ContentApiModel>>
        where T : List<ContentModelReference>
        where U : PropertyContentReferenceList
    {

        protected  IContentModelMapper _contentModelMapper;

        public ContentReferenceListPropertyModelBase(
            U PropertyContentReferenceList
            ,bool excludePersonalizedContent) : this(
          PropertyContentReferenceList,
          excludePersonalizedContent,
          ServiceLocator.Current.GetInstance<IPermanentLinkMapper>(),
          ServiceLocator.Current.GetInstance<ContentLoaderService>(),
          ServiceLocator.Current.GetInstance<IContentModelMapper>(),
          ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
          ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
          ServiceLocator.Current.GetInstance<UrlResolverService>()
        )
        {

        }

        [Obsolete("Use alternative constructor")]
        public ContentReferenceListPropertyModelBase(
            U propertyContentReferenceList,
            bool excludePersonalizedContent,
            IPermanentLinkMapper linkMapper,
            ContentLoaderService contentLoaderService,
            IContentModelMapper contentModelMapper,
            IContentAccessEvaluator accessEvaluator,
            ISecurityPrincipal principalAccessor)
            : this(propertyContentReferenceList,
                  excludePersonalizedContent,
                  linkMapper,
                  contentLoaderService,
                  contentModelMapper,
                  accessEvaluator,
                  principalAccessor,
                  ServiceLocator.Current.GetInstance<UrlResolverService>())
        {

        }

        public ContentReferenceListPropertyModelBase(
            U propertyContentReferenceList,
            bool excludePersonalizedContent,
            IPermanentLinkMapper linkMapper,
            ContentLoaderService contentLoaderService,
            IContentModelMapper contentModelMapper,
            IContentAccessEvaluator accessEvaluator,
            ISecurityPrincipal principalAccessor,
            UrlResolverService urlResolverService)
            : this(propertyContentReferenceList, ConverterContextFactory.ForObsolete(excludePersonalizedContent), 
                linkMapper, contentLoaderService, ServiceLocator.Current.GetInstance<ContentConvertingService>(), accessEvaluator, principalAccessor, urlResolverService)
        {
            _contentModelMapper = contentModelMapper;
        }

        private void InitializeObsolete()
        {
            if (_contentModelMapper == null)
            {
                //Do TryGetExistingInstance to not break unit tests
                ServiceLocator.Current.TryGetExistingInstance<IContentModelMapper>(out _contentModelMapper);
            }
        }        
    }
}
