using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.Framework;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Configuration
{
    /// <summary>
    /// Defines options used to configure the behavior of the Content Management API.
    /// </summary>
    [Options]
    public class ContentManagementApiOptions : IApiAuthorizationOptions
    {
        private const string DefaultScope = "epi_content_management";

        /// <summary>
        /// Creates a new instance of <see cref="ContentManagementApiOptions"/>
        /// </summary>
        public ContentManagementApiOptions()
        {
            AllowedScopes.Add(DefaultScope);
        }

        string IApiAuthorizationOptions.Name => "Content Management API";

        /// <summary>
        /// Gets or sets the scope claim type.
        /// </summary>
        /// <remarks>Default is 'scope'.</remarks>
        public string ScopeClaimType { get; set; } = "scope";

        /// <summary>
        /// Gets the scopes that are allowed to call the API endpoints.
        /// </summary>
        /// <remarks>Default is 'epi_content_management'.</remarks>
        public ICollection<string> AllowedScopes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The required role that must be assigned to content in order for it to be accessable in Content Management API.        
        /// </summary>
        public string RequiredRole { get; internal set; } = "contentapiwrite";

        /// <summary>
        /// Set the required roles.        
        /// </summary>
        public ContentManagementApiOptions SetRequiredRole(string requiredRole)
        {
            RequiredRole = requiredRole;
            return this;
        }

        /// <summary>
        /// Adds an allowed scope.
        /// </summary>
        /// <param name="scope">The scope to add.</param>
        public virtual ContentManagementApiOptions AddAllowedScope(string scope)
        {
            Validator.ThrowIfNullOrEmpty(nameof(scope), scope);
            AllowedScopes.Add(scope);
            return this;
        }

        /// <summary>
        /// Removes an allowed scope.
        /// </summary>
        /// <param name="scope">The scope to remove.</param>
        public virtual ContentManagementApiOptions RemoveAllowedScope(string scope)
        {
            Validator.ThrowIfNullOrEmpty(nameof(scope), scope);
            AllowedScopes.Remove(scope);
            return this;
        }

        /// <summary>
        /// Clears all allowed scopes.
        /// </summary>
        public virtual ContentManagementApiOptions ClearAllowedScopes()
        {
            AllowedScopes.Clear();
            return this;
        }

        /// <summary>
        /// Clone object
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
