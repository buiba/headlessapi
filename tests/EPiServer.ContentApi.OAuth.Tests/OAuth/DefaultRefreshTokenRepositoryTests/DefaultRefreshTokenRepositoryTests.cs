using System;
using EPiServer.ContentApi.OAuth.Internal;
using EPiServer.Data.Dynamic;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
	public class DefaultRefreshTokenRepositoryTests
	{
		internal readonly RefreshToken _refreshToken;
		protected Mock<ContentApiDynamicDataStoreFactory> _dataStoreFactory;
		protected DefaultRefreshTokenRepository Subject;
		protected Mock<DynamicDataStore> _dynamicDataStore;

		public DefaultRefreshTokenRepositoryTests()
		{
			_dataStoreFactory = new Mock<ContentApiDynamicDataStoreFactory>();
			_refreshToken = new RefreshToken()
			{
				ClientId = "clientid",
				Subject = "subject",
				RefreshTokenValue = "value",
				ExpiresUtc = DateTime.UtcNow.AddDays(1),
				IssuedUtc = DateTime.UtcNow,
				ProtectedTicket = "protectedticket",
				Guid = System.Guid.NewGuid()
			};

			Subject = new DefaultRefreshTokenRepository(_dataStoreFactory.Object);

			_dynamicDataStore = new Mock<DynamicDataStore>(null);
			_dynamicDataStore.Setup(store => store.Items<RefreshToken>()).Returns(() =>
			{
				var tokens = new List<RefreshToken> { _refreshToken };
				return (from d in tokens select d).AsQueryable().OrderBy(x => x.ClientId);
			});

			_dynamicDataStore.Setup(store => store.Save(It.IsAny<RefreshToken>())).Returns((RefreshToken token) =>
			{
				return token.Id.ExternalId;
			});

			_dynamicDataStore.Setup(store => store.Delete(It.IsAny<object>()));
			_dataStoreFactory.Setup(f => f.GetStore(It.IsAny<Type>())).Returns(() =>
			{
				return _dynamicDataStore.Object;
			});
		}
	}
}

