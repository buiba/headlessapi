using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Castle.Core.Internal;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Inventory;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce.Catalog;

namespace EPiServer.ContentApi.Commerce.Internal.Controller
{
    [RoutePrefix(RouteConstants.VersionTwoApiRoute + "inventory")]
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [ApiExceptionFilter]
    public class InventoryApiController : ApiController
    {
        private readonly IContentApiRequiredRoleFilter _requiredRoleFilter;
        private readonly UserService _userService;
        private readonly ContentLoaderService _contentLoaderService;
        private readonly IContentApiSiteFilter _siteFilter;
        private readonly InventoryService _inventoryService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly ISecurityPrincipal _principalAccessor;

        public InventoryApiController() : this(
            ServiceLocator.Current.GetInstance<IContentApiRequiredRoleFilter>(),
            ServiceLocator.Current.GetInstance<UserService>(),
            ServiceLocator.Current.GetInstance<ContentLoaderService>(),
            ServiceLocator.Current.GetInstance<IContentApiSiteFilter>(),
            ServiceLocator.Current.GetInstance<InventoryService>(),
            ServiceLocator.Current.GetInstance<ReferenceConverter>(),
            ServiceLocator.Current.GetInstance<ISecurityPrincipal>())
        { }

        public InventoryApiController(
            IContentApiRequiredRoleFilter requiredRoleFilter, 
            UserService userService, 
            ContentLoaderService contentLoaderService, 
            IContentApiSiteFilter siteFilter,
            InventoryService inventoryService, 
            ReferenceConverter referenceConverter, 
            ISecurityPrincipal principalAccessor)
        {
            _requiredRoleFilter = requiredRoleFilter;
            _userService = userService;
            _contentLoaderService = contentLoaderService;
            _siteFilter = siteFilter;
            _inventoryService = inventoryService;
            _referenceConverter = referenceConverter;
            _principalAccessor = principalAccessor;
        }

        /// <summary>
        /// Gets sku inventory information.
        /// </summary>
        /// <param name="contentId">The content id.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(IEnumerable<InventoryApiModel>))]
        public IHttpActionResult Get(Guid contentId)
        {
            if (contentId == Guid.Empty)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidHeaderValue, HttpStatusCode.BadRequest);
            }

            IContent content;
            try
            {
                content = _contentLoaderService.Get(contentId, string.Empty);
            }
            catch (ContentNotFoundException)
            {
                return new ContentApiErrorResult(ApiErrors.NotFound, HttpStatusCode.NotFound);
            }

            if (!(content is CatalogContentBase) || _siteFilter.ShouldFilterContent(content, SiteDefinition.Current))
            {
                return new ContentApiErrorResult(ApiErrors.NotFound, HttpStatusCode.NotFound);
            }

            if (_requiredRoleFilter.ShouldFilterContent(content) || !_userService.IsUserAllowedToAccessContent(content, _principalAccessor.GetCurrentPrincipal(), AccessLevel.Read))
            {
                return new ContentApiErrorResult(ApiErrors.Forbidden, HttpStatusCode.Forbidden);
            }

            var entryCode = _referenceConverter.GetCode(content.ContentLink);
            if (entryCode.IsNullOrEmpty())
            {
                return new ContentApiErrorResult(ApiErrors.NotFound, HttpStatusCode.NotFound);
            }

            return new ContentApiResult<IEnumerable<InventoryApiModel>>(_inventoryService.GetInventories(entryCode), HttpStatusCode.OK);            
        }
    }
}
