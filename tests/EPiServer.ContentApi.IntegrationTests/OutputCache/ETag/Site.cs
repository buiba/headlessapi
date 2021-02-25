using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache.ETag
{
    [Collection(IntegrationTestCollection.Name)]
    public class Site
    {
        private const string BaseUri = "api/episerver/v2.0/site";
        private ServiceFixture _fixture;

        public Site(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task List_WhenRequestWithValidETag_ShouldGet304()
        {
            var siteResponses = await _fixture.Client.GetAsync(BaseUri);
            AssertResponse.OK(siteResponses);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, BaseUri);
            requestMessage.Headers.IfNoneMatch.Add(siteResponses.Headers.ETag);
            var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NotModified(eTagResponse);
        }

        [Fact]
        public async Task List_WhenSiteDefinitionIsChanged_ShouldGet200AndUpdatedETag()
        {
            var siteResponses = await _fixture.Client.GetAsync(BaseUri);
            AssertResponse.OK(siteResponses);

            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true, "en");
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://one.com/"),
                    Name = "Test site",
                    StartPage = startPage.ContentLink,
                    Hosts = new List<HostDefinition> { new HostDefinition { Name = "one.com" } }
                };

                await _fixture.WithSite(testSite, async () =>
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, BaseUri);
                    requestMessage.Headers.IfNoneMatch.Add(siteResponses.Headers.ETag);
                    var eTagResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(eTagResponse);
                    Assert.NotEqual(siteResponses.Headers.ETag.Tag, eTagResponse.Headers.ETag.Tag);
                });
            });
        }

        [Fact]
        public async Task SingleSite_WhenRequestWithValidEtag_ShouldGetNotModified()
        {
            var url = $"{BaseUri}/{SiteDefinition.Current.Id}";
            var siteResponse = await _fixture.Client.GetAsync(url);
            var etag = siteResponse.Headers.ETag;

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.IfNoneMatch.Add(siteResponse.Headers.ETag);
            var secondResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NotModified(secondResponse);
            Assert.Equal(etag, secondResponse.Headers.ETag);
        }

        [Fact]
        public async Task SingleSite_WhenRequestWithInValidEtag_ShouldGet200StatusAndUpdatedEtag()
        {
            var siteDefinition = new SiteDefinition()
            {
                Name = "mysite",
                SiteUrl = new Uri("http://site.one"),
                StartPage = ContentReference.StartPage,
                Hosts = new List<HostDefinition>() { new HostDefinition() { Name = "myhost", Language = new CultureInfo("en"), Type = HostDefinitionType.Primary } }
            };

            await _fixture.WithSite(siteDefinition, async () =>
            {
                var url = $"{BaseUri}/{siteDefinition.Id}";
                var siteResponse = await _fixture.Client.GetAsync(url);
                var etag = siteResponse.Headers.ETag;

                var repository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
                siteDefinition.Name = "my site 02";
                Thread.Sleep(1000);
                repository.Save(siteDefinition);

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                requestMessage.Headers.IfNoneMatch.Add(etag);
                var secondResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(secondResponse);
                Assert.NotEqual(etag, secondResponse.Headers.ETag);
            });
        }

        [Fact]
        public async Task SingleSite_WhenAnotherSiteDefinitionIsChanged_ShouldGetNotModified()
        {
            var url = $"{BaseUri}/{SiteDefinition.Current.Id}";
            var siteResponse = await _fixture.Client.GetAsync(url);

            var testSite = new SiteDefinition()
            {
                StartPage = ContentReference.StartPage,
                Name = "site01",
                SiteUrl = new Uri("http://site.one"),
                Hosts = new List<HostDefinition>() { new HostDefinition() { Name = "host01", Language = new CultureInfo("en"), Type = HostDefinitionType.Primary } }
            };

            await _fixture.WithSite(testSite, async () => {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                requestMessage.Headers.IfNoneMatch.Add(siteResponse.Headers.ETag);
                var secondResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.NotModified(secondResponse);
            });
        }
    }
}
