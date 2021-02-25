using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentManagementApi.Configuration;
using EPiServer.Core;
using EPiServer.Security;

namespace EPiServer.ContentManagementApi.Internal
{
    /// <summary>
    /// Component responsible for evaluating if the content is exposed to Content Management API.
    /// </summary>
    public class RequiredRoleEvaluator
    {
        private readonly RoleService _roleService;
        private readonly ContentManagementApiOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredRoleEvaluator"/> class.
        /// </summary>
        public RequiredRoleEvaluator(RoleService roleService, ContentManagementApiOptions options)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _options = options;
        }

        /// <summary>
        /// Exposed for testing purpose.
        /// </summary>
        protected RequiredRoleEvaluator(){}

        /// <summary>
        /// Evaluates if the content is exposed to Content Management API.
        /// </summary>
        /// <param name="content">content.</param>
        /// <returns><c>true</c> if the content is exposed to the required role (contentapiwrite).</returns>
        public virtual bool HasAccess(IContent content)
        {
            if (content is null)
            {
                throw new ArgumentNullException("The provided content cannot be null.", nameof(content));
            }

            var requiredRole = _options.RequiredRole;
            if (string.IsNullOrWhiteSpace(requiredRole))
            {
                return true;
            }

            var entries = GetContentSecurityDescriptor(content)?.Entries;
            if (entries is null)
            {
                return true;
            }

            var requiredAccessControlEntry = new AccessControlEntry(requiredRole, AccessLevel.Read, SecurityEntityType.Role);
            var mappedRole = _roleService.GetMappedRolesAssociatedWithVirtualRole(requiredAccessControlEntry.Name)?.ToList()
                            ?? new List<string>();
            mappedRole.Add(requiredAccessControlEntry.Name);

            return entries.Any(entry => mappedRole.Any(m => m.Equals(entry.Name, StringComparison.OrdinalIgnoreCase))
                                        && entry.Access.HasFlag(requiredAccessControlEntry.Access));
        }

        private static IContentSecurityDescriptor GetContentSecurityDescriptor(IContent content) => (content as IContentSecurable)?.GetContentSecurityDescriptor();
    }
}
