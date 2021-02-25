using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Security.Internal
{
    /// <summary>
    ///     Interface for a filter which ensures that the Content Api Required Role setting is enforced
    /// </summary>
    [ServiceConfiguration(typeof(IContentApiRequiredRoleFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentApiRequiredRoleFilter : IContentApiRequiredRoleFilter
    {
        protected readonly ContentApiConfiguration _apiConfiguration;
        protected readonly RoleService _roleService;
        protected static readonly ILogger _logger = LogManager.GetLogger(typeof(ContentApiRequiredRoleFilter));

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentApiRequiredRoleFilter"/> class.
        /// </summary>
        public ContentApiRequiredRoleFilter(RoleService roleService, ContentApiConfiguration apiConfiguration)
        {
            _roleService = roleService;
            _apiConfiguration = apiConfiguration;
        }

        // Used when mocking in tests only.
        public ContentApiRequiredRoleFilter()
        {}

        /// <summary>
        /// Based on the provided IEnumerable of IContent, return an List of IContent 
        /// that includes only instances of IContent that have the required role attached
        /// </summary>
        /// <param name="content">IEnumable of IContent to filter</param>
        /// <returns>List of content that were not filtered out</returns>
        public virtual IEnumerable<IContent> FilterContents(IEnumerable<IContent> content)
        {
            return content.Where(x => x != null && !ShouldFilterContent(x)).ToList();
        }

        /// <summary>
        /// Based on the provided IContent instance, return a bool indicating if the provided content should be filtered out.
        /// </summary>
        /// <param name="content">IContent filter</param>
        /// <returns>true if the Content should be filtered, false if it should not</returns>
        public virtual bool ShouldFilterContent(IContent content)
        {
            if (content == null)
            {
                var exception = new ArgumentException("Provided content cannot be null", nameof(content));
                _logger.Error("Provided content cannot be null", exception);
                throw exception;
            }

            var role = _apiConfiguration.Default().RequiredRole;

            // If the required role is not setup, so no need for filter
            if (string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            var contentSecurityDescriptor = GetContentSecurityDescriptor(content);

            var entries = contentSecurityDescriptor?.Entries;
            if (entries == null)
            {
                return false;
            }
            var requiredAccessControlEntry = new AccessControlEntry(role, AccessLevel.Read, SecurityEntityType.Role);
            var mappedRole =   _roleService.GetMappedRolesAssociatedWithVirtualRole(requiredAccessControlEntry.Name) ?.ToList()
                            ?? new List<string> ();
            mappedRole.Add(requiredAccessControlEntry.Name);

			return !entries.Any(entry => mappedRole.Any(m => m.Equals(entry.Name, StringComparison.OrdinalIgnoreCase))
										&& entry.Access.HasFlag(requiredAccessControlEntry.Access));
		}

        protected virtual IContentSecurityDescriptor GetContentSecurityDescriptor(IContent content)
        {
            return (content as IContentSecurable)?.GetContentSecurityDescriptor();
        }
    }
}
