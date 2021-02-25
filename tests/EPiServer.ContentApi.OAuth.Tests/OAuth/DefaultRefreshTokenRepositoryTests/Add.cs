using EPiServer.ContentApi.OAuth.Internal;
using Moq;
using System;
using Xunit;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
	public class Add : DefaultRefreshTokenRepositoryTests
	{
		[Fact]
		public void It_should_not_add_when_token_is_not_type_of_RefreshToken()
		{
			var cdsRefreshToken = new CDSRefreshToken();
			var id = Subject.Add(cdsRefreshToken);

			Assert.Equal(Guid.Empty, id);
			_dynamicDataStore.Verify(store => store.Save(It.IsAny<object>()), Times.Never);
		}

		[Fact]
		public void It_should_not_add_when_token_is_null()
		{
			var id = Subject.Add(null);

			Assert.Equal(Guid.Empty, id);
			_dynamicDataStore.Verify(store => store.Save(It.IsAny<object>()), Times.Never);
		}

		[Fact]
		public void It_should_not_add_when_subject_is_null()
		{
			var token = _refreshToken;
			token.Subject = null;

			Assert.Throws<ArgumentNullException>(() => Subject.Add(token));
		}

		[Fact]
		public void It_should_not_add_when_client_is_null()
		{
			var token = _refreshToken;
			token.ClientId = null;

			Assert.Throws<ArgumentNullException>(() => Subject.Add(token));
		}

		[Fact]
		public void It_should_not_add_when_RefreshTokenValue_is_null()
		{
			var token = _refreshToken;
			token.RefreshTokenValue = null;

			Assert.Throws<ArgumentNullException>(() => Subject.Add(token));
		}

		[Fact]
		public void It_should_not_add_when_ProtectedTicket_is_null()
		{
			var token = _refreshToken;
			token.ProtectedTicket = null;

			Assert.Throws<ArgumentNullException>(() => Subject.Add(token));
		}

		[Fact]
		public void It_should_not_add_when_store_is_null()
		{
			var dataStoreFactory = new Mock<ContentApiDynamicDataStoreFactory>();
			dataStoreFactory.Setup(f => f.GetStore(It.IsAny<Type>())).Returns(() =>
			{
				return null;
			});

			var subject = new DefaultRefreshTokenRepository(dataStoreFactory.Object);

			var token = _refreshToken;
			var id = subject.Add(token);

			Assert.Equal(Guid.Empty, id);
			_dynamicDataStore.Verify(store => store.Save(It.IsAny<object>()), Times.Never);
		}

		[Fact]
		public void It_should_save_valid_token_and_delete_exsting_one()
		{
			var token = _refreshToken;
			var id = Subject.Add(token);

			Assert.NotEqual(Guid.Empty, id);
			_dynamicDataStore.Verify(store => store.Delete(It.IsAny<object>()), Times.Once);
			_dynamicDataStore.Verify(store => store.Save(It.IsAny<object>()), Times.Once);
		}
	}

	internal class CDSRefreshToken : IRefreshToken
	{
		public Guid Guid { get; set; }
		public string RefreshTokenValue { get; set; }
		public string Subject { get; set; }
		public string ClientId { get; set; }
		public DateTime IssuedUtc { get; set; }
		public DateTime ExpiresUtc { get; set; }
		public string ProtectedTicket { get; set; }
	}
}
