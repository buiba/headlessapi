using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class QueryContent
    {
        private const string BaseV2Uri = "api/episerver/v2.0/content";
        private ServiceFixture _fixture;

        public QueryContent(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        private string AbsoluteContentUrl(ContentReference contentLink) => new Uri(SiteDefinition.Current.SiteUrl, _fixture.UrlResolver.GetUrl(contentLink, null, new Web.Routing.VirtualPathArguments { ContextMode = ContextMode.Default, ValidateTemplate = false })).ToString();
        private string AbsoluteContentPreviewUrl(ContentReference contentLink) => new Uri(SiteDefinition.Current.SiteUrl, _fixture.UrlResolver.GetUrl(contentLink, null, new Web.Routing.VirtualPathArguments { ContextMode = ContextMode.Preview, ValidateTemplate = false })).ToString();
        private string AbsoluteContentEditUrl(ContentReference contentLink) => new Uri(SiteDefinition.Current.SiteUrl, _fixture.UrlResolver.GetUrl(contentLink, null, new Web.Routing.VirtualPathArguments { ContextMode = ContextMode.Edit, ValidateTemplate = false })).ToString();

        [Fact]
        public async Task GetItems_WhenRequestedWithContentGuidsAndUrl_ShouldReturnBadRequest()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}&contentUrl={HttpUtility.UrlEncode(AbsoluteContentUrl(child.ContentLink))}&references=");
                AssertResponse.StatusCode(HttpStatusCode.BadRequest, contentResponse);               
            });
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentReferenceAsStringAndUrl_ShouldReturnBadRequest()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?references={parent.ContentLink},{child.ContentLink}&contentUrl={HttpUtility.UrlEncode(AbsoluteContentUrl(child.ContentLink))}&guids=");
                AssertResponse.StatusCode(HttpStatusCode.BadRequest, contentResponse);
            });
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentGuids_ShouldReturnContent()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}&contentUrl=&references=");
                AssertResponse.OK(contentResponse);
                var contentItems = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(2, contentItems.Count);
                Assert.Equal(parent.Name, (string)contentItems[0]["name"]);
                Assert.Equal(child.Name, (string)contentItems[1]["name"]);
            });
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentUrl_ShouldReturnContent()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?contentUrl={HttpUtility.UrlEncode(AbsoluteContentUrl(child.ContentLink))}&guids=&references=");
                AssertResponse.OK(contentResponse);
                var contents = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(child.Name, (string)contents[0]["name"]);
                Assert.Equal(AbsoluteContentUrl(child.ContentLink), (string)contents[0]["url"]);
            });
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentReferenceAsString_ShouldReturnContent()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?references={parent.ContentLink},{child.ContentLink}&contentUrl=&guids=");
                AssertResponse.OK(contentResponse);
                var contentItems = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(2, contentItems.Count);
                Assert.Equal(parent.Name, (string)contentItems[0]["name"]);
                Assert.Equal(child.Name, (string)contentItems[1]["name"]);
            });
        }
    }
}
