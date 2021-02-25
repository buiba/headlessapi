using Xunit;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
	public class FindByValue: DefaultRefreshTokenRepositoryTests
	{
		[Fact]
		public void It_should_return_null_when_refreshTokenValue_is_null()
		{
			Assert.Null(Subject.FindByValue(null));
		}

		[Fact]
		public void It_should_return_null_when_refreshTokenValue_is_empty()
		{
			Assert.Null(Subject.FindByValue(string.Empty));
		}

		[Fact]
		public void It_should_return_Token_ignore_case_sensitive()
		{
			var token = _refreshToken;
			Assert.NotNull(Subject.FindByValue(token.RefreshTokenValue.ToUpper()));
		}
	}
}
