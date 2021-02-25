using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Cors;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class Cors
    {
        private readonly ServiceFixture _fixture;

        public Cors(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task List_WhenOriginMatchPolicy_ShouldAddAllowHeader()
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
                SiteUrl = new Uri("http://localhost:6879/"),
                StartPage = new ContentReference(100),
                Hosts = hosts
            };

            var siteRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
            siteRepository.Save(siteDefinition);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, siteDefinition.SiteUrl + ContentTypesController.RoutePrefix);
            var origin = "http://localhost:8878";
            requestMessage.Headers.Add(CorsConstants.Origin, origin);

            await _fixture.WithSite(siteDefinition, async () =>
            {
                {
                    ClearCorsPolicyCache();
                    using (new CorsOptionsScope())
                    {
                        var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                        AssertResponse.OK(contentResponse);
                        Assert.Equal(origin, contentResponse.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).First());
                    }
                }
            });
        }

        [Fact]
        public async Task List_WhenOriginDoesNotMatchAnyPolicy_ShouldNotAddAllowHeader()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentTypesController.RoutePrefix);
            requestMessage.Headers.Add(CorsConstants.Origin, "testing.com");
            using (new CorsOptionsScope())
            {
                ClearCorsPolicyCache();
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);
                Assert.False(contentResponse.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));
            }
        }

        [Fact]
        public async Task List_WhenAllowAllOrigin_ShouldReturnAllowAllOrigin()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentTypesController.RoutePrefix);
            requestMessage.Headers.Add(CorsConstants.Origin, "one.com");

            var policy = new CorsPolicy() { AllowAnyHeader = true, AllowAnyMethod = true, AllowAnyOrigin = true };
            using (var noClientConfiguration = new CorsOptionsScope("localhost", policy))
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
                SiteUrl = new Uri("http://localhost:6879/"),
                StartPage = new ContentReference(100),
                Hosts = hosts
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, siteDefinition.SiteUrl + ContentTypesController.RoutePrefix);
            var origin = "http://localhost:8878";
            requestMessage.Headers.Add(CorsConstants.Origin, origin);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestMethod, HttpMethod.Get.Method);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestHeaders, CorsConstants.Origin);

            var siteRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
            siteRepository.Save(siteDefinition);

            await _fixture.WithSite(siteDefinition, async () =>
            {
                {
                    ClearCorsPolicyCache();
                    using (new CorsOptionsScope())
                    {
                        var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                        AssertResponse.OK(contentResponse);
                        Assert.Equal(origin, contentResponse.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).First());
                    }
                }
            });
        }

        [Fact]
        public async Task PreFlight_WhenAllowAllOrigin_ShouldReturnAllowAllOrigin()
        {
            ClearCorsPolicyCache();
            var policy = new CorsPolicy() { AllowAnyHeader = true, AllowAnyMethod = true, AllowAnyOrigin = true };

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentTypesController.RoutePrefix);
            requestMessage.Headers.Add(CorsConstants.Origin, "one.com");
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestMethod, HttpMethod.Get.Method);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestHeaders, CorsConstants.Origin);

            using (new CorsOptionsScope("localhost", policy))
            {
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);
                Assert.Equal(CorsConstants.AnyOrigin, contentResponse.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).First());
            }
        }

        [Fact]
        public async Task PreFlight_WhenOriginDoesNotMatchAnyRules_ShouldReturnAllowAllOrigin()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentTypesController.RoutePrefix);
            requestMessage.Headers.Add(CorsConstants.Origin, "one.com");
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestMethod, HttpMethod.Get.Method);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestHeaders, CorsConstants.Origin);

            using (new CorsOptionsScope())
            {
                ClearCorsPolicyCache();
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);
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
                    using (var noClientConfiguration = new CorsOptionsScope())
                    {
                        var beforeSiteUpdatedRequestMessage = CreateRequest(siteDefinition.SiteUrl + ContentTypesController.RoutePrefix, "http://localhost:8898");
                        var contentResponse = await _fixture.Client.SendAsync(beforeSiteUpdatedRequestMessage);
                        AssertResponse.OK(contentResponse);
                        Assert.False(contentResponse.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));

                        hosts.Add(newHost);
                        siteDefinition.Hosts = hosts;
                        siteRepository.Save(siteDefinition);

                        var afterSiteUpdatedRequestMessage = CreateRequest(siteDefinition.SiteUrl + ContentTypesController.RoutePrefix, "http://localhost:8898");
                        var response = await _fixture.Client.SendAsync(afterSiteUpdatedRequestMessage);
                        AssertResponse.OK(response);
                        Assert.True(response.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));
                    }
                }
            });
        }

        [Fact]
        public async Task PreFlight_WhenSiteUpdated_And_WhenOriginDoesNotMatchAnyRules_ShouldReturnAllowAllOrigin()
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
                    using (var noClientConfiguration = new CorsOptionsScope())
                    {
                        var beforeSiteUpdatedRequestMessage = CreateRequest(siteDefinition.SiteUrl + ContentTypesController.RoutePrefix, "http://localhost:8878");
                        var contentResponse = await _fixture.Client.SendAsync(beforeSiteUpdatedRequestMessage);
                        AssertResponse.OK(contentResponse);
                        Assert.True(contentResponse.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));

                        siteDefinition.Hosts = newHosts;
                        siteRepository.Save(siteDefinition);

                        var afterSiteUpdatedRequestMessage = CreateRequest(siteDefinition.SiteUrl + ContentTypesController.RoutePrefix, "http://localhost:8878"); ;
                        var response = await _fixture.Client.SendAsync(afterSiteUpdatedRequestMessage);
                        AssertResponse.OK(response);
                        Assert.False(response.Headers.TryGetValues(CorsConstants.AccessControlAllowOrigin, out _));
                    }
                }
            });
        }

        private HttpRequestMessage CreateRequest(string url, string origin)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add(CorsConstants.Origin, origin);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestMethod, HttpMethod.Get.Method);
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestHeaders, CorsConstants.Origin);

            return requestMessage;
        }

        private void ClearCorsPolicyCache()
        {
            var corsPolicyService = ServiceLocator.Current.GetInstance<SiteBasedCorsPolicyService>();
            corsPolicyService.CachePolicies.Clear();
        }
    }
}
