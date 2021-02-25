using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Core.Security.Internal
{
    /// <summary>
    ///     Attribute which implements <see cref="ICorsPolicyProvider"/> in order to configure allowed origins within the Content Api.
    /// </summary>
    public class ContentApiCorsAttribute : Attribute, ICorsPolicyProvider
    {
        /// <summary>
        ///  Get Cors policy to verify access control origin 
        /// </summary>
        private Injected<CorsPolicyService> _corsPolicyService;
        

        /// <summary>
        ///  Get Cors policy to veirfy access control origin 
        /// </summary>
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var policy = _corsPolicyService.Service.GetOrCreatePolicy(request);
            return Task.FromResult(policy);
        }
    }
}