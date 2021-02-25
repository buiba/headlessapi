using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Filters;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Cors.Internal
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class CorsAttribute : ActionFilterAttribute, ICorsPolicyProvider
    {
        private readonly Injected<SiteBasedCorsPolicyService> _corsPolicyService;

        public CorsAttribute() { }

        internal CorsAttribute(SiteBasedCorsPolicyService corsPolicyService)
        {
            _corsPolicyService.Service = corsPolicyService;
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var policy = _corsPolicyService.Service.GetOrCreatePolicy(request);

            return Task.FromResult(policy);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Method == HttpMethod.Options)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.OK
                );
            }
        }
    }
}
