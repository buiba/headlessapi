using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Obsolete.Serialization;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization.Models
{

    /// <summary>
    /// propertymodel base for PropertyLongString
    /// </summary>
    public partial class CollectionPropertyModelBase<T, U> : PersonalizablePropertyModel<IEnumerable<T>, U>, IExpandableProperty<IEnumerable<ContentApiModel>>
                                                 where T : IContentItem
                                                 where U : PropertyLongString
    {
        protected IContentModelMapper _contentModelMapper;
        protected bool _excludePersonalizedContent;

        public CollectionPropertyModelBase(U propertyLongString, bool excludePersonalizedContent)
            : this(
                 propertyLongString,
                 excludePersonalizedContent,
                 ServiceLocator.Current.GetInstance<ContentLoaderService>(),
                 ServiceLocator.Current.GetInstance<IContentModelMapper>(),
                 ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
                 ServiceLocator.Current.GetInstance<ISecurityPrincipal>())
        { }

        public CollectionPropertyModelBase(
            U propertyLongString,
            bool excludePersonalizedContent,
            ContentLoaderService contentLoaderService,
            IContentModelMapper contentModelMapper,
            IContentAccessEvaluator accessEvaluator,
            ISecurityPrincipal principalAccessor) : this(propertyLongString, ConverterContextFactory.ForObsolete(excludePersonalizedContent))
        {}


        private void InitializeObsolete()
        {
            if (_contentModelMapper == null)
            {
                //Do TryGetExistingInstance to not break unit tests
                ServiceLocator.Current.TryGetExistingInstance<IContentModelMapper>(out _contentModelMapper);
            }
            _excludePersonalizedContent = ConverterContext.ExcludePersonalizedContent;
        }
    }
}
