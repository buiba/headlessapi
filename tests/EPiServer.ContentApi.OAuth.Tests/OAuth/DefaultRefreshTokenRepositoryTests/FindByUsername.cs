using Xunit;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
	public class FindByUsername : DefaultRefreshTokenRepositoryTests
	{
		[Fact]
		public void It_should_return_empty_list_when_username_is_null ()
		{
			Assert.Empty(Subject.FindByUsername(null));
		}

		[Fact]
		public void It_should_return_empty_list_when_username_is_empty()
		{
			Assert.Empty(Subject.FindByUsername(null));
		}

		[Fact]
		public void It_should_return_tokens_of_user()
		{
			var token = _refreshToken;
			Assert.NotEmpty(Subject.FindByUsername(token.Subject));
		}
	}
}
