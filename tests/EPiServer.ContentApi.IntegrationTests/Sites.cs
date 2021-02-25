using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Web;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class Sites
    {
        private const string BaseUri = "api/episerver/v2.0/site/";
        private readonly ServiceFixture _fixture;

        public Sites(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, BaseUri);

            var ressponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(ressponse);
        }

        [Fact]
        public async Task Get_WhenSiteApiIsDisabled_ShouldReturnForbidden()
        {
            using (new OptionsScope(o => o.SetSiteDefinitionApiEnabled(false)))
            {
                var response = await _fixture.Client.GetAsync(BaseUri);
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        public async Task Get_WhenMultiSiteFilteringIsEnabled_ShouldOnlyReturnCurrentSite()
        {
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var anotherSite = new SiteDefinition
                {
                    Id = Guid.NewGuid(),
                    SiteUrl = new Uri($"http://one.com/"),
                    Name = "Another site",
                    StartPage = startPage.ContentLink,
                    Hosts = { new HostDefinition { Name = "one.com" } }
                };

                await _fixture.WithSite(anotherSite, async () =>
                {
                    using (new OptionsScope(o => o.SetMultiSiteFilteringEnabled(true)))
                    {
                        var response = await _fixture.Client.GetAsync(BaseUri);
                        AssertResponse.OK(response);

                        var sites = await response.Content.ReadAsAsync<IEnumerable<SiteDefinitionModel>>();

                        Assert.Equal(SiteDefinition.Current.Id, Assert.Single(sites).Id);
                    }
                });
            }, false);
        }

        [Fact]
        public async Task Get_ShouldIncludeLanguagesFromStartPage()
        {
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true, "en");
            _fixture.CreateLanguageBranchWithDefaultName<StartPage>(startPage.ContentLink, true, "sv");

            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://one.com/"),
                    Name = "Test site",
                    StartPage = startPage.ContentLink,
                    Hosts = { new HostDefinition { Name = "one.com" } }
                };

                await _fixture.WithSite(testSite, async () =>
                {
                    var response = await _fixture.Client.GetAsync(BaseUri);
                    AssertResponse.OK(response);
                    var sites = await response.Content.ReadAsAsync<IEnumerable<SiteDefinitionModel>>();

                    var site = Assert.Single(sites, s => s.Name.Equals(testSite.Name));
                    Assert.Collection(site.Languages.OrderBy(x => x.Name),
                        x =>
                        {
                            Assert.Equal("en", x.Name);
                            Assert.Equal(CultureInfo.GetCultureInfo("en").DisplayName, x.DisplayName);
                            Assert.Equal("en", x.UrlSegment);
                        },
                        x =>
                        {
                            Assert.Equal("sv", x.Name);
                            Assert.Equal(CultureInfo.GetCultureInfo("sv").DisplayName, x.DisplayName);
                            Assert.Equal("sv", x.UrlSegment);
                        });
                });
            }, false);
        }

        [Fact]
        public async Task Get_WhenSiteHasWildcardHost_ShouldIncludeWildcardHost()
        {
            var response = await _fixture.Client.GetAsync(BaseUri);
            AssertResponse.OK(response);
            var sites = await response.Content.ReadAsAsync<IEnumerable<SiteDefinitionModel>>();

            var site = Assert.Single(sites);
            Assert.Equal(IntegrationTestCollection.DefaultSiteId, site.Id);
            Assert.Equal(IntegrationTestCollection.StartPageGuId, site.ContentRoots["startPage"].GuidValue.Value);
            Assert.Contains(sites.Single().Hosts, h => h.Name.Equals(HostDefinition.WildcardHostName));
        }

        [Fact]
        public async Task Get_WhenSiteHasLanguageMappedPrimaryHost_ShouldIncludeTypeAndLanguage()
        {
            var hostName = "publichost";

            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var siteWithPublicHost = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://{hostName}/"),
                    Name = "site with public host",
                    StartPage = startPage.ContentLink,
                    Hosts = { new HostDefinition { Name = hostName, Type = HostDefinitionType.Primary, Language = CultureInfo.GetCultureInfo("fi") }
                }
                };
                await _fixture.WithSite(siteWithPublicHost, async () =>
                {
                    var response = await _fixture.Client.GetAsync(BaseUri);
                    AssertResponse.OK(response);
                    var host = (await response.Content.ReadAsAsync<IEnumerable<SiteDefinitionModel>>()).FirstOrDefault(s => s.Name.Equals(siteWithPublicHost.Name)).Hosts.Single();
                    Assert.Equal(hostName, host.Name);
                    Assert.Equal(siteWithPublicHost.Hosts.First().Type.ToString(), host.Type);
                    Assert.Equal(siteWithPublicHost.Hosts.First().Language.Name, host.Language.Name);
                });
            }, false);
        }

        [Fact]
        public async Task GetById_WhenSiteApiIsDisabled_ShouldReturnForbidden()
        {
            using (new OptionsScope(o => o.SetSiteDefinitionApiEnabled(false)))
            {
                var response = await _fixture.Client.GetAsync(BaseUri + SiteDefinition.Current.Id);
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        public async Task GetById_WhenMatchesSite_ShouldReturnSite()
        {
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    Id = Guid.NewGuid(),
                    SiteUrl = new Uri($"http://one.com/"),
                    Name = "Test site",
                    StartPage = startPage.ContentLink,
                    Hosts = { new HostDefinition { Name = "one.com" } }
                };

                await _fixture.WithSite(testSite, async () =>
                {
                    var response = await _fixture.Client.GetAsync(BaseUri + testSite.Id);
                    AssertResponse.OK(response);

                    var site = await response.Content.ReadAsAsync<SiteDefinitionModel>();

                    Assert.NotNull(site);
                    Assert.Equal(testSite.Id, site.Id);
                });
            }, false);
        }

        [Fact]
        public async Task GetById_WhenIdDoesNotMatchSite_ShouldReturnNotFound()
        {
            var siteResponses = await _fixture.Client.GetAsync(BaseUri + Guid.NewGuid());
            AssertResponse.NotFound(siteResponses);
        }

        [Fact]
        public async Task GetById_WhenMultiSiteFilteringIsEnabledAndIdMatchesOtherSite_ShouldReturnNotFound()
        {
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    Id = Guid.NewGuid(),
                    SiteUrl = new Uri($"http://one.com/"),
                    Name = "Test site",
                    StartPage = startPage.ContentLink,
                    Hosts = { new HostDefinition { Name = "one.com" } }
                };

                await _fixture.WithSite(testSite, async () =>
                {
                    using (new OptionsScope(o => o.SetMultiSiteFilteringEnabled(true)))
                    {
                        var siteResponses = await _fixture.Client.GetAsync(BaseUri + testSite.Id);
                        AssertResponse.NotFound(siteResponses);
                    }
                });
            }, false);
        }

        [Fact]
        public async Task GetById_WhenInternalContentRootsAreEnabled_ShouldReturnInternalRoots()
        {
            using (new OptionsScope(o => o.SetIncludeInternalContentRoots(true)))
            {
                var response = await _fixture.Client.GetAsync(BaseUri + SiteDefinition.Current.Id);
                AssertResponse.OK(response);
                var site = await response.Content.ReadAsAsync<SiteDefinitionModel>();

                var expected = new[] { "contentAssetsRoot", "globalAssetsRoot", "rootPage", "startPage", "wasteBasket" };
                Assert.Equal(expected, site.ContentRoots.Keys.OrderBy(x => x));
            }
        }

        [Fact]
        public async Task GetById_WhenInternalContentRootsAreDisabled_ShouldNotReturnInternalRoots()
        {
            using (new OptionsScope(o => o.SetIncludeInternalContentRoots(false)))
            {
                var response = await _fixture.Client.GetAsync(BaseUri + SiteDefinition.Current.Id);
                AssertResponse.OK(response);
                var site = await response.Content.ReadAsAsync<SiteDefinitionModel>();

                var expected = new[] { "globalAssetsRoot", "startPage" };
                Assert.Equal(expected, site.ContentRoots.Keys.OrderBy(x => x));
            }
        }

        [Fact]
        public async Task GetById_WhenInternalContentRootsAreDisabledAndSiteHasSiteAssetRoot_ShouldIncludeSiteAssetRoot()
        {
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            var siteAssets = _fixture.GetWithDefaultName<ContentAssetFolder>(ContentReference.RootPage, true);
            await _fixture.WithContentItems(new IContent[] { startPage, siteAssets }, async () =>
            {
                var testSite = new SiteDefinition
                {
                    Id = Guid.NewGuid(),
                    SiteUrl = new Uri($"http://one.com/"),
                    Name = "Test site",
                    StartPage = startPage.ContentLink,
                    SiteAssetsRoot = siteAssets.ContentLink,
                };

                await _fixture.WithSite(testSite, async () =>
                {
                    using (new OptionsScope(o => o.SetIncludeInternalContentRoots(false)))
                    {
                        var response = await _fixture.Client.GetAsync(BaseUri + testSite.Id);
                        AssertResponse.OK(response);
                        var site = await response.Content.ReadAsAsync<SiteDefinitionModel>();

                        var expected = new[] { "globalAssetsRoot", "siteAssetsRoot", "startPage" };
                        Assert.Equal(expected, site.ContentRoots.Keys.OrderBy(x => x));
                    }
                });
            }, false);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenIncludeNullValuesIsConfigured_ShouldReturnNullPropertiesAccordingly(bool includeNullValues)
        {
            using (new OptionsScope(o => o.SetIncludeNullValues(includeNullValues)))
            {
                var response = await _fixture.Client.GetAsync(BaseUri + SiteDefinition.Current.Id);
                AssertResponse.OK(response);
                var site = JObject.Parse(await response.Content.ReadAsStringAsync());

                if (includeNullValues)
                {
                    Assert.NotNull(site["contentRoots"]["globalAssetsRoot"]["providerName"]);
                    Assert.NotNull(site["contentRoots"]["globalAssetsRoot"]["expanded"]);
                }
                else
                {
                    Assert.Null(site["contentRoots"]["globalAssetsRoot"]["providerName"]);
                    Assert.Null(site["contentRoots"]["globalAssetsRoot"]["expanded"]);
                }
            }
        }
    }
}
