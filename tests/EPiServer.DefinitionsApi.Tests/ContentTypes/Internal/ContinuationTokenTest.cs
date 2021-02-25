using System;
using System.Text;
using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    public class ContinuationTokenTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("\r\n")]
        public void TryParse_WithNullOrEmptyString_ShouldReturnSuccessWithEmptyToken(string tokenString)
        {
            Assert.True(ContinuationToken.TryParseTokenString(tokenString, out var token));
            Assert.Equal(ContinuationToken.None, token);
        }

        public static TheoryData InvalidTokenStrings => new TheoryData<string>
        {
            "This is not a Base64 string!!",
            ToBase64("This is a Base64 string"),
            ToBase64("NaN|100"),
            ToBase64("100|nope"),
            ToBase64("-5|100"),
            ToBase64("100|-5"),
        };

        [Theory]
        [MemberData(nameof(InvalidTokenStrings))]
        public void TryParse_WithInvalidString_ShouldReturnFalse(string tokenString)
        {
            Assert.False(ContinuationToken.TryParseTokenString(tokenString, out var token));
            Assert.Equal(ContinuationToken.None, token);
        }

        [Fact]
        public void Parse_WhenStringIsValidToken_ShouldReturnTrueWithToken()
        {
            Assert.True(ContinuationToken.TryParseTokenString(ToBase64("50|100"), out var token));
            Assert.Equal(new ContinuationToken(50, 100), token);
        }

        [Fact]
        public void AsTokenString_ShouldReturnParseableString()
        {
            var source = new ContinuationToken(68, 100);

            ContinuationToken.TryParseTokenString(source.AsTokenString(), out var token);
            Assert.Equal(source, token);
        }

        private static string ToBase64(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }
}
