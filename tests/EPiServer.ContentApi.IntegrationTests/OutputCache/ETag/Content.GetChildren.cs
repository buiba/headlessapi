using EPiServer.ContentApi.Core;
using EPiServer.Core;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache.ETag
{
    public partial class Content
    {
        private string ChildrenUri() => V2Uri + $"/{_linkedPage.ContentLink.ToReferenceWithoutVersion()}/children";

        [Fact]
        public async Task GetChildren_WhenRequestWithValidETag_ShouldGet304()
        {
            var contentResponse = await _fixture.Client.GetAsync(ChildrenUri());
            AssertResponse.OK(contentResponse);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ChildrenUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NotModified(eTagResponse);
        }

        [Fact]
        public async Task GetChildren_WhenChildChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(ChildrenUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _page.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ChildrenUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task GetChildren_WhenParentChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(ChildrenUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _linkedPage.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ChildrenUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task GetChildren_WhenDifferentContinuationTokens_ShouldGetDifferentETags()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ChildrenUri() + "?top=1");
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            var childrenResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(childrenResponse);

            var continuationRequest = new HttpRequestMessage(HttpMethod.Get, ChildrenUri());
            continuationRequest.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            continuationRequest.Headers.Add(PagingConstants.ContinuationTokenHeaderName, childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName).First());
            var continuationResponse = await _fixture.Client.SendAsync(continuationRequest);
            AssertResponse.OK(continuationResponse);

            var nextContinuationRequest = new HttpRequestMessage(HttpMethod.Get, ChildrenUri());
            nextContinuationRequest.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            nextContinuationRequest.Headers.Add(PagingConstants.ContinuationTokenHeaderName, continuationResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName).First());
            var nextContinuationResponse = await _fixture.Client.SendAsync(nextContinuationRequest);
            AssertResponse.OK(nextContinuationResponse);

            Assert.NotEqual(childrenResponse.Headers.ETag.Tag, continuationResponse.Headers.ETag.Tag);
            Assert.NotEqual(continuationResponse.Headers.ETag.Tag, nextContinuationResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task GetChildren_WhenContinuationTokenWithValidETag_ShouldGet304()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ChildrenUri() + "?top=1");
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            var childrenResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(childrenResponse);

            var continuationRequest = new HttpRequestMessage(HttpMethod.Get, ChildrenUri());
            continuationRequest.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            continuationRequest.Headers.Add(PagingConstants.ContinuationTokenHeaderName, childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName).First());
            var continuationResponse = await _fixture.Client.SendAsync(continuationRequest);
            AssertResponse.OK(continuationResponse);

            var nextContinuationRequest = new HttpRequestMessage(HttpMethod.Get, ChildrenUri());
            nextContinuationRequest.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            nextContinuationRequest.Headers.Add(PagingConstants.ContinuationTokenHeaderName, childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName).First());
            nextContinuationRequest.Headers.IfNoneMatch.Add(continuationResponse.Headers.ETag);
            var nextContinuationResponse = await _fixture.Client.SendAsync(nextContinuationRequest);
            AssertResponse.NotModified(nextContinuationResponse);
        }
    }
}
