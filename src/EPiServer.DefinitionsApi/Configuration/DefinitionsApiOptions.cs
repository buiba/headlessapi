using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.Framework;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.Configuration
{
    /// <summary>
    /// Defines options used to configure the behavior of the Definitions API.
    /// </summary>
    [Options]
    public class DefinitionsApiOptions : IApiAuthorizationOptions
    {
        private const string DefaultScope = "epi_definitions";

        /// <summary>
        /// Creates a new instance of <see cref="DefinitionsApiOptions"/>
        /// </summary>
        public DefinitionsApiOptions()
        {
            AllowedScopes.Add(DefaultScope);
        }

        string IApiAuthorizationOptions.Name => "Definition API";

        /// <summary>
        /// Gets or sets the scope claim type.
        /// </summary>
        /// <remarks>Default is 'scope'.</remarks>
        public string ScopeClaimType { get; set; } = "scope";

        /// <summary>
        /// Gets the scopes that are allowed to call the API endpoints.
        /// </summary>
        /// <remarks>Default is 'epi_definitions'.</remarks>
        public ICollection<string> AllowedScopes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds an allowed scope.
        /// </summary>
        /// <param name="scope">The scope to add.</param>
        public virtual DefinitionsApiOptions AddAllowedScope(string scope)
        {
            Validator.ThrowIfNullOrEmpty(nameof(scope), scope);
            AllowedScopes.Add(scope);
            return this;
        }

        /// <summary>
        /// Removes an allowed scope.
        /// </summary>
        /// <param name="scope">The scope to remove.</param>
        public virtual DefinitionsApiOptions RemoveAllowedScope(string scope)
        {
            Validator.ThrowIfNullOrEmpty(nameof(scope), scope);
            AllowedScopes.Remove(scope);
            return this;
        }

        /// <summary>
        /// Clears all allowed scopes.
        /// </summary>
        public virtual DefinitionsApiOptions ClearAllowedScopes()
        {
            AllowedScopes.Clear();
            return this;
        }
    }
}
