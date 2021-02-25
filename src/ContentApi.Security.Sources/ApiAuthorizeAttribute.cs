using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Security.Internal
{
    internal sealed class ApiAuthorizeAttribute : AuthorizationFilterAttribute
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(ApiAuthorizeAttribute));

        private readonly Injected<IApiAuthorizationOptions> _options;

        public ApiAuthorizeAttribute() { }

        internal ApiAuthorizeAttribute(IApiAuthorizationOptions options)
        {
            _options.Service = options;
        }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            base.OnAuthorizationAsync(actionContext, cancellationToken);

            if (_options.Service.AllowedScopes.Count == 0)
            {
                Log.Warning($"{_options.Service.Name} has no Allowed Scopes configured. API now accepts anonymous calls.");
                return Task.CompletedTask;
            }

            var identity = actionContext.ControllerContext.RequestContext.Principal?.Identity;
            if (UserHasScopes(identity))
            {
                return Task.CompletedTask;
            }

            HandleUnauthorizedRequest(actionContext);

            return Task.CompletedTask;
        }

        private bool UserHasScopes(IIdentity identity)
        {
            if (identity == null || identity.IsAuthenticated == false)
            {
                return false;
            }

            if (identity is ClaimsIdentity claimsIdentity)
            {
                var scopes = claimsIdentity.FindAll(_options.Service.ScopeClaimType);

                if (scopes == null || scopes.Any() == false)
                {
                    return false;
                }

                foreach (var scope in scopes)
                {
                    if (_options.Service.AllowedScopes.Contains(scope.Value, StringComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized");
            actionContext.Response.Headers.Add("WWW-Authenticate", new[] { "Bearer error=\"insufficient_scope\"" });
        }
    }
}
