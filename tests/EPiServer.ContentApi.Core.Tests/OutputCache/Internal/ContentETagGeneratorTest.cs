using EPiServer.ContentApi.Core.Internal;
﻿using EPiServer.ContentApi.Core.OutputCache;
using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.OutputCache.Internal
{
    internal static class ContentETagGeneratorTestHelper
    {
        public static HttpRequestMessage AddHeader(this HttpRequestMessage httpRequestMessage, string name, string value) => httpRequestMessage.AddHeader(name, new string[] { value });

        public static HttpRequestMessage AddHeader(this HttpRequestMessage httpRequestMessage, string name, IEnumerable<string> values)
        {
            httpRequestMessage.Headers.Add(name, values);
            return httpRequestMessage;
        }

        public static ContentApiTrackingContext AddReferencedContent(this ContentApiTrackingContext contentApiTrackingContext, int contentID, string contentLanguage, DateTime? savedTime)
        {
            contentApiTrackingContext.ReferencedContent.Add(new LanguageContentReference(new ContentReference(contentID), CultureInfo.GetCultureInfo(contentLanguage)), new ReferencedContentMetadata { SavedTime = savedTime });
            return contentApiTrackingContext;
        }
    }

    internal class TestContentApiHeaderProvider : IContentApiHeaderProvider
    {
        public IEnumerable<string> HeaderNames => new[] { "x-epi-custom" };
    }

    public class ContentETagGeneratorTest
    {
        private HttpRequestMessage GenerateTestHttpRequestMessage(string url = "http://epi.com/a") => new HttpRequestMessage(HttpMethod.Get, url)
            .AddHeader("x-epi-continuation", "value1")
            .AddHeader("custom-header", new string[] { "value2a", "value2b" });

        private ContentApiTrackingContext GenerateTestContentApiTrackingContext() => new ContentApiTrackingContext()
            .AddReferencedContent(1, "en", null)
            .AddReferencedContent(2, "en", new DateTime(2020, 01, 01))
            .AddReferencedContent(2, "sv", new DateTime(2020, 01, 01));

        private ContentETagGenerator Subject() => new ContentETagGenerator(new IContentApiHeaderProvider[] { new DefaultContentApiHeaderProvider(), new TestContentApiHeaderProvider() });

        private IContent CreateContent(ContentReference contentLink, string language, DateTime? savedTime)
        {
            PropertyDataCollection publishedProperties = new PropertyDataCollection
            {
                { MetaDataProperties.PageLink, new PropertyContentReference(contentLink) },
                { MetaDataProperties.PageLanguageBranch, new PropertyString(language) }
            };

            if (savedTime != null && savedTime.HasValue)
            {
                publishedProperties.Add(MetaDataProperties.PageSaved, new PropertyDate(savedTime.Value));
            }

            return new PageData(null, publishedProperties);
        }

        [Fact]
        public void Generate_WhenSameInput_ShouldBeSameEtag()
        {
            Assert.Equal(
                Subject().Generate(GenerateTestHttpRequestMessage(), GenerateTestContentApiTrackingContext()),
                Subject().Generate(GenerateTestHttpRequestMessage(), GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenSameInputWithHeaderValueIsNull_ShouldBeSameEtag()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", (string)null);
            Assert.Equal(
                Subject().Generate(httpRequestMessage, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenSameInputWithSavedTimeIsNull_ShouldBeSameEtag()
        {
            var contentApiTrackingContext1 = new ContentApiTrackingContext().AddReferencedContent(1, "en", null);
            var contentApiTrackingContext2 = new ContentApiTrackingContext().AddReferencedContent(1, "en", null);
            Assert.Equal(
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext1),
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext2));
        }

        [Fact]
        public void Generate_WhenUrlDiffers_ShouldNotBeSameEtag()
        {
            var httpRequestMessage1 = GenerateTestHttpRequestMessage("http://epi.com/a");
            var httpRequestMessage2 = GenerateTestHttpRequestMessage("http://epi.com/b");
            Assert.NotEqual(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenDependentHeaderIsAdded_ShouldNotBeSameEtag()
        {
            var httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", "value1");
            var httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a");
            Assert.NotEqual(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenNonDependentHeaderIsAdded_ShouldBeSameEtag()
        {
            var httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("custom-header", "value1");
            var httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a");
            Assert.Equal(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenDependentHeaderNameDiffers_ShouldNotBeSameEtag()
        {
            var httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", "value1");
            var httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-custom", "value1");
            Assert.NotEqual(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenDependentHeaderValueDiffers_ShouldNotBeSameEtag()
        {
            var httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", "value1");
            var httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", "value2");
            Assert.NotEqual(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenNonDependentHeaderValueDiffers_ShouldBeSameEtag()
        {
            var httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("custom-header", "value1");
            var httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("custom-header", "value2");
            Assert.Equal(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenDepeendentHeaderCountDiffers_ShouldNotBeSameEtag()
        {
            var httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", "value1").AddHeader("x-epi-custom", "value2");
            var httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", "value1");
            Assert.NotEqual(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenHeaderOrderDiffers_ShouldBeSameEtag()
        {
            var httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-continuation", "value1").AddHeader("x-epi-custom", "value2");
            var httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "http://epi.com/a").AddHeader("x-epi-custom", "value2").AddHeader("x-epi-continuation", "value1");
            Assert.Equal(
                Subject().Generate(httpRequestMessage1, GenerateTestContentApiTrackingContext()),
                Subject().Generate(httpRequestMessage2, GenerateTestContentApiTrackingContext()));
        }

        [Fact]
        public void Generate_WhenContentReferenceDiffers_ShouldNotBeSameEtag()
        {
            var contentApiTrackingContext1 = new ContentApiTrackingContext().AddReferencedContent(1, "en", new DateTime(2020, 01, 01));
            var contentApiTrackingContext2 = new ContentApiTrackingContext().AddReferencedContent(2, "en", new DateTime(2020, 01, 01));
            Assert.NotEqual(
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext1),
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext2));
        }

        [Fact]
        public void Generate_WhenContentLanguageDiffers_ShouldNotBeSameEtag()
        {
            var contentApiTrackingContext1 = new ContentApiTrackingContext().AddReferencedContent(1, "en", new DateTime(2020, 01, 01));
            var contentApiTrackingContext2 = new ContentApiTrackingContext().AddReferencedContent(1, "sv", new DateTime(2020, 01, 01));
            Assert.NotEqual(
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext1),
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext2));
        }

        [Fact]
        public void Generate_WhenContentSavedTimeDiffers_ShouldNotBeSameEtag()
        {
            var contentApiTrackingContext1 = new ContentApiTrackingContext().AddReferencedContent(1, "en", new DateTime(2020, 01, 01));
            var contentApiTrackingContext2 = new ContentApiTrackingContext().AddReferencedContent(1, "en", new DateTime(2020, 01, 02));
            Assert.NotEqual(
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext1),
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext2));
        }

        [Fact]
        public void Generate_WhenReferencedContentCountDiffers_ShouldNotBeSameEtag()
        {
            var contentApiTrackingContext1 = new ContentApiTrackingContext().AddReferencedContent(1, "en", new DateTime(2020, 01, 01));
            var contentApiTrackingContext2 = new ContentApiTrackingContext().AddReferencedContent(1, "en", new DateTime(2020, 01, 01)).AddReferencedContent(2, "en", new DateTime(2020, 01, 01));
            Assert.NotEqual(
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext1),
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext2));
        }

        [Fact]
        public void Generate_WhenReferencedContentOrderDiffers_ShouldBeSameEtag()
        {
            var contentApiTrackingContext1 = new ContentApiTrackingContext().AddReferencedContent(2, "en", new DateTime(2020, 01, 01)).AddReferencedContent(1, "en", new DateTime(2020, 01, 01));
            var contentApiTrackingContext2 = new ContentApiTrackingContext().AddReferencedContent(1, "en", new DateTime(2020, 01, 01)).AddReferencedContent(2, "en", new DateTime(2020, 01, 01));
            Assert.Equal(
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext1),
                Subject().Generate(GenerateTestHttpRequestMessage(), contentApiTrackingContext2));
        }

        [Fact]
        public void Generate_ForSpecificContent_WhenSameAllInputs_ShouldBeSameEtag()
        {
            var now = DateTime.Now;
            var content1 = CreateContent(new ContentReference(1), "en", now);
            var content2 = CreateContent(new ContentReference(1), "en", now);
            Assert.Equal(
                  Subject().Generate(content1),
                  Subject().Generate(content2));
        }

        [Fact(Skip = "The test does not set up properly. Consider to remove it.")]
        public void Generate_ForSpecificContent_WhenSameInputWithSavedTimeIsNull_ShouldBeSameEtag()
        {
            var content1 = CreateContent(new ContentReference(1), "en", null);
            var content2 = CreateContent(new ContentReference(1), "en", null);
           
            Assert.Equal(
                  Subject().Generate(content1),
                  Subject().Generate(content2));
        }

        [Fact]
        public void Generate_ForSpecificContent_WhenDifferentContentLink_ShouldNotBeSameEtag()
        {
            var now = DateTime.Now;
            var content1 = CreateContent(new ContentReference(1), "en", now);
            var content2 = CreateContent(new ContentReference(2), "en", now);
            Assert.NotEqual(
                  Subject().Generate(content1),
                  Subject().Generate(content2));
        }

        [Fact]
        public void Generate_ForSpecificContent_WhenDifferentLanguage_ShouldNotBeSameEtag()
        {
            var now = DateTime.Now;
            var content1 = CreateContent(new ContentReference(1), "en", now);
            var content2 = CreateContent(new ContentReference(1), "sv", now);
            Assert.NotEqual(
                  Subject().Generate(content1),
                  Subject().Generate(content2));
        }

        [Fact]
        public void Generate_ForSpecificContent_WhenDifferentSavedTime_ShouldNotBeSameEtag()
        {
            var content1 = CreateContent(new ContentReference(1), "en", DateTime.Now);
            var content2 = CreateContent(new ContentReference(1), "en", DateTime.Now.AddDays(1));
            Assert.NotEqual(
                  Subject().Generate(content1),
                  Subject().Generate(content2));
        }
    }
}
