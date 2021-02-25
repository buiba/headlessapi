using EPiServer.ContentApi.Core.Internal;
using System;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace EPiServer.ContentApi.Tests.Localization
{
    public class HttpRequestHeadersExtensionsTests
    {
        [Fact]
        public void ParseAcceptLanguageHeader_ShouldReturnEmptyList_WithEmptyHeader()
        {
            var request = new HttpRequestMessage();
            request.Headers.Add("Accept-Language", "");

            var result = request.Headers.ParseAcceptLanguageHeader();
            Assert.Empty(result);
        }

        [Fact]
        public void ParseAcceptLanguageHeader_ShouldReturnEmptyList_WithWildcard()
        {
            var request = new HttpRequestMessage();
            request.Headers.Add("Accept-Language", "*");

            var result = request.Headers.ParseAcceptLanguageHeader();
            Assert.Empty(result);
        }

        [Fact]
        public void ParseAcceptLanguageHeader_ShouldReturnLanguagesFromHeader()
        {
            string[] languages = {"en-US", "fr-CA"};
            var request = new HttpRequestMessage();
            request.Headers.Add("Accept-Language", String.Join(", ", languages));

            var result = request.Headers.ParseAcceptLanguageHeader();
            Assert.Equal(result.ToList(), languages.ToList());
        }

        [Fact]
        public void ParseAcceptLanguageHeader_ShouldReturnLanguagesFromHeader_WithoutQualityWeighting()
        {
            string[] languages = { "en-US", "fr-CA" };
            var request = new HttpRequestMessage();
            request.Headers.Add("Accept-Language", String.Join(";q=0.9, ", languages));

            var result = request.Headers.ParseAcceptLanguageHeader();
            Assert.Equal(result.ToList(), languages.ToList());
        }
    }
}
