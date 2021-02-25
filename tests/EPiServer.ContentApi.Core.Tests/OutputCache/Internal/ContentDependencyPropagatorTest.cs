using EPiServer.ContentApi.Core.OutputCache;
using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.OutputCache.Internal
{
    public class ContentDependencyPropagatorTest
    {
        [Fact]
        public void Publish_WhenContentIsNotLocalizable_ShouldRemoveCommonCacheKey()
        {
            var commonKey = "commonkey";
            var contentLink = new ContentReference(12);
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(contentLink);

            var contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            contentCacheKeyCreator.Setup(c => c.CreateCommonCacheKey(contentLink)).Returns(commonKey);

            var provider = new CaptureKeysContentOutputCacheProvider();
            var subject = new ContentDependencyPropagator(Mock.Of<IContentLoader>(), contentCacheKeyCreator.Object, provider);

            var args = new ContentEventArgs(contentLink, content.Object);
            subject.PublishingContent(null, args);
            subject.PublishedContent(null, args);
            Assert.Contains(commonKey, provider.Keys);
        }

        [Fact]
        public void Publish_WhenContentIsNotVersionableAndExistingContent_ShouldNotRemoveListingKey()
        {
            var parentListingKey = "parentkey";
            var contentLink = new ContentReference(12);
            var parentLink = new ContentReference(13);
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(contentLink);
            content.Setup(c => c.ParentLink).Returns(parentLink);

            var contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            contentCacheKeyCreator.Setup(c => c.CreateChildrenCacheKey(parentLink, null)).Returns(parentListingKey);

            var provider = new CaptureKeysContentOutputCacheProvider();
            var subject = new ContentDependencyPropagator(Mock.Of<IContentLoader>(), contentCacheKeyCreator.Object, provider);

            var args = new ContentEventArgs(contentLink, content.Object);
            subject.PublishingContent(null, args);
            subject.PublishedContent(null, args);
            Assert.DoesNotContain(parentListingKey, provider.Keys);
        }

        [Fact]
        public void Publish_WhenContentIsNotVersionableAndNewContent_ShouldRemoveListingKey()
        {
            var parentListingKey = "parentkey";
            var contentLink = ContentReference.EmptyReference;
            var parentLink = new ContentReference(13);
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(contentLink);
            content.Setup(c => c.ParentLink).Returns(parentLink);

            var contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            contentCacheKeyCreator.Setup(c => c.CreateChildrenCacheKey(parentLink, null)).Returns(parentListingKey);

            var provider = new CaptureKeysContentOutputCacheProvider();
            var subject = new ContentDependencyPropagator(Mock.Of<IContentLoader>(), contentCacheKeyCreator.Object, provider);

            var args = new ContentEventArgs(contentLink, content.Object);
            subject.PublishingContent(null, args);
            subject.PublishedContent(null, args);
            Assert.Contains(parentListingKey, provider.Keys);
        }

        [Fact]
        public void Publish_WhenContentIsVersionableAndPendingPublish_ShouldRemoveListingKey()
        {
            var parentListingKey = "parentkey";
            var contentLink = new ContentReference(12);
            var parentLink = new ContentReference(13);
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(contentLink);
            content.Setup(c => c.ParentLink).Returns(parentLink);

            var versionable = content.As<IVersionable>();
            versionable.Setup(v => v.IsPendingPublish).Returns(true);

            var contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            contentCacheKeyCreator.Setup(c => c.CreateChildrenCacheKey(parentLink, null)).Returns(parentListingKey);

            var provider = new CaptureKeysContentOutputCacheProvider();
            var subject = new ContentDependencyPropagator(Mock.Of<IContentLoader>(), contentCacheKeyCreator.Object, provider);

            var args = new ContentEventArgs(contentLink, content.Object);
            subject.PublishingContent(null, args);
            subject.PublishedContent(null, args);
            Assert.Contains(parentListingKey, provider.Keys);
        }

        [Fact]
        public void Publish_WhenContentIsVersionableAndNotPendingPublish_ShouldNotRemoveListingKey()
        {
            var parentListingKey = "parentkey";
            var contentLink = new ContentReference(12);
            var parentLink = new ContentReference(13);
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(contentLink);
            content.Setup(c => c.ParentLink).Returns(parentLink);

            var versionable = content.As<IVersionable>();
            versionable.Setup(v => v.IsPendingPublish).Returns(false);

            var contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            contentCacheKeyCreator.Setup(c => c.CreateChildrenCacheKey(parentLink, null)).Returns(parentListingKey);

            var provider = new CaptureKeysContentOutputCacheProvider();
            var subject = new ContentDependencyPropagator(Mock.Of<IContentLoader>(), contentCacheKeyCreator.Object, provider);

            var args = new ContentEventArgs(contentLink, content.Object);
            subject.PublishingContent(null, args);
            subject.PublishedContent(null, args);
            Assert.DoesNotContain(parentListingKey, provider.Keys);
        }
    }

    internal class CaptureKeysContentOutputCacheProvider : IOutputCacheProvider
    {
        public List<string> Keys { get; } = new List<string>();

        public Task<OutputCacheResult> GetAsync(HttpRequestMessage requestMessage, ClaimsIdentity identity)
        {
            throw new NotImplementedException();
        }

        public void Remove(IEnumerable<string> dependencyKeys) => Keys.AddRange(dependencyKeys);
        public Task SetAsync(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, OutputCacheEvaluateResult outputCacheEvaluateResult, ClaimsIdentity identity)
        {
            throw new NotImplementedException();
        }
    }
}
