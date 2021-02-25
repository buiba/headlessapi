using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web.Http.Controllers;

namespace EPiServer.ContentApi.Core.Security
{
    /// <summary>
    /// Responsible for authorizing user
    /// </summary>
    [ServiceConfiguration(typeof(ContentApiAuthorizationService))]
    public class ContentApiAuthorizationService
    {
        protected readonly RoleService _roleService;
        protected readonly UserService _userService;
        protected readonly ContentApiConfiguration _apiConfig;
        protected readonly ISecurityPrincipal _principalAccessor;
        protected readonly IContentApiSiteFilter _siteFilter;
        protected readonly IContentApiRequiredRoleFilter _requiredRoleFilter;
        protected readonly IContentLoader _contentLoader;

        /// <summary>
        /// Exposed for tests
        /// </summary>
        protected ContentApiAuthorizationService() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentApiAuthorizationService"/> class.
        /// </summary>       
        public ContentApiAuthorizationService(
            ContentApiConfiguration apiConfig, 
            RoleService roleService,
            UserService userService, 
            ISecurityPrincipal principalAccessor) : this (
                apiConfig, 
                roleService, 
                userService, 
                principalAccessor, 
                ServiceLocator.Current.GetInstance<IContentApiSiteFilter>(),
                ServiceLocator.Current.GetInstance<IContentApiRequiredRoleFilter>(),
                ServiceLocator.Current.GetInstance<IContentLoader>())
        {            
		}

        public ContentApiAuthorizationService(
            ContentApiConfiguration apiConfig, 
            RoleService roleService,
            UserService userService, 
            ISecurityPrincipal principalAccessor,
            IContentApiSiteFilter siteFilter,
            IContentApiRequiredRoleFilter requiredRoleFilter,
            IContentLoader contentLoader
            )
        {
            _apiConfig = apiConfig;
            _roleService = roleService;
            _userService = userService;
            _principalAccessor = principalAccessor;
            _siteFilter = siteFilter;
            _requiredRoleFilter = requiredRoleFilter;
            _contentLoader = contentLoader;
        }

        /// <summary>
        /// Authorize user by using Principal object of current http context
        /// </summary>        
        public virtual Tuple<HttpStatusCode, string> Authorize(HttpActionContext actionContext)
        {
            var options = _apiConfig.GetOptions();

            // does not have minimum roles configuration
            if (string.IsNullOrEmpty(options.MinimumRoles))
            {
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.OK, string.Empty);
            }

			var minimumRoles = options.MinimumRoles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
														.Where(r => !string.IsNullOrWhiteSpace(r))?.ToList();

			// there's no valid minimum roles configuration
			if (minimumRoles == null || !minimumRoles.Any())
            {
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.OK, string.Empty);
            }

			// get current principal

			var currentPrincipal = _principalAccessor.GetCurrentPrincipal();
            if (currentPrincipal == null || !currentPrincipal.Identity.IsAuthenticated)
            {                
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.Unauthorized, string.Empty);
            }
            
            if (!IsUserHasMinimumRole(currentPrincipal, minimumRoles))
            {
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.Forbidden, "You are not authorised to access data");                
            }

            return new Tuple<HttpStatusCode, string>(HttpStatusCode.OK, string.Empty); 
        }

        /// <summary>
        /// Check if user has minimum roles or not
        /// Return true if user has at lease one valid minimum role, otherwise return false.        
        /// </summary>       
        protected bool IsUserHasMinimumRole(IPrincipal principal, List<string> minimumRoles)
        {
            foreach (var minRole in minimumRoles)
            {
                // get mapped roles
                var mappedRole = _roleService.GetMappedRolesAssociatedWithVirtualRole(minRole)?.ToList() ?? new List<string> { minRole };
                if (!mappedRole.Contains(minRole))
                {
                    mappedRole.Add(minRole);
                }

				foreach (var role in mappedRole)
				{
					if (principal.IsInRole(role))
					{
						// User must have at least one minimum role in order to hit any method on the Content Api 
						return true;
					}
				}
			}
            return false;
        }

        /// <summary>
        /// Check if given content is valid.
        /// </summary>        
        public virtual bool CanUserAccessContent(IContent content)
            => !ShouldFilterContent(content);

        /// <summary>
        /// Check if given content reference is valid.
        /// </summary> 
        public virtual bool CanUserAccessContent(ContentReference contentReference)
        {
            if (!_contentLoader.TryGet(contentReference, out IContent content))
            {
                return false;
            };

            return !ShouldFilterContent(content);
        }

        /// <summary>
        /// Determines wether anonymous principal has read access to specified content
        /// </summary>
        /// <param name="contentLink">A reference to the content item to check for anonymous access</param>
        /// <returns>True if anonymous has read access else false</returns>
        public virtual bool IsAnonymousAllowedToAccessContent(ContentReference contentLink)
        {
            if (!_contentLoader.TryGet(contentLink, out IContent content))
            {
                return false;
            };

            return IsAnonymousAllowedToAccessContent(content);
        }

        /// <summary>
        /// Determines wether anonymous principal has read access to specified content
        /// </summary>
        /// <param name="content">The content item to check for anonymous access</param>
        /// <returns>True if anonymous has read access else false</returns>
        public virtual bool IsAnonymousAllowedToAccessContent(IContent content) => _userService.IsUserAllowedToAccessContent(content, _principalAccessor.GetAnonymousPrincipal(), AccessLevel.Read);

        /// <summary>
        /// Check whether a content should be filtered by the SiteFilter, RequiredRoleFilter and the current user is not allowed to read.
        /// </summary>
        /// <param name="content">The content to check.</param>
        /// <returns> 
        ///     True if the given content should be filtered by the SiteFilter, RequiredRoleFilter and the current user is not allowed to read,
        ///     False otherwise.
        /// </returns>
        private bool ShouldFilterContent(IContent content)
        {
            return content == null ||
                _siteFilter.ShouldFilterContent(content, SiteDefinition.Current) ||
                _requiredRoleFilter.ShouldFilterContent(content) ||
                !_userService.IsUserAllowedToAccessContent(content, _principalAccessor.GetCurrentPrincipal(), AccessLevel.Read);
        }
    }
}
