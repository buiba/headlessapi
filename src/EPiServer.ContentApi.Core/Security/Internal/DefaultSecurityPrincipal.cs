using EPiServer.Security;
using EPiServer.ServiceLocation;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;

namespace EPiServer.ContentApi.Core.Security.Internal
{
    /// <summary>
    /// Default implementation of <see cref="ISecurityPrincipal"/> for initialzing and accessing <see cref="IPrincipal"/> within ContentApi's scope.
    /// </summary>
    [ServiceConfiguration(typeof(ISecurityPrincipal))]
    public class DefaultSecurityPrincipal : ISecurityPrincipal
    {
        private static IPrincipal _anonymousPrincipal = VirtualRolePrincipal.CreateWrapper(new GenericPrincipal(new GenericIdentity("Anonymous"), new string[0]));

        /// <summary>
        /// Get <see cref="IPrincipal"/> from provided <see cref="HttpActionContext"/>. Set Principal on current thread and current <see cref="HttpContext"/>
        /// </summary>
        /// <param name="actionContext"></param>
        public virtual void InitializePrincipal(HttpActionContext actionContext)
        {
            // custom authentication logic, we must set the principal on two places: Current thread & current httpContext
            // for reference, go here https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/authentication-and-authorization-in-aspnet-web-api

            Thread.CurrentPrincipal = actionContext.RequestContext.Principal;
            // in case the application is not self-hosted
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = actionContext.RequestContext.Principal;
            }
        }

        /// <inheritdoc />
        public virtual IPrincipal GetCurrentPrincipal()
        {
            return HttpContext.Current?.User ?? Thread.CurrentPrincipal;
        }

        /// <inheritdoc />
        public virtual IPrincipal GetAnonymousPrincipal() => _anonymousPrincipal;
    }
}
