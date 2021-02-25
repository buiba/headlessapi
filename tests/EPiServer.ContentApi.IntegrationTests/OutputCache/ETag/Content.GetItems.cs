using EPiServer.ContentApi.IntegrationTests.TestSetup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache.ETag
{
    public partial class Content
    {
        private string GetItemsUri() => V2Uri + $"?guids={_page.ContentGuid}&expand=MainContentArea";

        [Fact]
        public async Task GetItems_WhenRequestWithValidETag_ShouldGet304()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetItemsUri());
            AssertResponse.OK(contentResponse);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetItemsUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NotModified(eTagResponse);
        }

        [Fact]
        public async Task GetItems_WhenPageChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetItemsUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _page.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);            

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetItemsUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task GetItems_WhenDependencyChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetItemsUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _linkedPage.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);
                       
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetItemsUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }


    }
}
