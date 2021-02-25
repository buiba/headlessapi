using EPiServer.ContentApi.Core.Obsolete.Serialization;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System;
using System.Globalization;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// base class for mapping between property models and property data based on PropertyContentReference 
    /// </summary>
    public partial class ContentReferencePropertyModelBase<T, U> : PersonalizablePropertyModel<T, U>,
                                                         IExpandableProperty<ContentApiModel>
                                                        where T : ContentModelReference
                                                        where U : PropertyContentReference
    {

        protected IContentModelMapper _contentModelMapper;
        protected bool _excludePersonalizedContent;

        public ContentReferencePropertyModelBase(U propertyContentReference, bool excludePersonalizedContent)
            : this(propertyContentReference,
                   excludePersonalizedContent,
                   ServiceLocator.Current.GetInstance<IContentLoader>(),
                   ServiceLocator.Current.GetInstance<IContentModelMapper>(),
                   ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
                   ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
                   ServiceLocator.Current.GetInstance<UrlResolverService>())
        {
        }

        [Obsolete("Use alternative constructor")]
        public ContentReferencePropertyModelBase(U propertyContentReference,
                                                 bool excludePersonalizedContent,
                                                 IContentLoader contentLoader,
                                                 IContentModelMapper contentModelMapper,
                                                 IContentAccessEvaluator accessEvaluator,
                                                 ISecurityPrincipal principalAccessor)
            : this(propertyContentReference,
                   excludePersonalizedContent,
                   contentLoader,
                   contentModelMapper,
                   accessEvaluator,
                   principalAccessor,
                   ServiceLocator.Current.GetInstance<UrlResolverService>())
        {
        }

        public ContentReferencePropertyModelBase(
            U propertyContentReference,
            bool excludePersonalizedContent,
            IContentLoader contentLoader,
            IContentModelMapper contentModelMapper,
            IContentAccessEvaluator accessEvaluator,
            ISecurityPrincipal principalAccessor,
            UrlResolverService urlResolverService)
            : this(propertyContentReference, ConverterContextFactory.ForObsolete(true),
                   ServiceLocator.Current.GetInstance<IContentLoader>(),
                   ServiceLocator.Current.GetInstance<ContentConvertingService>(),
                   ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
                   ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
                   ServiceLocator.Current.GetInstance<UrlResolverService>())
        {
            _contentModelMapper = contentModelMapper;
        }

        public ContentReferencePropertyModelBase(
           U propertyContentReference,
           ConverterContext converterContext,
           IContentLoader contentLoader,
           ContentConvertingService contentConvertingService,
           IContentAccessEvaluator accessEvaluator,
           ISecurityPrincipal principalAccessor,
           UrlResolverService urlResolverService)
            : this(propertyContentReference,
                   converterContext,
                 ServiceLocator.Current.GetInstance<ContentLoaderService>(),
                   ServiceLocator.Current.GetInstance<ContentConvertingService>(),
                   ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
                   ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
                   ServiceLocator.Current.GetInstance<UrlResolverService>())
        {
            _contentLoader = contentLoader;
        }

        private void InitializeObsolete()
        {
            if (_contentModelMapper == null)
            {
                //Do TryGetExistingInstance to not break unit tests
                ServiceLocator.Current.TryGetExistingInstance<IContentModelMapper>(out _contentModelMapper);
            }

            if (_contentLoader == null)
            {
                //Do TryGetExistingInstance to not break unit tests
                ServiceLocator.Current.TryGetExistingInstance<IContentLoader>(out _contentLoader);
            }
            _excludePersonalizedContent = ConverterContext.ExcludePersonalizedContent;
        }       
    }
}

