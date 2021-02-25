using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache.ETag
{
    public partial class Content
    {
        private string GetByUrlUri() => V2Uri + $"?contentUrl={HttpUtility.UrlEncode(AbsoluteContentUrl(_page.ContentLink))}&expand=MainContentArea";

        [Fact]
        public async Task GetByUrl_WhenRequestWithValidETag_ShouldGet304()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetByUrlUri());
            AssertResponse.OK(contentResponse);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetByUrlUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NotModified(eTagResponse);
        }

        [Fact]
        public async Task GetByUrl_WhenPageChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetByUrlUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _page.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);           

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetByUrlUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task GetByUrl_WhenDependencyChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetByUrlUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _linkedPage.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetByUrlUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task Get_WhenParentChange_ShouldGetDifferentEtag()
        {
            var parentPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var page = _fixture.GetWithDefaultName<StandardPage>(parentPage.ContentLink, true);
            var uri = V2Uri + $"?contentUrl={HttpUtility.UrlEncode(AbsoluteContentUrl(page.ContentLink))}";

            var contentResponse = await _fixture.Client.GetAsync(uri);
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = page.CreateWritableClone();
            var newParentPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            clone.ParentLink = new PageReference(newParentPage.ContentLink.ID, newParentPage.ContentLink.WorkID);
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.ForceCurrentVersion, Security.AccessLevel.NoAccess);

            var nextResponse = await _fixture.Client.GetAsync(uri);
            AssertResponse.OK(nextResponse);

            Assert.NotEqual(contentResponse.Headers.ETag.Tag, nextResponse.Headers.ETag.Tag);
        }
    }
}
