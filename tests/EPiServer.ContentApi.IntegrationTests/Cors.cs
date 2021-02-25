using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Cors;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class Cors
    {
        private const string V2Uri = "api/episerver/v2.0/content";
        private readonly ServiceFixture _fixture;
        private const string CustomHeader = "custom-header";

        public Cors(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_WhenOriginMatchPolicy_ShouldAddAllowHeader()
        {
            var hosts = new List<HostDefinition>()
                {
                    new HostDefinition()
                    {
                        Name = "localhost:8878",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("en")
                    },
                      new HostDefinition()
                    {
                        Name = "cd.host01",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("sv")
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host02",
                        Type = HostDefinitionType.Edit
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host03",
                        Type = HostDefinitionType.RedirectPermanent
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host04",
                        Type = HostDefinitionType.RedirectTemporary
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host05",
                        Type = HostDefinitionType.Undefined
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host06",
                        Type = HostDefinitionType.Primary
                    },
                };

            var siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid(),
                Name = "site one",
                SiteUrl = new Uri("http://localhost:8877/"),
                StartPage = new ContentReference(100),
                Hosts = hosts
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{siteDefinition.SiteUrl}" + V2Uri);
            var origin = "http://cd.host01:8877";
            requestMessage.Headers.Add(CorsConstants.Origin, origin);
            var siteRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
            siteRepository.Save(siteDefinition);

            await _fixture.WithSite(siteDefinition, async () =>
            {
                {
                    ClearCorsPolicyCache();
                    using (var noClientConfiguration = new OptionsScope(o => o.SetClients(null)))
                    {
                        var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                        AssertResponse.OK(contentResponse);
                        Assert.Equal(origin, contentResponse.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).First());
                    }
                }
            });
        }

        [Fact]
        public async Task Get_WhenOriginMatchPolicy_ShouldAddAccessControlExposeHeaders()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri);
            requestMessage.Headers.Add(CorsConstants.Origin, "one.com");

            using (var noClientConfiguration = new OptionsScope(o => o.SetClients(new List<ContentApiClient>
            {
                new ContentApiClient()
                {
                    AccessControlAllowOrigin = "*",
                    ClientId = "Default"
                }
            })))
            {
                ClearCorsPolicyCache();

                var contentResponse = await _fixture.Client.SendAsync(requestMessage);

                AssertResponse.OK(contentResponse);
                Assert.Collection(
                    contentResponse.Headers.GetValues(CorsConstants.AccessControlExposeHeaders).SelectMany(x => x.Split(',')),
                    x => Assert.Equal(PagingConstants.ContinuationTokenHeaderName, x),
                    x => Assert.Equal(MetadataHeaderConstants.BranchMetadataHeaderName, x),
                    x => Assert.Equal(MetadataHeaderConstants.ContentGUIDMetadataHeaderName, x),
                    x => Assert.Equal(MetadataHeaderConstants.ContextModeMetadataHeaderName, x),
                    x => Assert.Equal(MetadataHeaderConstants.RemainingRouteMetadataHeaderName, x),
                    x => Assert.Equal(MetadataHeaderConstants.SiteIdMetadataHeaderName, x),
                    x => Assert.Equal(MetadataHeaderConstants.StartPageMetadataHeaderName, x));
            }
        }

        [Fact]
        public async Task Get_WhenOriginDoesNotMatchAnyPolicy_ShouldNotAddAllowHeader()
        {

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri);
            requestMessage.Headers.Add(CorsConstants.Origin, "testing.com");
            using (var noClientConfiguration = new OptionsScope(o => o.SetClients(null)))
            {
                ClearCorsPolicyCache();
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);
                Assert.False(contentResponse.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));
            }
        }

        [Fact]
        public async Task Get_WhenAllowAllOrigin_ShouldReturnAllowAllOrigin()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri);
            requestMessage.Headers.Add(CorsConstants.Origin, "one.com");

            using (var noClientConfiguration = new OptionsScope(o => o.SetClients(new List<ContentApiClient>
            {
                new ContentApiClient()
                {
                    AccessControlAllowOrigin = "*",
                    ClientId = "Default"
                }
            })))
            {
                ClearCorsPolicyCache();

                var contentResponse = await _fixture.Client.SendAsync(requestMessage);

                AssertResponse.OK(contentResponse);
                Assert.Equal(CorsConstants.AnyOrigin, contentResponse.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).First());
            }
        }

        [Fact]
        public async Task PreFlight_WhenOriginMatchPolicy_ShouldAddAllowHeader()
        {
            var hosts = new List<HostDefinition>()
                {
                    new HostDefinition()
                    {
                        Name = "localhost:8878",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("en")
                    },
                      new HostDefinition()
                    {
                        Name = "cd.host01",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("sv")
                    }
                };

            var siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid(),
                Name = "site one",
                SiteUrl = new Uri("http://localhost:8877/"),
                StartPage = new ContentReference(100),
                Hosts = hosts
            };

            var origin = "http://cd.host01:8877";
            var requestMessage = CreateRequest($"{siteDefinition.SiteUrl}" + V2Uri, origin);
            var siteRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
            siteRepository.Save(siteDefinition);

            await _fixture.WithSite(siteDefinition, async () =>
            {
                {
                    ClearCorsPolicyCache();
                    using (var noClientConfiguration = new OptionsScope(o => o.SetClients(null)))
                    {
                        var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                        AssertResponse.OK(contentResponse);
                        var headers = contentResponse.Headers;
                        Assert.Equal(origin, headers.GetValues(CorsConstants.AccessControlAllowOrigin).First());
                        Assert.Equal(CustomHeader, headers.GetValues(CorsConstants.AccessControlAllowHeaders).First());
                    }
                }
            });
        }

        [Fact]
        public async Task PreFlight_WhenAllowAllOrigin_ShouldReturnAllowAllOrigin()
        {
            var requestMessage = CreateRequest(V2Uri, "one.com");
            ClearCorsPolicyCache();
            var contentResponse = await _fixture.Client.SendAsync(requestMessage);

            AssertResponse.OK(contentResponse);
            Assert.Equal(CorsConstants.AnyOrigin, contentResponse.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).First());
        }

        [Fact]
        public async Task PreFlight_WhenOriginDoesNotMatchAnyRules_ShouldNotAllowRequest()
        {
            var requestMessage = CreateRequest(V2Uri, "one.com");
            using (var noClientConfiguration = new OptionsScope(o => o.SetClients(null)))
            {
                ClearCorsPolicyCache();
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.BadRequest(contentResponse);
                Assert.False(contentResponse.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));
            }
        }

        [Fact]
        public async Task PreFlight_WhenSiteUpdated_And_OriginMatchPolicy_ShouldAddAllowHeader()
        {
            var hosts = new List<HostDefinition>()
                {
                    new HostDefinition()
                    {
                        Name = "localhost:8878",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("en")
                    },
                      new HostDefinition()
                    {
                        Name = "cd.host01",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("sv")
                    }
                };

            var newHost = new HostDefinition()
            {
                Name = "localhost:8898",
                Type = HostDefinitionType.Undefined,
                Language = new CultureInfo("en")
            };

            var siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid(),
                Name = "site one",
                SiteUrl = new Uri("http://localhost:8877/"),
                StartPage = new ContentReference(100),
                Hosts = hosts
            };

            var siteRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();

            await _fixture.WithSite(siteDefinition, async () =>
            {
                {
                    ClearCorsPolicyCache();
                    using (var noClientConfiguration = new OptionsScope(o => o.SetClients(null)))
                    {
                        var beforeSiteUpdatedRequestMessage = CreateRequest($"{siteDefinition.SiteUrl}" + V2Uri, "http://localhost:8898");
                        var contentResponse = await _fixture.Client.SendAsync(beforeSiteUpdatedRequestMessage);
                        AssertResponse.BadRequest(contentResponse);
                        Assert.False(contentResponse.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));

                        hosts.Add(newHost);
                        siteDefinition.Hosts = hosts;
                        siteRepository.Save(siteDefinition);

                        var afterSiteUpdatedRequestMessage = CreateRequest($"{siteDefinition.SiteUrl}" + V2Uri, "http://localhost:8898"); ;
                        var response = await _fixture.Client.SendAsync(afterSiteUpdatedRequestMessage);
                        AssertResponse.OK(response);
                        Assert.True(response.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));
                    }
                }
            });
        }

        [Fact]
        public async Task PreFlight_WhenSiteUpdated_And_WhenOriginDoesNotMatchAnyRules_ShouldNotAllowRequest()
        {
            var hosts = new List<HostDefinition>()
                {
                    new HostDefinition()
                    {
                        Name = "localhost:8878",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("en")
                    },
                      new HostDefinition()
                    {
                        Name = "cd.host01",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("sv")
                    }
                };

            var newHosts = new List<HostDefinition>(){
                new HostDefinition
                {
                    Name = "localhost:8898",
                    Type = HostDefinitionType.Primary,
                    Language = new CultureInfo("en")
                }
            };

            var siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid(),
                Name = "site one",
                SiteUrl = new Uri("http://localhost:8877/"),
                StartPage = new ContentReference(100),
                Hosts = hosts
            };

            var siteRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();

            await _fixture.WithSite(siteDefinition, async () =>
            {
                {
                    ClearCorsPolicyCache();
                    using (var noClientConfiguration = new OptionsScope(o => o.SetClients(null)))
                    {
                        var beforeSiteUpdatedRequestMessage = CreateRequest($"{siteDefinition.SiteUrl}" + V2Uri, "http://localhost:8878");
                        var contentResponse = await _fixture.Client.SendAsync(beforeSiteUpdatedRequestMessage);
                        AssertResponse.OK(contentResponse);
                        Assert.True(contentResponse.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));

                        siteDefinition.Hosts = newHosts;
                        siteRepository.Save(siteDefinition);

                        var afterSiteUpdatedRequestMessage = CreateRequest($"{siteDefinition.SiteUrl}" + V2Uri, "http://localhost:8878"); ;
                        var response = await _fixture.Client.SendAsync(afterSiteUpdatedRequestMessage);
                        AssertResponse.BadRequest(response);
                        Assert.False(response.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));
                    }
                }
            });
        }

        private HttpRequestMessage CreateRequest(string url, string origin)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Options, url);
            requestMessage.Headers.Add(CorsConstants.Origin, origin);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestMethod, HttpMethod.Get.Method);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestHeaders, CustomHeader);

            return requestMessage;
        }

        private void ClearCorsPolicyCache()
        {
            var corsPolicyService = ServiceLocator.Current.GetInstance<CorsPolicyService>();
            corsPolicyService.ClearCache(null, null);
        }
    }
}
