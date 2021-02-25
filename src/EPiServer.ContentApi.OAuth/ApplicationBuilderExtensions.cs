using EPiServer.ContentApi.OAuth.Internal;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace EPiServer.ContentApi.OAuth
{
    /// <summary>
    /// Extension methods to initialize ContentApi in the owin <see cref="Owin.IAppBuilder"/> pipeline.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Initialize ContentApi OAuth Authorization to use ASP.NET Identity with OAuthAuthorization tokens
        /// </summary>
        /// <typeparam name="TManager">The type of user manager configured in the application.</typeparam>
        /// <typeparam name="TUser">The user manager's user type.</typeparam>
        /// <param name="app">The application builder.</param>
        /// <returns></returns>
        public static IAppBuilder UseContentApiIdentityOAuthAuthorization<TManager, TUser>(this IAppBuilder app)
            where TManager : UserManager<TUser, string>
            where TUser : IdentityUser, IUIUser, new()
        {
            return UseContentApiIdentityOAuthAuthorization<TManager, TUser>(app, new ContentApiOAuthOptions());
        }


        /// <summary>
        ///     Initialize ContentApi OAuth Authorization to use ASP.NET Identity with OAuthAuthorization tokens
        /// </summary>
        /// <typeparam name="TManager">The type of user manager configured in the application.</typeparam>
        /// <typeparam name="TUser">The user manager's user type.</typeparam>
        /// <param name="app">The application builder.</param>
        /// <param name="oAuthOptions">Options to control behavior of the ContentApi OAuth authorization server.</param>
        /// <returns></returns>
        public static IAppBuilder UseContentApiIdentityOAuthAuthorization<TManager, TUser>(this IAppBuilder app, ContentApiOAuthOptions oAuthOptions)
        where TManager : UserManager<TUser, string>
        where TUser : IdentityUser, IUIUser, new()
        {
            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = !oAuthOptions.RequireSsl,
                TokenEndpointPath = new PathString(oAuthOptions.TokenEndpointPath),
                Provider = new IdentityAuthorizationServerProvider<TManager, TUser, string>(),
                RefreshTokenProvider = new IdentityRefreshTokenProvider(oAuthOptions, ServiceLocator.Current.GetInstance<IRefreshTokenRepository>())
            };

            if (oAuthOptions.AccessTokenExpireTimeSpan.HasValue)
            {
                oAuthServerOptions.AccessTokenExpireTimeSpan = oAuthOptions.AccessTokenExpireTimeSpan.Value;
            }

            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

			return app;
        }
    }
}
