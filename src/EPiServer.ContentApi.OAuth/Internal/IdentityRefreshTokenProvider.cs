using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;

namespace EPiServer.ContentApi.OAuth.Internal
{
    /// <summary>
    ///     Refresh Token Provider implementation which utilizes the <see cref="IRefreshTokenRepository"/> to create and validate refresh tokens for OWIN OAuth authentication
    /// </summary>
    public class IdentityRefreshTokenProvider : IAuthenticationTokenProvider
    {
        private static readonly object _lock = new object();
        private readonly ContentApiOAuthOptions _options;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRefreshTokenProvider"/> class.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="refreshTokenRepository"></param>
        public IdentityRefreshTokenProvider(ContentApiOAuthOptions options, IRefreshTokenRepository refreshTokenRepository)
        {
            _options = options;
            _refreshTokenRepository = refreshTokenRepository;
        }

        /// <summary>
        ///     Creates a RefreshToken, storing it in the <see cref="IRefreshTokenRepository"/> and attaching it to the provided <see cref="AuthenticationTokenCreateContext"/>, 
        /// </summary>
        /// <param name="context">AuthenticationTokenCreateContext to base the ticket creation</param>
        /// <returns></returns>
        public Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientid = context.Ticket.Properties.Dictionary[AuthorisationConstants.ClientId];
            var grantType = context.OwinContext.Get<string>(AuthorisationConstants.GrantType);

            if (string.IsNullOrEmpty(clientid))
            {
                return Task.FromResult<object>(null);
            }

            var refreshTokenValue = Guid.NewGuid().ToString("n");
            var hashRefreshTokenValue = GetHash(refreshTokenValue);

            var refreshTokenLifeTime = _options.RefreshTokenExpireTimeSpan;
            var newToken = _refreshTokenRepository.CreateToken(hashRefreshTokenValue,
                                                                    clientid,
                                                                    context.Ticket.Identity.Name,
                                                                    DateTime.UtcNow,
                                                                    DateTime.UtcNow.Add(refreshTokenLifeTime));

            // When there are concurrent requests from the same identity sent to OAuth Server, two refresh tokens might be created for an user.
            // Lock here can ensure that this case will not happen.
            lock (_lock)
            {
                var existingRefreshToken = _refreshTokenRepository.FindByUsername(context.Ticket.Identity.Name).Where(rt => rt.ClientId == clientid).SingleOrDefault();

                if (existingRefreshToken != null)
                {
                    // If the new refresh token is created with grant_type = 'password', then it should have a new expiration time
                    // Otherwise (i.e refresh token is created with grant_type = 'refresh_token'), it should have the same expiration time as the existing one                
                    if (string.Equals(grantType, AuthorisationConstants.RefreshToken, StringComparison.OrdinalIgnoreCase))
                    {
                        newToken.ExpiresUtc = existingRefreshToken.ExpiresUtc;
                    }
                    _refreshTokenRepository.Remove(existingRefreshToken);
                }

                context.Ticket.Properties.IssuedUtc = newToken.IssuedUtc;
                context.Ticket.Properties.ExpiresUtc = newToken.ExpiresUtc;

                newToken.ProtectedTicket = context.SerializeTicket();

                var result = _refreshTokenRepository.Add(newToken);
                if (result != Guid.Empty)
                {
                    context.SetToken(refreshTokenValue);
                }
            } 

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Looks up a refesh token provided in the <see cref="AuthenticationTokenReceiveContext"/> and deserializes it, if found
        /// </summary>
        /// <param name="context">AuthenticationTokenReceiveContext to locate the ticket</param>
        /// <returns></returns>
        public Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>(AuthorisationConstants.AllowedOrigin);

            var responseHeaders = context.OwinContext.Response.Headers;
            // Only add CORS header here if it does not exist. Otherwise in case client setup the CORS middleware along with OAuth,
            // the header will be added multiple times and cause an error
            if (!responseHeaders.ContainsKey(AuthorisationConstants.AccessControlAllowOrigin))
            {
                responseHeaders.Add(AuthorisationConstants.AccessControlAllowOrigin, new[] { allowedOrigin });
            }

            var hashTokenValue = GetHash(context.Token);
            var refreshToken = _refreshTokenRepository.FindByValue(hashTokenValue);

            if (refreshToken != null)
            {
                context.DeserializeTicket(refreshToken.ProtectedTicket);
            }

            return Task.FromResult<object>(null);
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        private string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
