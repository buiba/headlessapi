using System;
using Xunit;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
	public class CreateToken : DefaultRefreshTokenRepositoryTests
	{
		[Fact]
		public void It_should_get_ArgumentNullException_when_token_value_is_null_or_empty()
		{
			Assert.Throws<ArgumentNullException>(() => Subject.CreateToken(null, "clientId", "subject", DateTime.UtcNow, DateTime.UtcNow));
			Assert.Throws<ArgumentNullException>(() => Subject.CreateToken(string.Empty, "clientId", "subject", DateTime.UtcNow, DateTime.UtcNow));
		}

		[Fact]
		public void It_should_get_ArgumentNullException_when_clientId_is_null_or_empty()
		{
			Assert.Throws<ArgumentNullException>(() => Subject.CreateToken("hashValue", null, "subject", DateTime.UtcNow, DateTime.UtcNow));
			Assert.Throws<ArgumentNullException>(() => Subject.CreateToken("hashValue", string.Empty, "subject", DateTime.UtcNow, DateTime.UtcNow));
		}

		[Fact]
		public void It_should_get_ArgumentNullException_when_subject_is_null_or_empty()
		{
			Assert.Throws<ArgumentNullException>(() => Subject.CreateToken("hashValue", "clientId", null, DateTime.UtcNow, DateTime.UtcNow));
			Assert.Throws<ArgumentNullException>(() => Subject.CreateToken("hashValue", "clientId", string.Empty, DateTime.UtcNow, DateTime.UtcNow));
		}
	}
}
