using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Security.Internal
{
    public class ApiAuthorizeAttributeTests
    {
        [Fact]
        public async Task OnAuthorization_WhenAnonymousNoAllowedScopes_ShouldReturnOk()
        {
            var options = GetOptions();
            var action = new HttpActionContext();
            var attribute = new ApiAuthorizeAttribute(options);

            await attribute.OnAuthorizationAsync(action.Context, CancellationToken.None);

            // When response is null it means the filter didn't touch the response,
            // i.e. authorization succeeded.
            Assert.Null(action.ExecutedContext.Response);
        }

        [Fact]
        public async Task OnAuthorization_WhenAnonymousAndAllowedScopes_ShouldReturnUnauthorized()
        {
            var options = GetOptions("scope");
            var action = new HttpActionContext();
            var attribute = new ApiAuthorizeAttribute(options);

            await attribute.OnAuthorizationAsync(action.Context, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Unauthorized, action.ExecutedContext.Response.StatusCode);
        }

        [Fact]
        public async Task OnAuthorization_WhenUserNotMemberOfAllowedScopes_ShouldReturnUnauthorized()
        {
            var principal = GetPrincipal("scope_1", "scope_2");
            var options = GetOptions("scope");
            var action = new HttpActionContext(principal);
            var attribute = new ApiAuthorizeAttribute(options);

            await attribute.OnAuthorizationAsync(action.Context, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Unauthorized, action.ExecutedContext.Response.StatusCode);
        }

        [Fact]
        public async Task OnAuthorization_WhenUserMemberOfAllowedScopes_ShouldReturnOk()
        {
            var principal = GetPrincipal("scope_1", "scope_2");
            var options = GetOptions("scope_2");
            var action = new HttpActionContext(principal);
            var attribute = new ApiAuthorizeAttribute(options);

            await attribute.OnAuthorizationAsync(action.Context, CancellationToken.None);

            // When response is null it means the filter didn't touch the response,
            // i.e. authorization succeeded.
            Assert.Null(action.ExecutedContext.Response);
        }

        private ClaimsPrincipal GetPrincipal(params string[] scopes)
        {
            // Important to set authenticationType, otherwise IsAuthenticated is false.
            var identity = new ClaimsIdentity("local", "name", "role");

            foreach (var scope in scopes)
            {
                identity.AddClaim(new Claim("scope", scope));
            }

            var principal = new ClaimsPrincipal(identity);

            return principal;
        }

        private TestApiAuthorizationOptions GetOptions(params string[] allowedScopes)
        {
            var options = new TestApiAuthorizationOptions();
            options.AllowedScopes.Clear();

            if (allowedScopes is object)
            {
                foreach (var scope in allowedScopes)
                {
                    options.AllowedScopes.Add(scope);
                }
            }

            return options;
        }
        private class TestApiAuthorizationOptions : IApiAuthorizationOptions
        {
            public ICollection<string> AllowedScopes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public string ScopeClaimType => "scope";
            public string Name => "Test API";
        }
    }

    internal class HttpActionContext
    {
        public HttpActionContext(IPrincipal principal = null)
        {
            Context = new System.Web.Http.Controllers.HttpActionContext(
                new HttpControllerContext(
                    new HttpConfiguration(),
                    Mock.Of<IHttpRouteData>(),
                    new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com")))
                {
                    RequestContext = new HttpRequestContext
                    {
                        Principal = principal ?? ClaimsPrincipal.Current
                    }
                },
                Mock.Of<HttpActionDescriptor>());

            ExecutedContext = new HttpActionExecutedContext(Context, null);
        }

        public System.Web.Http.Controllers.HttpActionContext Context { get; private set; }

        public HttpActionExecutedContext ExecutedContext { get; private set; }
    }
}
