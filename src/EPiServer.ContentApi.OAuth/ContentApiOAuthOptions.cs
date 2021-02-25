using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.OAuth
{
    /// <summary>
    /// Options to control behavior of the Service Api OAuth authorization server.
    /// </summary>
    public class ContentApiOAuthOptions
    {
        private TimeSpan? _accessTokenExpireTimeSpan = TimeSpan.FromDays(14);
        private TimeSpan _refreshTokenExpireTimeSpan = TimeSpan.FromDays(14);
        private string _tokenEndpointPath = "/api/episerver/auth/token";
		private IEnumerable<ApiClientInfo> _clients;

		/// <summary>
		/// Require secure connections to the authorization server. Defaults to false.
		/// </summary>
		public bool RequireSsl { get; set; } = false;

        /// <summary>
        /// Path to the OAuth endpoint for accessing OAuth Access and Refresh tokens
        /// </summary>
        public string TokenEndpointPath
        {
            get
            {
                return _tokenEndpointPath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Token Endpoint Path is not valid");
                }
                _tokenEndpointPath = value;
            }
        } 

        /// <summary>
        /// Optional timeout for access tokens before they must be refreshed (via password grant or refresh token). If not specified, the default of
        /// <see cref="Microsoft.Owin.Security.OAuth.OAuthAuthorizationServerOptions"/> is used.
        /// </summary>
        public TimeSpan? AccessTokenExpireTimeSpan
        {
            get
            {
                return _accessTokenExpireTimeSpan;
            }
            set
            {
                if (value != null && value <= TimeSpan.Zero)
                {
                    throw new ArgumentException("Access token expire time span is not valid");
                }
                _accessTokenExpireTimeSpan = value;
            }
        } 

        /// <summary>
        /// Length of timeout for refresh tokens, before it needs to be refreshed. If not specified, the default is 2 weeks (14 days).
        /// </summary>
        public TimeSpan RefreshTokenExpireTimeSpan
        {
            get
            {
                return _refreshTokenExpireTimeSpan;
            }
            set
            {
                if (value == null || value <= TimeSpan.Zero)
                {
                    throw new ArgumentException("Refresh token expire time span is not valid");
                }
                _refreshTokenExpireTimeSpan = value;
            }
        }

		/// <summary>
		///     Return a list of Api Client
		/// </summary>
		public IEnumerable<ApiClientInfo> Clients
		{
			set
			{
				_clients = value;
			}

			get
			{
				if (_clients == null || !_clients.Any())
				{
					_clients = LoadDefaultApiClientList();
				}

				return _clients;
			}
		}

		/// <summary>
		/// Load default list of api clients.
		/// </summary>
		protected virtual List<ApiClientInfo> LoadDefaultApiClientList()
		{
			return new List<ApiClientInfo>
			{
				new ApiClientInfo
				{
					ClientId = "Default",
					AccessControlAllowOrigin = "*"
				}
			};
		}
	}
}
