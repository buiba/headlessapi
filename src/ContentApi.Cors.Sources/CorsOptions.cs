using System;
using System.Collections.Generic;
using System.Web.Cors;
using EPiServer.Framework;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Cors.Internal
{
    /// <summary>
    /// Defines options used to configure the behavior of the content types API.
    /// </summary>
    [Options]
    internal class CorsOptions
    {
        //Exposed for tests
        internal IDictionary<string, CorsPolicy> Policies { get; set; } = new Dictionary<string, CorsPolicy>(StringComparer.Ordinal);

        /// <summary>
        /// Adds a new policy for a specific authority.
        /// </summary>
        /// <param name="authority">The authority the policy should be applied to.</param>
        /// <param name="policy">The <see cref="CorsPolicy"/> policy to be added.</param>
        public void AddPolicy(string authority, CorsPolicy policy)
        {
            Validator.ThrowIfNullOrEmpty(nameof(authority), authority);
            Validator.ThrowIfNull(nameof(policy), policy);
            Policies[authority] = policy;
        }

        /// <summary>
        /// Gets the policy based on the <paramref name="authority"/>.
        /// </summary>
        /// <remarks>
        /// It will use a policy for specified authority, if no policy is defined for the authority the default policy is returned.
        /// </remarks>
        /// <param name="authority">The name of the policy to lookup.</param>
        /// <returns>The <see cref="CorsPolicy"/> if the policy was added.<c>null</c> otherwise.</returns>
        public CorsPolicy GetPolicy(string authority)
        {
            Validator.ThrowIfNullOrEmpty(nameof(authority), authority);
            if (Policies.TryGetValue(authority, out var policy))
            {
                return policy;
            }

            return null;
        }
    }
}
