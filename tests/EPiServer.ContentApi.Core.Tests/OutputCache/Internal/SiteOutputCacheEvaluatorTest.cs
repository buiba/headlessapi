using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Framework.Cache;
using EPiServer.Web;
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
    public class SiteOutputCacheEvaluatorTest
    {
        [Fact]
        public void EvaluateRequest_WhenCalled_ShouldAddDependencyToLocalCache()
        {
            var contentApiTrackingContext = createContentApiTrackingContextAccesor();
            var localCache = new Mock<ISynchronizedObjectInstanceCache>();
            var subject = new SiteOutputCacheEvaluator(localCache.Object, Mock.Of<SiteETagGenerator>(), Mock.Of<ISiteDefinitionRepository>(), contentApiTrackingContext.Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            foreach (var item in contentApiTrackingContext.Object.Current.ReferencedSites)
            {
                localCache.Verify(l => l.Insert($"{SiteOutputCacheEvaluator.SiteDependency}{item.Id}", It.IsAny<Object>(), CacheEvictionPolicy.Empty), Times.Once(), "Should add dependency to cache");
            }
        }

        [Fact]
        public void EvaluateRequest_WhenCalled_ShouldReturnDependency()
        {
            var contentApiTrackingContext = createContentApiTrackingContextAccesor();
            var localCache = new Mock<ISynchronizedObjectInstanceCache>();
            var subject = new SiteOutputCacheEvaluator(localCache.Object, Mock.Of<SiteETagGenerator>(), Mock.Of<ISiteDefinitionRepository>(), contentApiTrackingContext.Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());

            foreach (var item in contentApiTrackingContext.Object.Current.ReferencedSites)
            {
                Assert.Contains($"{SiteOutputCacheEvaluator.SiteDependency}{item.Id}", result.DependencyKeys);
            }
        }

        [Fact]
        public void EvaluateRequest_WhenCalled_ShouldCallETagGeneratorAndAssignETagToResult()
        {
            var expectedETag = "someEtag";
            var eTagGenerator = new Mock<SiteETagGenerator>();
            eTagGenerator.Setup(e => e.Generate(It.IsAny<IEnumerable<ReferencedSiteMetadata >>())).Returns(expectedETag);
            var subject = new SiteOutputCacheEvaluator(Mock.Of<ISynchronizedObjectInstanceCache>(), eTagGenerator.Object, Mock.Of<ISiteDefinitionRepository>(), createContentApiTrackingContextAccesor().Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.Equal(expectedETag, result.ETag);
        }

        [Fact]
        public void EvaluateRequest_WhenCalled_ShouldNotAssignExpires()
        {
            var localCache = new Mock<ISynchronizedObjectInstanceCache>();
            var subject = new SiteOutputCacheEvaluator(localCache.Object, Mock.Of<SiteETagGenerator>(), Mock.Of<ISiteDefinitionRepository>(), createContentApiTrackingContextAccesor().Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.Expires.HasValue);
        }

        [Fact]
        public void EvaluateRequest_WhenCalledAndNoSiteMetaData_ShouldReturnNocachable()
        {
            var contentApiTrackingContext = createContentApiTrackingContextAccesor(false);
            var localCache = new Mock<ISynchronizedObjectInstanceCache>();
            var subject = new SiteOutputCacheEvaluator(localCache.Object, Mock.Of<SiteETagGenerator>(), Mock.Of<ISiteDefinitionRepository>(), contentApiTrackingContext.Object);
            var result = subject.EvaluateRequest(new HttpRequestMessage(), new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.IsCacheable);
        }

        private Mock<IContentApiTrackingContextAccessor> createContentApiTrackingContextAccesor(bool createSiteMetaDataReferences = true)
        {
            var contextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            ContentApiTrackingContext context = new ContentApiTrackingContext();

            if (createSiteMetaDataReferences) 
            {
                context.ReferencedSites.Add(new ReferencedSiteMetadata (Guid.NewGuid(), DateTime.Now));
                context.ReferencedSites.Add(new ReferencedSiteMetadata (Guid.NewGuid(), DateTime.Now));
                context.ReferencedSites.Add(new ReferencedSiteMetadata (Guid.NewGuid(), DateTime.Now));
            }

            contextAccessor.Setup(accessor => accessor.Current).Returns(context);
            return contextAccessor;
        }
    }
}
