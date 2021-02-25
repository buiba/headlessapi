using CustomMappers;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class Filters : IAsyncLifetime
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private ServiceFixture _fixture;
        private StandardPage _page;
        private StandardPage _linkedPage;

        public Filters (ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync()
        {
            _linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            _page = _fixture.GetWithDefaultName<StandardPage>(_linkedPage.ContentLink, true, init: page =>
            {
                var links = new LinkItemCollection { new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(_linkedPage.ContentGuid, ".aspx"), Text = _linkedPage.Name } };
                page.Links = links;
                var contentArea = new ContentArea { Items = { new ContentAreaItem { ContentLink = _linkedPage.ContentLink } } };
                page.MainContentArea = contentArea;
                page.ContentReferenceList = new List<ContentReference>(new [] { _linkedPage.ContentLink });
                page.TargetReference = _linkedPage.ContentLink;
            });
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _fixture.ContentRepository.Delete(_linkedPage.ContentLink, true, Security.AccessLevel.NoAccess);
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReference_AndUsingOptimizedOptions_ShouldRemoveContentIdAndWorkId()
        {
            using (new OptionsScope(true))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content["targetReference"]["id"]);
                Assert.Null(content["targetReference"]["workId"]);
                Assert.Equal(_linkedPage.ContentGuid, content["targetReference"]["guidValue"]);
            }
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceIsExpanded_AndUsingOptimizedOptions_ShouldRemoveContentIdAndWorkIdOnExpanded()
        {
            using (new OptionsScope(true))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=TargetReference");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content["targetReference"]["expanded"]["contentLink"]["id"]);
                Assert.Null(content["targetReference"]["expanded"]["contentLink"]["workId"]);
                Assert.Equal(_linkedPage.ContentGuid, content["targetReference"]["expanded"]["contentLink"]["guidValue"]);
            }
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceIsNotExpanded_ShouldNotAddLanguageToContentLink()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            Assert.Null(content["targetReference"]["language"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceIsExpanded_AndUsingOptimizedOptions_ShouldAddLanguageToContentLink()
        {
            using (new OptionsScope(true))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=TargetReference");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(_linkedPage.Language.Name, content["targetReference"]["expanded"]["contentLink"]["language"]["name"]);
            }
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentArea_AndUsingOptimizedOptions_ShouldRemoveContentIdAndWorkIdOnAreaItem()
        {
            using (new OptionsScope(true))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var contentArea = content["mainContentArea"] as JArray;
                Assert.Null(contentArea[0]["contentLink"]["id"]);
                Assert.Null(contentArea[0]["contentLink"]["workId"]);
                Assert.Equal(_linkedPage.ContentGuid, contentArea[0]["contentLink"]["guidValue"]);
            }
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentAreaIsExpanded_AndUsingOptimizedOptions_ShouldRemoveContentIdAndWorkIdOnExpandedAreaItem()
        {
            using (new OptionsScope(true))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=MainContentArea");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var contentArea = content["mainContentArea"] as JArray;
                Assert.Null(contentArea[0]["contentLink"]["expanded"]["contentLink"]["id"]);
                Assert.Null(contentArea[0]["contentLink"]["expanded"]["contentLink"]["workId"]);
                Assert.Equal(_linkedPage.ContentGuid, contentArea[0]["contentLink"]["expanded"]["contentLink"]["guidValue"]);
            }
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentAreaIsExpanded_AndUsingOptimizedOptions_ShouldAddLanguageToContentLinkOnExpandedAreaItem()
        {
            using (new OptionsScope(true))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=MainContentArea");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var contentArea = content["mainContentArea"] as JArray;
                Assert.Equal(_linkedPage.Language.Name, contentArea[0]["contentLink"]["language"]["name"]);
            }
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentAreaIsNotExpanded_ShouldNotAddLanguageToContentLinkOnExpandedAreaItem()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var contentArea = content["mainContentArea"] as JArray;
            Assert.Null(contentArea[0]["contentLink"]["language"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfLinkCollection_ShouldRemoveContentIdAndWorkIdOnLinkItemNode()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["links"] as JArray;
            Assert.Null(links[0]["contentLink"]["id"]);
            Assert.Null(links[0]["contentLink"]["workId"]);
            Assert.Equal(_linkedPage.ContentGuid, links[0]["contentLink"]["guidValue"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfLinkCollectionIsExpanded_ShouldRemoveContentIdAndWorkIdOnExpandedLinkItemNode()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=Links");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["links"] as JArray;
            Assert.Null(links[0]["contentLink"]["expanded"]["contentLink"]["id"]);
            Assert.Null(links[0]["contentLink"]["expanded"]["contentLink"]["workId"]);
            Assert.Equal(_linkedPage.ContentGuid, links[0]["contentLink"]["expanded"]["contentLink"]["guidValue"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfLinkCollectionIsExpanded_ShouldAddLanguageToContentItemOnExpandedLinkItemNode()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=Links");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["links"] as JArray;
            Assert.Equal(_linkedPage.Language.Name, links[0]["contentLink"]["language"]["name"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfLinkCollectionIsNotExpanded_ShouldNotAddLanguageToContentItemOnExpandedLinkItemNode()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["links"] as JArray;
            Assert.Null(links[0]["contentLink"]["language"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceList_ShouldRemoveContentIdAndWorkIdOnListItem()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["contentReferenceList"] as JArray;
            Assert.Null(links[0]["id"]);
            Assert.Null(links[0]["workId"]);
            Assert.Equal(_linkedPage.ContentGuid, links[0]["guidValue"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceListIsExpanded_ShouldRemoveContentIdAndWorkIdOnExpandedListItem()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=ContentReferenceList");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["contentReferenceList"] as JArray;
            Assert.Null(links[0]["expanded"]["contentLink"]["id"]);
            Assert.Null(links[0]["expanded"]["contentLink"]["workId"]);
            Assert.Equal(_linkedPage.ContentGuid, links[0]["expanded"]["contentLink"]["guidValue"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceListIsExpanded_ShouldAddLanguageToContentLinkOnExpandedListItem()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=ContentReferenceList");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["contentReferenceList"] as JArray;
            Assert.Equal(_linkedPage.Language.Name, links[0]["language"]["name"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceListIsNotExpanded_ShouldNotAddLanguageToContentLinkOnExpandedListItem()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["contentReferenceList"] as JArray;
            Assert.Null(links[0]["language"]);
        }

        [Fact]
        public async Task Get_WhenVersion2Default_ShouldNotIncludeMasterLanguage()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            Assert.Null(content["masterLanguage"]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenVersion2Default_ShouldIncludeMasterLanguage(bool optimizeForDerlivery)
        {
            using (new OptionsScope(optimizeForDerlivery))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                if (!optimizeForDerlivery)
                    Assert.Equal("en", (string)content["masterLanguage"]["name"]);
                else
                    Assert.Null(content["masterLanguage"]);
            }
        }

        [Fact]
        public async Task Get_WhenVersion2AndConfiguredOption_ShouldIncludeMasterLanguage()
        {
            using (new OptionsScope(c => c.SetIncludeMasterLanguage(true)))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal("en", (string)content["masterLanguage"]["name"]);
            }        
        }

        [Fact]
        public async Task Get_WhenVersion2AndConfiguredOption_ShouldNotIncludeMasterLanguage()
        {
            using (new OptionsScope(c => { c.SetIncludeMasterLanguage(false); c.SetIncludeNullValues(false); }))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content["masterLanguage"]);
            }
        }

        [Fact]
        public async Task Get_WhenContentReferenceIsExpanded_ShouldNotContainMasterLanguageOnExpanded()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=TargetReference");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            Assert.Null(content["targetReference"]["expanded"]["masterLanguage"]);
        }

        [Fact]
        public async Task Get_WhenContentAreaIsExpanded_ShouldNotContainMasterLanguageOnExpanded()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=MainContentArea");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var contentArea = content["mainContentArea"] as JArray;
            Assert.Null(contentArea[0]["contentLink"]["expanded"]["masterLanguage"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfLinkCollectionIsExpanded_ShouldNotContainMasterLanguageOnExpanded()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=Links");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["links"] as JArray;
            Assert.Null(links[0]["contentLink"]["expanded"]["masterLanguage"]);
        }

        [Fact]
        public async Task Get_WhenCustomPropertyOfContentReferenceListIsExpanded_ShouldNotContainMasterLanguageOnExpanded()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand=ContentReferenceList");
            AssertResponse.OK(contentResponse);
            var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
            var links = content["contentReferenceList"] as JArray;
            Assert.Null(links[0]["expanded"]["masterLanguage"]);
        }

        [Theory]
        [InlineData("TargetReference")]
        [InlineData("ContentReferenceList")]
        [InlineData("Links")]
        [InlineData("MainContentArea")]
        public async Task Get_WhenCustomPropertyIsExpanded_ShouldCallFiltersForExpanded(string propertyToExpand)
        {
            using (var assertCounter = new AssertFilterCalled(_page.ContentGuid, _linkedPage.ContentGuid))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_page.ContentGuid}?expand={propertyToExpand}");
                AssertResponse.OK(contentResponse);
            }
        }

        private class AssertFilterCalled : IDisposable
        {
            private readonly IEnumerable<Guid> _expectedIds;
            private readonly CountingContentFilter _countingContentFilter;
            private readonly CountingContentModelFilter _countingModelFilter;

            public AssertFilterCalled(params Guid[] expectedIds)
            {
                _expectedIds = expectedIds ?? Enumerable.Empty<Guid>();
                _countingContentFilter = ServiceLocator.Current.GetAllInstances<IContentFilter>().OfType<CountingContentFilter>().Single();
                _countingModelFilter = ServiceLocator.Current.GetAllInstances<IContentApiModelFilter>().OfType<CountingContentModelFilter>().Single();
                _countingContentFilter.CalledItems.Clear();
                _countingModelFilter.CalledItems.Clear();
            }

            public void Dispose()
            {
                foreach (var id in _expectedIds)
                {
                    Assert.Contains(_countingContentFilter.CalledItems, r => r == id);
                    Assert.Contains(_countingModelFilter.CalledItems, r => r.GuidValue.Value == id);
                }
            }
        }
    }


}
