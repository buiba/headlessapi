using EPiServer.Data.Dynamic;
using EPiServer.Framework;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EPiServer.ContentApi.OAuth.Internal
{
	/// <summary>
	///     The default implementation of <see cref="IRefreshTokenRepository"/>
	///     It uses DDS to persist and manipulate data
	/// </summary>
	[ServiceConfiguration(typeof(IRefreshTokenRepository))]
	public class DefaultRefreshTokenRepository :  IRefreshTokenRepository
	{
		private readonly ContentApiDynamicDataStoreFactory _dataStoreFactory;

		/// <summary>
		/// Initialize an instance of <see cref="DefaultRefreshTokenRepository"/>
		/// </summary>
		/// <param name="dataStoreFactory"></param>
		public DefaultRefreshTokenRepository(ContentApiDynamicDataStoreFactory dataStoreFactory)
		{
			_dataStoreFactory = dataStoreFactory;
		}

		/// <inheritdoc />
		public virtual IRefreshToken CreateToken(string refreshTokenValue, string clientId, string subject, DateTime issuedUtc, DateTime expiredUtc)
		{
			Validator.ThrowIfNullOrEmpty(nameof(refreshTokenValue), refreshTokenValue);
			Validator.ThrowIfNullOrEmpty(nameof(clientId), clientId);
			Validator.ThrowIfNullOrEmpty(nameof(subject), subject);

			return new RefreshToken()
			{
				RefreshTokenValue = refreshTokenValue,
				ClientId = clientId,
				Subject = subject,
				IssuedUtc = issuedUtc,
				ExpiresUtc = expiredUtc
			};
		}

		/// <inheritdoc />
		public virtual System.Guid Add(IRefreshToken token)
		{
			if (token == null || !(token is RefreshToken))
			{
				return Guid.Empty;
			}

			Validator.ThrowIfNullOrEmpty(nameof(token.RefreshTokenValue), token.RefreshTokenValue);
			Validator.ThrowIfNullOrEmpty(nameof(token.ClientId), token.ClientId);
			Validator.ThrowIfNullOrEmpty(nameof(token.Subject), token.Subject);
			Validator.ThrowIfNullOrEmpty(nameof(token.ProtectedTicket), token.ProtectedTicket);

			var store = GetStore();
			if (store == null)
			{
				return Guid.Empty;
			}

			var existingToken = GetTokensByConditions(t => t.Subject.ToLower().Equals(token.Subject.ToLower())
														&& t.ClientId.ToLower().Equals(token.ClientId.ToLower()) ).SingleOrDefault();
			if (existingToken != null)
			{
				Remove(existingToken);
			}

			return store.Save(token).ExternalId;
		}

		/// <inheritdoc />
		public virtual IRefreshToken FindByValue(string refreshTokenValue)
		{
			if (string.IsNullOrWhiteSpace(refreshTokenValue))
			{
				return null;
			}

			return GetTokensByConditions(t => t.RefreshTokenValue.ToLower().Equals(refreshTokenValue.ToLower()))?.SingleOrDefault();
		}

		/// <inheritdoc />
		public virtual IRefreshToken FindById(Guid id)
		{
			if (Guid.Empty == id)
			{
				return null;
			}

			return GetTokensByConditions(t => t.Id.ExternalId == id)?.SingleOrDefault();
		}

		/// <inheritdoc />
		public virtual IEnumerable<IRefreshToken> FindByUsername(string userName)
		{
			if (string.IsNullOrWhiteSpace(userName))
			{
				return Enumerable.Empty<IRefreshToken>();
			}

			return GetTokensByConditions(t => t.Subject.ToLower().Equals(userName.ToLower()));
		}

		/// <inheritdoc />
		public virtual void Remove(IRefreshToken refreshToken)
		{
			if (refreshToken == null || !(refreshToken is RefreshToken))
			{
				return;
			}

			var store = GetStore();
			if (store == null)
			{
				return;
			}

			store.Delete(refreshToken);
		}

		/// <inheritdoc />
		public virtual void Remove(string refreshTokenValue)
		{
			var store = GetStore();
			if (store == null || string.IsNullOrWhiteSpace(refreshTokenValue))
			{
				return;
			}

			var refreshToken = FindByValue(refreshTokenValue);
			if (refreshToken != null)
			{
				store.Delete(refreshToken);
			}
		}

		/// <summary>
		/// Get RefreshToken from store by expression provided.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		private IEnumerable<IRefreshToken> GetTokensByConditions(Expression<Func<RefreshToken, bool>> expression)
		{
			var store = GetStore();
			if (store == null)
			{
				return Enumerable.Empty<RefreshToken>();
			}

			return store.Items<RefreshToken>().Where(expression);
		}

		private DynamicDataStore GetStore()
		{
			return _dataStoreFactory.GetStore(typeof(RefreshToken));
		}
	}
}
