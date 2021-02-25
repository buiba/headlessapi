using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class ImpersonationActionFilter : IAuthenticationFilter
    {
        public bool AllowMultiple => false;

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            if ((Thread.CurrentPrincipal?.Identity.IsAuthenticated).GetValueOrDefault(false))
            {
                context.Principal = Thread.CurrentPrincipal;
            }
            return Task.CompletedTask;
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
