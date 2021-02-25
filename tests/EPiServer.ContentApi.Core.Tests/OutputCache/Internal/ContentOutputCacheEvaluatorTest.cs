using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using Moq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.OutputCache.Internal
{
    public class ContentOutputCacheEvaluatorTest
    {
        [Fact]
        public void EvaluateRequest_IfContextContainSecuredContent_ShouldReturnNotCachable()
        {
            var context = new ContentApiTrackingContext();
            context.SecuredContent.Add(new ContentReference(12));
            var subject = Subject(context);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.IsCacheable);
        }

        [Fact]
        public void EvaluateRequest_IfContextContainPersonalizedProperty_ShouldReturnNotCachable()
        {
            var context = new ContentApiTrackingContext();
            var metadata = new ReferencedContentMetadata();
            metadata.PersonalizedProperties.Add("MainBody");
            context.ReferencedContent.Add(new LanguageContentReference(new ContentReference(12), CultureInfo.InvariantCulture), metadata);
            var subject = Subject(context);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.IsCacheable);
        }

        [Fact]
        public void EvaluateRequest_IfContextDoesNotContainSecuredContenntOrPersonalizedProperties_ShouldReturnCachable()
        {
            var context = new ContentApiTrackingContext();
            context.ReferencedContent.Add(new LanguageContentReference(new ContentReference(12), CultureInfo.InvariantCulture), new ReferencedContentMetadata());
            var subject = Subject(context);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.True(result.IsCacheable);
        }

        [Fact]
        public void EvaluateRequest_IfContexContainReferencedContent_ShouldAddCommonDependencyToResult()
        {
            var contentLink = new ContentReference(129);
            var dependencyKey = "somekey";
            var contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            contentCacheKeyCreator.Setup(s => s.CreateCommonCacheKey(contentLink)).Returns(dependencyKey);

            var context = new ContentApiTrackingContext();
            context.ReferencedContent.Add(new LanguageContentReference(contentLink, CultureInfo.InvariantCulture), new ReferencedContentMetadata());
            var subject = Subject(context, contentCacheKeyCreator.Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.True(result.IsCacheable);
            Assert.Contains(dependencyKey, result.DependencyKeys);
        }

        [Fact]
        public void EvaluateRequest_IfContexContainLanguageReferencedContent_ShouldAddLanguageDependencyToResult()
        {
            var contentLink = new ContentReference(129);
            var dependencyKey = "somekey_en";
            var contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            contentCacheKeyCreator.Setup(s => s.CreateLanguageCacheKey(contentLink, "en")).Returns(dependencyKey);

            var context = new ContentApiTrackingContext();
            context.ReferencedContent.Add(new LanguageContentReference(contentLink, CultureInfo.GetCultureInfo("en")), new ReferencedContentMetadata());
            var subject = Subject(context, contentCacheKeyCreator.Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.True(result.IsCacheable);
            Assert.Contains(dependencyKey, result.DependencyKeys);
        }

        [Fact]
        public void EvaluateRequest_IfContexContainExireDates_ShouldAddClosestExpiresToResult()
        {
            var firstDate = DateTime.Now.AddDays(3);
            var closestDate = DateTime.Now.AddDays(1);
            var lastDate = DateTime.Now.AddDays(2);

            var context = new ContentApiTrackingContext();
            context.ReferencedContent.Add(new LanguageContentReference(new ContentReference(11), CultureInfo.InvariantCulture), new ReferencedContentMetadata { ExpirationTime = firstDate });
            context.ReferencedContent.Add(new LanguageContentReference(new ContentReference(12), CultureInfo.InvariantCulture), new ReferencedContentMetadata { ExpirationTime = closestDate });
            context.ReferencedContent.Add(new LanguageContentReference(new ContentReference(13), CultureInfo.InvariantCulture), new ReferencedContentMetadata { ExpirationTime = lastDate });
            var subject = Subject(context);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.True(result.IsCacheable);
            Assert.Equal(closestDate, result.Expires.Value);
        }

        [Fact]
        public void EvaluateRequest_WhenCalled_ShouldCallETagGeneratorAndAssignETagToResult()
        {
            var expectedETag = "someEtag";
            var eTagGenerator = new Mock<ContentETagGenerator>();
            eTagGenerator.Setup(e => e.Generate(It.IsAny<HttpRequestMessage>(), It.IsAny<ContentApiTrackingContext>())).Returns(expectedETag);
            var contentApiTrackingContextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            contentApiTrackingContextAccessor.SetupGet(x => x.Current).Returns(new ContentApiTrackingContext());
            var subject = new ContentOutputCacheEvaluator(contentApiTrackingContextAccessor.Object, Mock.Of<IContentCacheKeyCreator>(), eTagGenerator.Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.Equal(expectedETag, result.ETag);
        }

        [Fact]
        public void EvaluateRequest_WhenCalled_ShouldNotAssignExpires()
        {
            var contentApiTrackingContextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            contentApiTrackingContextAccessor.SetupGet(x => x.Current).Returns(new ContentApiTrackingContext());
            var subject = new ContentOutputCacheEvaluator(contentApiTrackingContextAccessor.Object, Mock.Of<IContentCacheKeyCreator>(), Mock.Of<ContentETagGenerator>());
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.Expires.HasValue);
        }

        ContentOutputCacheEvaluator Subject(ContentApiTrackingContext context, IContentCacheKeyCreator contentCacheKeyCreator = null)
        {
            var contextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            contextAccessor.Setup(c => c.Current).Returns(context);
            return new ContentOutputCacheEvaluator(contextAccessor.Object, contentCacheKeyCreator ?? Mock.Of<IContentCacheKeyCreator>(), Mock.Of<ContentETagGenerator>());
        }
    }
}
