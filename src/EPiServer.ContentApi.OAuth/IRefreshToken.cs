using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.OAuth
{
    /// <summary>
	/// Refresh token information for OAuth 2.0 
	/// </summary>
	public interface IRefreshToken
    {
        /// <summary>
        /// Unique ID of token
        /// </summary>
        System.Guid Guid { get; set; }

        /// <summary>
        /// Hash value of token
        /// </summary>
        string RefreshTokenValue { get; set; }

        /// <summary>
        /// Username owns the refresh token
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Client Id 
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// UTC Time that RefreshToken is issued
        /// </summary>
        DateTime IssuedUtc { get; set; }

        /// <summary>
        /// UTC Time that RefreshToken wil be expired
        /// </summary>
        DateTime ExpiresUtc { get; set; }

        /// <summary>
        /// Protected Ticket
        /// </summary>
        string ProtectedTicket { get; set; }
    }
}
