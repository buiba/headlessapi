using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.Core;
using EPiServer.Web;
using Moq;
using System.Net.Http;
using System.Security.Claims;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.OutputCache.Internal
{
    public class AncestorsContentOutputCacheEvaluatorTest
    {
        private const string V2Uri = "http://site.com/api/episerver/v2.0/content";
        private readonly ContentReference ContentLink = new ContentReference(1);

        private readonly Mock<IContentCacheKeyCreator> _contentCacheKeyCreator;
        private readonly Mock<IPermanentLinkMapper> _permanentLinkMapper;
        private readonly AncestorsContentOutputCacheEvaluator Subject;

        public AncestorsContentOutputCacheEvaluatorTest()
        {
            _contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            _permanentLinkMapper = new Mock<IPermanentLinkMapper>();
            Subject = new AncestorsContentOutputCacheEvaluator(_contentCacheKeyCreator.Object, _permanentLinkMapper.Object);

            _contentCacheKeyCreator.Setup(c => c.CreateCommonCacheKey(ContentLink)).Returns(ContentLink.ToString());
        }

        [Fact]
        public void EvaluateRequest_GetAncestorsByContentLink_IfValidRequestUri_ShouldReturnCachable()
        {
            var requestUri = V2Uri + $"/{ContentLink}/ancestors";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());
            Assert.True(result.IsCacheable);
            Assert.Contains(ContentLink.ToString(), result.DependencyKeys);
        }

        [Fact]
        public void EvaluateRequest_GetAncestorsByContentLink_IfInvalidRequestUri_ShouldReturnNotCachable()
        {
            var requestUri = V2Uri + $"/invalidContentLink/ancestors";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.IsCacheable);
            Assert.DoesNotContain(ContentLink.ToString(), result.DependencyKeys);
        }


        [Fact]
        public void EvaluateRequest_GetAncestorsByContentGuid_IfPermanentLinkForContentGuidExisted_ShouldReturnCachable()
        {
            var contentGuid = System.Guid.NewGuid();
            var requestUri = V2Uri + $"/{contentGuid.ToString()}/ancestors";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            _permanentLinkMapper.Setup(p => p.Find(contentGuid)).Returns(new PermanentLinkMap(contentGuid, new ContentReference(1)));

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());
            Assert.True(result.IsCacheable);
        }

        [Fact]
        public void EvaluateRequest_GetAncestorsByContentGuid_IfPermanentLinkForContentGuidNotExisted_ShouldReturnNotCachable()
        {
            var contentGuid = System.Guid.NewGuid();
            var requestUri = V2Uri + $"/{contentGuid.ToString()}/ancestors";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            PermanentLinkMap permanentLink = null;
            _permanentLinkMapper.Setup(p => p.Find(contentGuid)).Returns(permanentLink);

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.IsCacheable);
        }
    }
}
