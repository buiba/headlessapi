using System;
using System.Web.Http;
using EPiServer.ContentApi.Cms.Internal;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Cms.Controllers
{
    /// <summary>
    ///     Controller for returning requests for IContent from IContentLoader, with appropriate filtering
    /// </summary>
    public partial class ContentApiController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentApiController"/> class.
        /// </summary>
        [Obsolete("Use alternative constructor")]
        public ContentApiController(
            IContentModelMapper contentModelMapper,
            ContentLoaderService contentLoaderService,
            IContentApiSiteFilter siteFilter,
            IContentApiRequiredRoleFilter requiredRoleFilter,
            ISecurityPrincipal principalAccessor,
            UserService userService)
            : this(ServiceLocator.Current.GetInstance<IContentModelMapperFactory>(), contentLoaderService, siteFilter, requiredRoleFilter, principalAccessor, userService, ServiceLocator.Current.GetInstance<ContentApiAuthorizationService>())
        { }


        /// <summary>
        /// Initializes a new instance of the <see cref="ContentApiController"/> class.
        /// </summary>
        [Obsolete("Use alternative constructor")]
        public ContentApiController(
            IContentModelMapperFactory contentModelFactory,
            ContentLoaderService contentLoaderService,
            IContentApiSiteFilter siteFilter,
            IContentApiRequiredRoleFilter requiredRoleFilter,
            ISecurityPrincipal principalAccessor,
            UserService userService)
            : this(contentModelFactory, contentLoaderService, siteFilter, requiredRoleFilter, principalAccessor, userService, ServiceLocator.Current.GetInstance<ContentApiAuthorizationService>())
        { }

        [Obsolete("Use alternative constructor")]
        public ContentApiController(
            IContentModelMapperFactory contentModelFactory,
            ContentLoaderService contentLoaderService,
            IContentApiSiteFilter siteFilter,
            IContentApiRequiredRoleFilter requiredRoleFilter,
            ISecurityPrincipal principalAccessor,
            UserService userService,
            ContentApiAuthorizationService authorizationService)
            : this(ServiceLocator.Current.GetInstance<ContentConvertingService>(), contentLoaderService, siteFilter, requiredRoleFilter, principalAccessor, userService, authorizationService, ServiceLocator.Current.GetInstance<OptionsResolver>(), ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        { }


        [Obsolete("Use alternative constructor")]
        public ContentApiController(
           ContentConvertingService contentConvertingService,
           ContentLoaderService contentLoaderService,
           IContentApiSiteFilter siteFilter,
           IContentApiRequiredRoleFilter requiredRoleFilter,
           Core.Security.ISecurityPrincipal principalAccessor,
           UserService userService,
           ContentApiAuthorizationService authorizationService,
           OptionsResolver optionsResolver,
           ContentApiSerializerResolver contentSerializerResolver)
        : this(contentConvertingService,
              contentLoaderService,
              siteFilter,
              requiredRoleFilter,
              principalAccessor,
              ServiceLocator.Current.GetInstance<Core.IContextModeResolver>(),
              userService,
              authorizationService,
              optionsResolver,
              contentSerializerResolver,
              ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>())
        { }

        [Obsolete("Use alternative constructor")]
        public ContentApiController(
           ContentConvertingService contentConvertingService,
           ContentLoaderService contentLoaderService,
           IContentApiSiteFilter siteFilter,
           IContentApiRequiredRoleFilter requiredRoleFilter,
           ISecurityPrincipal principalAccessor,
           Core.IContextModeResolver contextModeResolver,
           UserService userService,
           ContentApiAuthorizationService authorizationService,
           OptionsResolver optionsResolver,
           ContentApiSerializerResolver contentSerializerResolver,
           IContentApiTrackingContextAccessor contentApiTrackingContextAccessor)
            : this(contentConvertingService, contentLoaderService, siteFilter, requiredRoleFilter, principalAccessor,
                   contextModeResolver, userService, authorizationService, ServiceLocator.Current.GetInstance<ContentApiConfiguration>(), contentSerializerResolver,
                   contentApiTrackingContextAccessor,
                   ServiceLocator.Current.GetInstance<ContentResolver>(),
                   ServiceLocator.Current.GetInstance<ISiteDefinitionResolver>(),
                   ServiceLocator.Current.GetInstance<IContentLanguageAccessor>(),
                   ServiceLocator.Current.GetInstance<IUpdateCurrentLanguage>())
        { }
    }
}
