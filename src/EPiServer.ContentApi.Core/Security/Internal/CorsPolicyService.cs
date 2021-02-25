using EPiServer.ContentApi.Core.Configuration;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;

namespace EPiServer.ContentApi.Core.Security.Internal
{
    [ServiceConfiguration(typeof(CorsPolicyService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CorsPolicyService
    {
        private readonly ContentApiConfiguration _apiConfiguration;
        private readonly ISiteDefinitionResolver _siteDefinitionResolver;
        private readonly ILanguageBranchRepository _languageBranchRepository;
        internal readonly ConcurrentDictionary<string, CorsPolicy> _cache = new ConcurrentDictionary<string, CorsPolicy>();        

        public CorsPolicyService(ContentApiConfiguration apiConfiguration, ISiteDefinitionResolver siteDefinitionResolver, ILanguageBranchRepository languageBranchRepository) 
        {
            _apiConfiguration = apiConfiguration;
            _siteDefinitionResolver = siteDefinitionResolver;
            _languageBranchRepository = languageBranchRepository;            
        }

        public virtual CorsPolicy GetOrCreatePolicy(HttpRequestMessage request) => _cache.GetOrAdd(request.RequestUri.Authority, (authority) => GetOrCreatePolicy(authority));

        public virtual void ClearCache(object sender, EventArgs e) => _cache.Clear();

        private CorsPolicy GetOrCreatePolicy(string authority)
        {
            var apiConfig = _apiConfiguration.Default();
            var allowAnyOrigin = AllClientsHaveWildcardOrigin(apiConfig);
            var policy = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = allowAnyOrigin,
                SupportsCredentials = !allowAnyOrigin,
                // We need to explicit set the `Access-Control-Expose-Headers` in the CORS response,
                // otherwise the browser/client cannot access these header in the response.
                ExposedHeaders =
                {
                    PagingConstants.ContinuationTokenHeaderName,
                    MetadataHeaderConstants.BranchMetadataHeaderName,
                    MetadataHeaderConstants.ContentGUIDMetadataHeaderName,
                    MetadataHeaderConstants.ContextModeMetadataHeaderName,
                    MetadataHeaderConstants.RemainingRouteMetadataHeaderName,
                    MetadataHeaderConstants.SiteIdMetadataHeaderName,
                    MetadataHeaderConstants.StartPageMetadataHeaderName
                }
            };

            // No need to extract origins if clients config this way
            // guarantee back-ward compability
            if (allowAnyOrigin)
            {
                return policy;
            }

            policy = ExtractOriginsFromConfiguration(policy, apiConfig);

            // Use this api instead of _siteDefinitionResolver.GetByHostname(host, fallback) (an extension method) so that we can mock more easily
            var site = _siteDefinitionResolver.GetByHostname(authority, false, out _);
            if (site == null)
            {
                return policy;
            }

            var languages = _languageBranchRepository.ListEnabled();
            foreach (var host in site.Hosts.Where(h => h.Type == HostDefinitionType.Primary || (h.Type == HostDefinitionType.Undefined && h.Name != "*")))
            {
                if ((host.Language == null) || languages.Any(lang =>string.Equals(lang.LanguageID, host.Language.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    policy.Origins.Add(VirtualPathUtilityEx.RemoveTrailingSlash(host.Url.AbsoluteUri));
                }
            }

            return policy;
        }

        private CorsPolicy ExtractOriginsFromConfiguration(CorsPolicy policy, ContentApiOptions apiConfig)
        {
            if (apiConfig.Clients == null || !apiConfig.Clients.Any())
            {
                return policy;
            }

            var clientOrigins = apiConfig.Clients.Select(x => x.AccessControlAllowOrigin).Distinct().ToList();
            clientOrigins.ForEach(x => policy.Origins.Add(x));
            return policy;
        }

        private bool AllClientsHaveWildcardOrigin(ContentApiOptions options)
        {
            return (options.Clients != null && options.Clients.Any()) ? options.Clients.All(x => x.AccessControlAllowOrigin == "*") : false;
        }
    }
}
