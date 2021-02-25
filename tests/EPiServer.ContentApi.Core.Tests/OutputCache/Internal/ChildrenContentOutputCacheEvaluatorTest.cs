using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Moq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.OutputCache.Internal
{
    public class ChildrenContentOutputCacheEvaluatorTest
    {
        private const string V2Uri = "http://site.com/api/episerver/v2.0/content";
        private readonly ContentReference ContentLink = new ContentReference(1);

        private readonly Mock<IContentCacheKeyCreator> _contentCacheKeyCreator;
        private readonly Mock<IPermanentLinkMapper> _permanentLinkMapper;
        private readonly ContentETagGenerator _contentEtagGenerator;
        private readonly Mock<ContentLoaderService> _contentLoaderService;

        private readonly ChildrenContentOutputCacheEvaluator Subject;

        private readonly IContent ParentContent;

        public ChildrenContentOutputCacheEvaluatorTest ()
        {
            _contentCacheKeyCreator = new Mock<IContentCacheKeyCreator>();
            _permanentLinkMapper = new Mock<IPermanentLinkMapper>();
            _contentEtagGenerator = new ContentETagGenerator();
            _contentLoaderService = new Mock<ContentLoaderService>();

            Subject = new ChildrenContentOutputCacheEvaluator(_contentCacheKeyCreator.Object, _permanentLinkMapper.Object, _contentEtagGenerator, _contentLoaderService.Object);

            _contentCacheKeyCreator.Setup(c => c.CreateChildrenCacheKey(ContentLink, null)).Returns(ContentLink.ToString());
            ParentContent = CreateContent(ContentLink, "en", DateTime.Now);
            
            _contentLoaderService.Setup(x => x.Get(ContentLink, It.IsAny<string>(), It.IsAny<bool>())).Returns(ParentContent);
        }

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
        public void EvaluateRequest_GetChildrenByContentLink_IfValidRequestUri_AndAcceptLanguageHeaderIsNotSet_ShouldReturnCachable()
        {
            var requestUri = V2Uri + $"/{ContentLink}/children";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());

            Assert.True(result.IsCacheable);
            Assert.Contains(ContentLink.ToString(), result.DependencyKeys);
            Assert.Equal(_contentEtagGenerator.Generate(ParentContent), result.ETag);
        }

        [Fact]
        public void EvaluateRequest_GetChildrenByContentLink_IfValidRequestUri_AndAcceptLanguageHeaderIsSet_ShouldReturnCachable()
        {
            var requestUri = V2Uri + $"/{ContentLink}/children";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));
            httpRequest.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());

            Assert.True(result.IsCacheable);
            Assert.Contains(ContentLink.ToString(), result.DependencyKeys);
            Assert.Equal(_contentEtagGenerator.Generate(ParentContent), result.ETag);
        }

        [Fact]
        public void EvaluateRequest_GetChildrenByContentLink_IfInvalidRequestUri_ShouldReturnNotCachable()
        {
            var requestUri = V2Uri + $"/invalidContentLink/children";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.IsCacheable);
            Assert.DoesNotContain(ContentLink.ToString(), result.DependencyKeys);
        }

        [Fact]
        public void EvaluateRequest_GetChildrenByContentGuid_IfPermanentLinkForContentGuidExisted_ShouldReturnCachable()
        {
            var contentGuid = System.Guid.NewGuid();
            var requestUri = V2Uri + $"/{contentGuid.ToString()}/children";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            _permanentLinkMapper.Setup(p => p.Find(contentGuid)).Returns(new PermanentLinkMap(contentGuid, new ContentReference(1)));

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());
            Assert.True(result.IsCacheable);
        }

        [Fact]
        public void EvaluateRequest_GetChildrenByContentGuid_IfPermanentLinkForContentGuidNotExisted_ShouldReturnNotCachable()
        {
            var contentGuid = System.Guid.NewGuid();
            var requestUri = V2Uri + $"/{contentGuid.ToString()}/children";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, new System.Uri(requestUri));

            PermanentLinkMap permanentLink = null;
            _permanentLinkMapper.Setup(p => p.Find(contentGuid)).Returns(permanentLink);

            var result = Subject.EvaluateRequest(httpRequest, new HttpResponseMessage(), new ClaimsIdentity());
            Assert.False(result.IsCacheable);
        }
    }
}
