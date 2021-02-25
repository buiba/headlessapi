using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.Core;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache.ETag
{
    public partial class Content
    {
        private const string Stale_While_Revalidate = "stale-while-revalidate";
        private string GetUri() => V2Uri + $"/{_page.ContentGuid}";

        [Fact]
        public async Task Get_WhenRequestWithValidETag_ShouldGet304()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetUri());
            AssertResponse.OK(contentResponse);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NotModified(eTagResponse);
            Assert.Contains(eTagResponse.Headers.CacheControl.Extensions, h => Stale_While_Revalidate.Equals(h.Name));
        }

        [Fact]
        public async Task Get_WhenPageChangeRequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetUri());
            AssertResponse.OK(contentResponse);           
            
            await Task.Delay(1000);

            var clone = _page.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);            

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
            Assert.Contains(contentResponse.Headers.CacheControl.Extensions, h => Stale_While_Revalidate.Equals(h.Name));
        }

        [Fact]
        public async Task Get_WhenDependencyChangeButPropertyIsNotExpanded_RequestWithOldETag_ShouldGet200AndSameETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetUri());
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _linkedPage.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);            

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.Equal(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task Get_WhenDependencyChangeAndPropertyIsExpanded_RequestWithOldETag_ShouldGet200AndUpdatedETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetUri() + "?expand=MainContentArea");
            AssertResponse.OK(contentResponse);

            await Task.Delay(1000);

            var clone = _linkedPage.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri() + "?expand=MainContentArea");
            requestMessage.Headers.IfNoneMatch.Add(contentResponse.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(eTagResponse);
            Assert.NotEqual(contentResponse.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task Get_WhenMultipleLanguages_ShouldNotGetSameETag()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            var enResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(enResponse);

            requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("sv"));
            var svResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(svResponse);

            Assert.NotEqual(enResponse.Headers.ETag.Tag, svResponse.Headers.ETag.Tag);
        }

        [Fact]
        public async Task Get_WhenMultipleLanguagesAndOneLanguageIsRePublished_ShouldGet200OKForAllLangauges()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            var enResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(enResponse);

            requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("sv"));
            var svResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(svResponse);

            Assert.NotEqual(enResponse.Headers.ETag.Tag, svResponse.Headers.ETag.Tag);

            var clone = _page.CreateWritableClone();
            clone.Name = clone.Name + "1";
            _fixture.ContentRepository.Save(clone, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);

            requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
            requestMessage.Headers.IfNoneMatch.Add(enResponse.Headers.ETag);
            enResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(enResponse);

            requestMessage = new HttpRequestMessage(HttpMethod.Get, GetUri());
            requestMessage.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("sv"));
            requestMessage.Headers.IfNoneMatch.Add(svResponse.Headers.ETag);
            svResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.OK(svResponse);
        }

        [Fact]
        public async Task Get_WhenPropertyIsExpanded_ShouldGetDifferentETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetUri());
            AssertResponse.OK(contentResponse);

            var expandResponse = await _fixture.Client.GetAsync(GetUri() + "?expand=MainContentArea");
            AssertResponse.OK(expandResponse);

            Assert.NotEqual(contentResponse.Headers.ETag.Tag, expandResponse.Headers.ETag.Tag);

            var expandedETagRequest = new HttpRequestMessage(HttpMethod.Get, GetUri() + "?expand=MainContentArea");
            expandedETagRequest.Headers.IfNoneMatch.Add(expandResponse.Headers.ETag);
            var expandedETagResponse = await _fixture.Client.SendAsync(expandedETagRequest);
            AssertResponse.NotModified(expandedETagResponse);
        }

        [Fact]
        public async Task Get_WhenPropertyIsSelected_ShouldGetDifferentETag()
        {
            var contentResponse = await _fixture.Client.GetAsync(GetUri());
            AssertResponse.OK(contentResponse);

            var expandResponse = await _fixture.Client.GetAsync(GetUri() + "?select=MainContentArea");
            AssertResponse.OK(expandResponse);

            Assert.NotEqual(contentResponse.Headers.ETag.Tag, expandResponse.Headers.ETag.Tag);

            var expandedETagRequest = new HttpRequestMessage(HttpMethod.Get, GetUri() + "?select=MainContentArea");
            expandedETagRequest.Headers.IfNoneMatch.Add(expandResponse.Headers.ETag);
            var expandedETagResponse = await _fixture.Client.SendAsync(expandedETagRequest);
            AssertResponse.NotModified(expandedETagResponse);
        }

        [Fact]
        public async Task GetByUrl_WhenParentChange_ShouldGetDifferentEtag()
        {
            var parentPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var page = _fixture.GetWithDefaultName<StandardPage>(parentPage.ContentLink, true);
            var uri = V2Uri + $"/{page.ContentLink}";

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
