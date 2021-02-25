using EPiServer.ContentApi.OAuth.Internal;
using EPiServer.Data.Dynamic;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
	public class Remove : DefaultRefreshTokenRepositoryTests
	{
		[Fact]
		public void It_should_not_delete_when_token_is_null()
		{
			Subject.Remove(null as IRefreshToken);
			_dynamicDataStore.Verify(store => store.Delete(It.IsAny<RefreshToken>()), Times.Never);
		}

		[Fact]
		public void It_should_not_delete_when_token_is_not_type_of_RefreshToken()
		{
			Subject.Remove(new CDSRefreshToken() as IRefreshToken);
			_dynamicDataStore.Verify(store => store.Delete(It.IsAny<RefreshToken>()), Times.Never);
		}

		[Fact]
		public void It_should_not_delete_when_refreshTokenValue_is_null()
		{
			Subject.Remove(null as string);
			_dynamicDataStore.Verify(store => store.Delete(It.IsAny<RefreshToken>()), Times.Never);
		}

		[Fact]
		public void It_should_not_delete_when_refreshTokenValue_is_empty()
		{
			Subject.Remove(string.Empty);
			_dynamicDataStore.Verify(store => store.Delete(It.IsAny<RefreshToken>()), Times.Never);
		}

		[Fact]
		public void It_should_delete_RefreshToken_when_token_is_type_of_RefreshToken()
		{
			Subject.Remove(new RefreshToken() as IRefreshToken);
			_dynamicDataStore.Verify(store => store.Delete(It.IsAny<RefreshToken>()), Times.Once);
		}

		[Fact]
		public void It_should_delete_RefreshToken_when_refreshTokenValue_has_value()
		{
			var token = _refreshToken;
			Subject.Remove(token.RefreshTokenValue);
			_dynamicDataStore.Verify(store => store.Delete(It.IsAny<RefreshToken>()), Times.Once);
		}
	}
}
