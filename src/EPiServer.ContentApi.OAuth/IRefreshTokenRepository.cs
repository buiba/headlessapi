using System;
using System.Collections.Generic;

namespace EPiServer.ContentApi.OAuth
{
    /// <summary>
    ///     Repository interface for <see cref="IRefreshToken"/> CRUD operations
    /// </summary>
    public interface IRefreshTokenRepository
    {
		/// <summary>
		/// Create refresh token object with given data. 
		/// </summary>
		/// <param name="hashValue">Hash value of Refresh Token</param>
		/// <param name="clientid">Client Id</param>
		/// <param name="subject">Subject</param>
		/// <param name="issuedUtc">Issued UTC Time</param>
		/// <param name="expiredUtc">Expires UTC Time</param>
		IRefreshToken CreateToken(string hashValue, string clientid, string subject, DateTime issuedUtc, DateTime expiredUtc);

		/// <summary>
		///     Add a RefreshToken to the repository. If an existing refresh token exists for a given <see cref="ApiClientInfo"/> ID, then the existing token will be removed.
		/// </summary>
		/// <param name="token">RefreshToken instance to be added to the repository</param>
		System.Guid Add(IRefreshToken token);

		/// <summary>
		///     Find a RefreshToken by value
		/// </summary>
		/// <param name="refreshTokenValue">value of the RefreshToken to find</param>
		IRefreshToken FindByValue(string refreshTokenValue);

		/// <summary>
		///   Find a RefreshToken by ID
		/// </summary>
		/// <param name="id"></param>
		IRefreshToken FindById(System.Guid id);

		/// <summary>
		///     Retrieve a List of RefreshToken by username (case insensitive)
		/// </summary>
		/// <param name="userName"></param>
		IEnumerable<IRefreshToken> FindByUsername(string userName);

		/// <summary>
		///     Remove a provided RefreshToken from the repository
		/// </summary>
		/// <param name="refreshToken">RefreshToken instance to be removed</param>
		void Remove(IRefreshToken refreshToken);

		/// <summary>
		///     Remove a RefreshToken from the repository by value
		/// </summary>
		/// <param name="refreshTokenValue">value of the RefreshToken to remove</param>
		void Remove(string refreshTokenValue);
    }
}