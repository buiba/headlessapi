using EPiServer.Core;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache.ETag
{
    public partial class Content
    {
        private string GetAncestorsUri() => V2Uri + $"/{_page.ContentGuid}/ancestors";

        [Fact]
        public async Task GetAncestors_WhenRequestWithValidETag_ShouldGet304()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetAncestorsUri());
            AssertResponse.OK(contentResponse);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetAncestorsUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);

            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NotModified(eTagResponse);
        }

        [Fact]
        public async Task GetAncestors_WhenPageChangeRequestWithETag_ShouldGet200OK()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetAncestorsUri());
            AssertResponse.OK(contentResponse);

            var clone = _page.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);           

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetAncestorsUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
        }

        [Fact]
        public async Task GetAncestors_WhenPageIsMovedRequestWithETag_ShouldGet200OK()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetAncestorsUri());
            AssertResponse.OK(contentResponse);

            _fixture.ContentRepository.Move(_page.ContentLink, ContentReference.StartPage, Security.AccessLevel.NoAccess, Security.AccessLevel.NoAccess);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetAncestorsUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
        }

        [Fact]
        public async Task GetAncestors_WhenParentChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetAncestorsUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);            

            var clone = _linkedPage.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);            

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetAncestorsUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }
    }
}
