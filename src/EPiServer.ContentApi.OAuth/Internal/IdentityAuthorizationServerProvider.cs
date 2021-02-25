using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace EPiServer.ContentApi.OAuth.Internal
{
    /// <summary>
    /// Extends <see cref="Microsoft.Owin.Security.OAuth.OAuthAuthorizationServerProvider"/> to provide support for bearer token
    /// authentication with ASP.NET Identity and generic user implementations.
    /// </summary>
    internal class IdentityAuthorizationServerProvider<TManager, TUser, TKey> : ApiAuthorizationServerProviderBase
        where TManager : UserManager<TUser, TKey>
        where TUser : class, IUser<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ContentApiOAuthOptions _options;
        private static readonly ILogger _log = LogManager.GetLogger(typeof(IdentityAuthorizationServerProvider<TManager, TUser, TKey>));

        public IdentityAuthorizationServerProvider() : this(ServiceLocator.Current.GetInstance<ContentApiOAuthOptions>())
        {

        }

        public IdentityAuthorizationServerProvider(ContentApiOAuthOptions options) : base()
        {
			_options = options;
		}

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are present on the request.
        /// Custom error handling can happen here. Eg: check for missing or wrong value parameter, etc.
        /// Call context.SetError() to mark the request as invalid and return an error message to client
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                context.Rejected();
                context.SetError(OAuthErrors.InvalidClientId, "Client ID must be sent");
                return Completed();
            }

            var grantType = context.Parameters[AuthorisationConstants.GrantType];
            if (grantType == null)
            {
                context.SetError(OAuthErrors.InvalidGrant, "grant_type must be sent");
                return Completed();
            }

            if (grantType.Equals(AuthorisationConstants.RefreshToken) && context.Parameters[AuthorisationConstants.RefreshToken] == null )
            {                
                context.SetError(OAuthErrors.InvalidRefreshToken, "Refresh token must be sent");
                return Completed();
            }

            var client = _options.Clients.FirstOrDefault(x => x.ClientId == context.ClientId);

            if (client == null)
            {
                context.Rejected();
                context.SetError(OAuthErrors.InvalidClientId, string.Format("Client '{0}' is not registered in the system.", context.ClientId));
                return Completed();
            }

            context.OwinContext.Set(AuthorisationConstants.GrantType, grantType);
            context.OwinContext.Set(AuthorisationConstants.ClientId, client.ClientId);
            context.OwinContext.Set(AuthorisationConstants.AllowedOrigin, client.AccessControlAllowOrigin);

            var originHeader = context.Request.Headers["Origin"];
            if (!String.IsNullOrEmpty(originHeader) && client.AccessControlAllowOrigin != "*" && !originHeader.Equals(client.AccessControlAllowOrigin))
            {
                context.Rejected();
                context.SetError(OAuthErrors.InvalidOrigin, string.Format("Origin '{0}' is not allowed by Access-Control-Allow-Origin", originHeader));
                return Completed();
            }

            context.Validated();
            return Completed();
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "password"
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            _log.Information($"Authentication Request: {GetRequestInfo(context)}");

            var allowedOrigin = context.OwinContext.Get<string>(AuthorisationConstants.AllowedOrigin);

            if (allowedOrigin == null) allowedOrigin = "*";
            
            var responseHeaders = context.OwinContext.Response.Headers;
            // Only add CORS header here if it does not exist. Otherwise in case client setup the CORS middleware along with OAuth,
            // the header will be added multiple times and cause an error
            if (!responseHeaders.ContainsKey(AuthorisationConstants.AccessControlAllowOrigin))
            {
                responseHeaders.Add(AuthorisationConstants.AccessControlAllowOrigin, new[] { allowedOrigin });
            }

            TManager userManager;
            try
            {
                userManager = context.OwinContext.GetUserManager<TManager>();
            }
            catch (Exception x)
            {
                _log.Error($"Failed to load UserManager of type {typeof(TManager).FullName} from the owin context.", x);
                RejectWithServerError(context);
                return;
            }

            TUser user;
            try
            {
                user = await userManager.FindAsync(context.UserName, context.Password);
            }
            catch (Exception x)
            {
                _log.Error("Error reading user information from UserManager.", x);
                RejectWithServerError(context);
                return;
            }            

            if (user != null && !IsUserInactiveOrLockedOut(user))
            {
                var identity = await userManager.CreateIdentityAsync(user, context.Options.AuthenticationType);

                var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                        AuthorisationConstants.ClientId, context.ClientId ?? string.Empty
                    },
                    {
                        AuthorisationConstants.Username, context.UserName
                    }
                });

                var ticket = new AuthenticationTicket(identity, props);
                context.Validated(ticket);
            }
            else
            {
                _log.Warning($"Failed Authentication Request: {GetRequestInfo(context)}");
                RejectWithInvalidUserIdOrPassword(context);
            }
        }

        /// <summary>
        /// Check if user account is inactive or locked out
        /// </summary>        
        protected virtual bool IsUserInactiveOrLockedOut(TUser user)
        {
            var uiUser = user as IUIUser;
            return uiUser != null ? (!uiUser.IsApproved || uiUser.IsLockedOut) : false;
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "refresh_token"
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary[AuthorisationConstants.ClientId];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.Rejected();
                context.SetError(OAuthErrors.InvalidClientId, $"Refresh token is not valid for client '{context.ClientId}'");
                return Completed();
            }

            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Completed();
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Completed();
        }
    }
}
