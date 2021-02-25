using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace EPiServer.ContentApi.OAuth.Internal
{
    /// <summary>
    /// Base class for <see cref="Microsoft.Owin.Security.OAuth.IOAuthAuthorizationServerProvider"/> implementations, adding a
    /// few helper methods.
    /// </summary>
    internal abstract class ApiAuthorizationServerProviderBase : OAuthAuthorizationServerProvider
    {
        /// <summary>
        /// Produces a completed <see cref="Task"/> for non-asynchronous implementations of methods
        /// returning tasks.
        /// </summary>
        /// <returns>An Task wrapping a null result.</returns>
        protected Task Completed()
        {
            var source = new TaskCompletionSource<object>();
            source.SetResult(null);
            return source.Task;
        }

        /// <summary>
        /// Returns request header values.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A comma separated list of key:value pairs.</returns>
        protected string GetRequestInfo(OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (context.Request != null)
            {
                return string.Join(", ",
                    Enumerable.Select<KeyValuePair<string, string[]>, string>(context.Request.Headers,
                        h => h.Key + ":" + string.Join(", ", h.Value)));
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the context to rejected with an "invalid_credentials" error.
        /// </summary>
        /// <param name="context">The resource grant context.</param>
        protected void RejectWithInvalidUserIdOrPassword(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.SetError(OAuthErrors.InvalidCredentials, "Invalid username or password, or the user account is inactive/locked out");            
        }       

        /// <summary>
        /// Sets the context to rejected with a "server_error" error.
        /// </summary>
        /// <param name="context">The resource grant context.</param>
        protected void RejectWithServerError(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.SetError(OAuthErrors.ServerError);            
        }
    }
}