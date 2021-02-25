using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Cors.Internal
{
    /// <summary>
    ///     Handle CORS policy for sites
    /// </summary>
    [ServiceConfiguration(typeof(SiteBasedCorsPolicyService), Lifecycle = ServiceInstanceScope.Singleton, IncludeServiceAccessor = false)]
    internal class SiteBasedCorsPolicyService

    {
        private readonly ISiteDefinitionResolver _siteDefinitionResolver;
        private readonly ILanguageBranchRepository _languageBranchRepository;
        private readonly CorsOptions _corsOptions;

        /// <summary>
        ///     Cache policies
        /// </summary>
        internal ConcurrentDictionary<string, CorsPolicy> CachePolicies { get; set; } = new ConcurrentDictionary<string, CorsPolicy>();

        /// <summary>
        ///     Initialize an new instance of CorsPolicyService
        /// </summary>
        /// <param name="siteDefinitionResolver">site definition resolver</param>
        /// <param name="languageBranchRepository">language branch repository</param>
        /// <param name="corsOptions">cors options</param>
        public SiteBasedCorsPolicyService(ISiteDefinitionResolver siteDefinitionResolver, ILanguageBranchRepository languageBranchRepository, CorsOptions corsOptions)
        {
            _siteDefinitionResolver = siteDefinitionResolver;
            _languageBranchRepository = languageBranchRepository;
            _corsOptions = corsOptions;
        }

        /// <summary>
        ///     Get or create cors policy
        /// </summary>
        /// <param name="request">http request message</param>
        public virtual CorsPolicy GetOrCreatePolicy(HttpRequestMessage request) => CachePolicies.GetOrAdd(key: request.RequestUri.Authority, (authority) => GetOrCreatePolicy(authority));

        /// <summary>
        /// Clear cors policy
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">EventArgs</param>
        public virtual void ClearCache(object sender, EventArgs e) => CachePolicies.Clear();

        private CorsPolicy GetOrCreatePolicy(string authority)
        {
            var allowAnyOrigin = AllClientsHaveWildcardOrigin();
            var policy = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = allowAnyOrigin,
                SupportsCredentials = !allowAnyOrigin
            };

            if (allowAnyOrigin)
            {
                return policy;
            }

            // get policy from configuration
            var configPolicies = _corsOptions.GetPolicy(authority);
            if (configPolicies != null)
            {
                configPolicies.Origins.ToList().ForEach(x => policy.Origins.Add(x));
            }

            var site = _siteDefinitionResolver.GetByHostname(authority, false, out _);
            if (site == null)
            {
                return policy;
            }

            var languages = _languageBranchRepository.ListEnabled();
            foreach (var host in site.Hosts.Where(h => h.Type == HostDefinitionType.Primary || (h.Type == HostDefinitionType.Undefined && h.Name != "*")))
            {
                if ((host.Language == null) || languages.Any(lang => string.Equals(lang.LanguageID, host.Language.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    policy.Origins.Add(VirtualPathUtilityEx.RemoveTrailingSlash(host.Url.AbsoluteUri));
                }
            }

            return policy;
        }

        private bool AllClientsHaveWildcardOrigin()
        {
            return _corsOptions.Policies != null &&_corsOptions.Policies.Any() && _corsOptions.Policies.All(x => x.Value.AllowAnyOrigin);
        }
    }
}
