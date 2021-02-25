using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Cors;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.IntegrationTests.TestSetup
{
    public class CorsOptionsScope : IDisposable
    {
        private readonly CorsOptions _corsOptions;
        private readonly SiteBasedCorsPolicyService _corsService;
        private readonly IDictionary<string, CorsPolicy> _currentPolicies;

        public CorsOptionsScope(string authority = null, CorsPolicy corsPolicy = null)
        {
            _corsOptions = ServiceLocator.Current.GetInstance<CorsOptions>();
            _corsService = ServiceLocator.Current.GetInstance<SiteBasedCorsPolicyService>();

            _currentPolicies = _corsOptions.Policies;

            _corsOptions.Policies = new Dictionary<string, CorsPolicy>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(authority))
            {
                _corsOptions.Policies.Add(authority, corsPolicy);
            }
        }

        public void Dispose()
        {
            _corsOptions.Policies = _currentPolicies;
            _corsService.CachePolicies = new ConcurrentDictionary<string, CorsPolicy>();
        }
    }
}
