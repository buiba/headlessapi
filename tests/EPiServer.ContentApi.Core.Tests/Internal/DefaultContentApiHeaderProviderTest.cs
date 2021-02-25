using EPiServer.ContentApi.Core.Internal;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Internal
{
    public class DefaultContentApiHeaderProviderTest
    {
        private DefaultContentApiHeaderProvider Subject() => new DefaultContentApiHeaderProvider();

        [Fact]
        public void HeaderNames_WhenListing_ShouldContainHeader_Accept() => Assert.Contains("Accept", Subject().HeaderNames);

        [Fact]
        public void HeaderNames_WhenListing_ShouldContainHeader_Accept_Language() => Assert.Contains("Accept-Language", Subject().HeaderNames);

        [Fact]
        public void HeaderNames_WhenListing_ShouldContainHeader_x_epi_continuation() => Assert.Contains("x-epi-continuation", Subject().HeaderNames);

        [Fact]
        public void HeaderNames_WhenListing_ShouldNotContainCustomHeader() => Assert.DoesNotContain("custom-header", Subject().HeaderNames);
    }
}
