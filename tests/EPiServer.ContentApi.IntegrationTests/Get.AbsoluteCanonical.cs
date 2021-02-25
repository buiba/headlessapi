using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class GetAbsoluteCanonical
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private ServiceFixture _fixture;

        public GetAbsoluteCanonical(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(V2Uri, false)]
        [InlineData(V2Uri, true)]
        public async Task Get_WhenHavingEditHostAndPrimaryHost_AndLinkedContentInMainContentArea_ShouldReturnTheAbsolutePrimaryUrl(string UriBase, bool optimizeForDerlivery)
        {
            var editHost = "edit.com";

            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var contentArea = new ContentArea();
            contentArea.Items.Add(new ContentAreaItem { ContentLink = linkedPage.ContentLink, ContentGuid = linkedPage.ContentGuid });

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.MainContentArea = contentArea;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {
                    JObject content;

                    using (new OptionsScope(optimizeForDerlivery))
                    {
                        var contentResponse = await _fixture.Client.GetAsync(UriBase + $"/{page.ContentGuid}?expand=*");
                        AssertResponse.OK(contentResponse);
                        content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    }

                    Assert.Contains($"http://localhost/en/", content["contentLink"]["url"].ToString());

                    if (!optimizeForDerlivery)
                        Assert.Contains($"http://localhost/en/", content["mainContentArea"]["expandedValue"][0]["contentLink"]["url"].ToString());
                    else
                        Assert.Contains($"http://localhost/en/", content["mainContentArea"][0]["contentLink"]["expanded"]["contentLink"]["url"].ToString());
                                       
                });
            }, false);
        }

        [Theory]
        [InlineData(V2Uri, false)]
        [InlineData(V2Uri, true)]
        public async Task Get_WhenHavingEditHostAndPrimaryHost_AndLinkedContentInLinkItemCollection_ShouldReturnTheAbsolutePrimaryUrl(string UriBase, bool optimizeForDerlivery)
        {
            var editHost = "edit.com";

            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var links = new LinkItemCollection { new LinkItem { Href = UrlResolver.Current.GetUrl(linkedPage), Text = "Link to standard page" } };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Links = links;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {
                    JObject content;
                   
                    using (new OptionsScope(optimizeForDerlivery))
                    {
                        var contentResponse = await _fixture.Client.GetAsync(UriBase + $"/{page.ContentGuid}");
                        AssertResponse.OK(contentResponse);
                        content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    }

                    Assert.Contains($"http://localhost/en/", content["contentLink"]["url"].ToString());

                    if (!optimizeForDerlivery)
                        Assert.Contains($"http://localhost/en/", content["links"]["value"][0]["href"].ToString());
                    else
                        Assert.Contains($"http://localhost/en/", content["links"][0]["href"].ToString());                                 
                });
            }, false);
        }

        [Theory]
        [InlineData(V2Uri)]
        public async Task Get_WhenHavingEditHostAndPrimaryHost_AndLinkedContentInXhtmlString_ShouldReturnTheAbsolutePrimaryUrl(string UriBase)
        {
            var editHost = "edit.com";

            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.MainBody = new XhtmlString(string.Format("<p><a href=\"{0}\">link to standard page</a></p>", UrlResolver.Current.GetUrl(linkedPage)));
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {

                    var contentResponse = await _fixture.Client.GetAsync(UriBase + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost/en/", content["contentLink"]["url"].ToString());
                    Assert.Contains($"http://localhost/en/", content["mainBody"].ToString());
                });
            }, false);
        }

        [Theory]
        [InlineData(V2Uri, false)]
        [InlineData(V2Uri, true)]
        public async Task Get_WhenHavingEditHostAndPrimaryHost_AndLinkedContentInTargetContentReference_ShouldReturnTheAbsolutePrimaryUrl(string UriBase, bool optimizeForDerlivery)
        {
            var editHost = "edit.com";

            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.TargetReference = linkedPage.ContentLink;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {
                    JObject content;
                    using (new OptionsScope(optimizeForDerlivery))
                    {
                        var contentResponse = await _fixture.Client.GetAsync(UriBase + $"/{page.ContentGuid}");
                        AssertResponse.OK(contentResponse);
                        content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    }

                    Assert.Contains($"http://localhost/en/", content["contentLink"]["url"].ToString());

                    if (!optimizeForDerlivery)
                        Assert.Contains($"http://localhost/en/", content["targetReference"]["value"]["url"].ToString());
                    else
                        Assert.Contains($"http://localhost/en/", content["targetReference"]["url"].ToString());                               
                });
            }, false);
        }

        [Theory]
        [InlineData(V2Uri)]
        public async Task Get_WhenHavingEditHostAndPrimaryHost_AndLinkedContentInUrl_ShouldReturnTheAbsolutePrimaryUrl(string UriBase)
        {
            var editHost = "edit.com";

            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Uri = new Url(UrlResolver.Current.GetUrl(linkedPage)); ;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {

                    var contentResponse = await _fixture.Client.GetAsync(UriBase + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost/en/", content["contentLink"]["url"].ToString());
                    Assert.Contains($"http://localhost/en/", content["uri"].ToString());
                });
            }, false);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenThereIsDifferentPrimaryHostForDifferentLanguage_AndUrlPropertySetsSpecificLanguage_ShouldReturnUrlValueWithThePrimaryHostOfTheSpecificLanguage(bool optimizeForDerlivery)
        {
            var enPrimaryHost = "en.host";
            var svPrimaryHost = "sv.host";
            var enPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var svPage = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(enPage.ContentLink, true, "sv");

            var enPageWritable = (enPage.CreateWritableClone() as StandardPage);
            enPageWritable.Uri = _fixture.PermanentLinkMapper.Find(svPage.ContentLink)
                .PermanentLinkUrl.ToString() + "?epslanguage=sv";

            _fixture.ContentRepository.Save(enPageWritable, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);

            await _fixture.WithContent(enPage, async () =>
            {
                await _fixture.WithHosts(new[] { new HostDefinition { Name = enPrimaryHost, Type = HostDefinitionType.Primary, Language = new System.Globalization.CultureInfo("en") }, new HostDefinition { Name = svPrimaryHost, Type = HostDefinitionType.Primary, Language = new System.Globalization.CultureInfo("sv") } }, async () =>
                {
                    using (new OptionsScope(optimizeForDerlivery))
                    {
                        var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{enPage.ContentLink.ToReferenceWithoutVersion()}");
                        AssertResponse.OK(contentResponse);

                        var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                        var uriValue = optimizeForDerlivery ? (string)content["uri"] : (string)content["uri"]["value"];
                        Assert.StartsWith($"http://{svPrimaryHost}", uriValue);
                    }
                });
            }, false);
        }

        [Fact]
        public async Task Get_WhenThereIsDifferentPrimaryHostForDifferentLanguage_AndUrlInLinkCollectionPropertySetsSpecificLanguage_ShouldReturnUrlValueWithThePrimaryHostOfTheSpecificLanguage()
        {
            var enPrimaryHost = "en.host";
            var svPrimaryHost = "sv.host";
            var enPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var svPage = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(enPage.ContentLink, true, "sv");

            var links = new LinkItemCollection { new LinkItem { Href =  _fixture.PermanentLinkMapper.Find(svPage.ContentLink)
                .PermanentLinkUrl.ToString() + "?epslanguage=sv", Text = "Link text" } };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
                p.Links = links;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHosts(new[] { new HostDefinition { Name = enPrimaryHost, Type = HostDefinitionType.Primary, Language = new System.Globalization.CultureInfo("en") }, new HostDefinition { Name = svPrimaryHost, Type = HostDefinitionType.Primary, Language = new System.Globalization.CultureInfo("sv") } }, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.StartsWith($"http://{svPrimaryHost}", (string)content["links"][0]["href"]);
                });
            }, false);

        }

        [Fact]
        public async Task Get_WhenThereIsDifferentPrimaryHostForDifferentLanguage_AndUrlInXhtmlStringSetsSpecificLanguage_ShouldReturnUrlValueWithThePrimaryHostOfTheSpecificLanguage()
        {
            var enPrimaryHost = "en.host";
            var svPrimaryHost = "sv.host";
            var enPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var svPage = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(enPage.ContentLink, true, "sv");

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString(string.Format("<p><a href=\"{0}\">link text</a></p>", _fixture.PermanentLinkMapper.Find(svPage.ContentLink)
                .PermanentLinkUrl.ToString() + "?epslanguage=sv"));
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHosts(new[] { new HostDefinition { Name = enPrimaryHost, Type = HostDefinitionType.Primary, Language = new System.Globalization.CultureInfo("en") }, new HostDefinition { Name = svPrimaryHost, Type = HostDefinitionType.Primary, Language = new System.Globalization.CultureInfo("sv") } }, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://{svPrimaryHost}", (string)content["mainBody"]);
                });
            }, false);
        }


        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenHaveContentNotUnderAnySite_InUrlProperty_ShouldReturnTheAbsolutePrimaryUrl(bool optimizeForDerlivery)
        {
            var editHost = "edit.com";

            await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
            {
                var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);

                var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
                {
                    p.Heading = "The heading";
                    p.Uri = UrlResolver.Current.GetUrl(target);
                });

                await _fixture.WithContent(page, async () =>
                {
                    using (new OptionsScope(optimizeForDerlivery))
                    {
                        var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                        AssertResponse.OK(contentResponse);

                        var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                        var uriValue = optimizeForDerlivery ? (string)content["uri"] : (string)content["uri"]["value"];
                        Assert.StartsWith($"http://localhost", uriValue);
                    }
                }, false);
            });
        }
        
        [Fact]
        public async Task Get_WhenHaveContentNotUnderAnySite_InXhtmlString_AndHaveOnlyPrimaryHost_ShouldReturnTheAbsolutePrimaryUrl()
        {
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString(string.Format("<p><a href=\"{0}\">link to page</a></p>", UrlResolver.Current.GetUrl(target)));
            });

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Contains($"http://localhost", (string)content["mainBody"]);
            }, false);
        }

        [Fact]
        public async Task Get_WhenHaveContentNotUnderAnySite_InXhtmlString_ShouldReturnTheAbsolutePrimaryUrl()
        {
            var editHost = "edit.com";
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString(string.Format("<p><a href=\"{0}\">link to page</a></p>", UrlResolver.Current.GetUrl(target)));
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost", (string)content["mainBody"]);
                });
            }, false);
        }

        [Fact]
        public async Task Get_WhenHaveContentNotUnderAnySite_InContentReference_ShouldReturnTheAbsolutePrimaryUrl()
        {
            var editHost = "edit.com";
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
                p.TargetReference = target.ContentLink;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost", (string)content["targetReference"]["url"]);
                });
            }, false);
        }

        [Fact]
        public async Task Get_WhenHaveContentNotUnderAnySite_InLinkCollection_ShouldReturnTheAbsolutePrimaryUrl()
        {
            var editHost = "edit.com";
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var links = new LinkItemCollection { new LinkItem { Href = UrlResolver.Current.GetUrl(target), Text = "Link to page" } };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
                p.Links = links;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost", (string)content["links"][0]["href"]);
                });
            }, false);
        }

        [Fact]
        public async Task Get_WhenHaveContentNotUnderAnySite_InMainContentArea_ShouldReturnTheAbsolutePrimaryUrl()
        {
            var editHost = "edit.com";
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var contentArea = new ContentArea { Items = { new ContentAreaItem { ContentLink = target.ContentLink } } };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The Heading";
                p.MainContentArea = contentArea;
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHost(new HostDefinition { Name = editHost, Type = HostDefinitionType.Edit }, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand=*");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost/en/", (string)content["mainContentArea"][0]["contentLink"]["expanded"]["contentLink"]["url"]);
                });
            }, false);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async void Get_WhenHaveContentAccrossMultipleSites_AndPropertyUrlLinkToOtherSiteContent_ShouldReturnPrimaryHostOfOtherSite(bool optimizeForDerlivery)
        {
            var hostName = "siteone.com";
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://{hostName}/"),
                    Name = "site test",
                    StartPage = startPage.ContentLink,
                    Hosts = new List<HostDefinition> { new HostDefinition { Name = hostName, Type = HostDefinitionType.Primary, Language = CultureInfo.GetCultureInfo("fi") }
                }
                };
                await _fixture.WithSite(testSite, async () =>
                {
                    var page2 = _fixture.GetWithDefaultName<StandardPage>(startPage.ContentLink, true);
                    var page1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: page =>
                    {
                        page.Uri = _fixture.PermanentLinkMapper.Find(page2.ContentLink).PermanentLinkUrl.ToString();
                    });
                    using (new OptionsScope(optimizeForDerlivery))
                    {
                        var contentRes = await _fixture.Client.GetAsync(V2Uri + $"/{page1.ContentLink}");
                        var content = JObject.Parse(await contentRes.Content.ReadAsStringAsync());
                        var uriValue = optimizeForDerlivery ? content["uri"] : content["uri"]["value"];
                        Assert.Contains(hostName, uriValue.ToString());
                    }

                });
            }, false);
        }
        
        [Fact]
        public async void Get_WhenHaveContentAccrossMultipleSites_AndPropertyLinkItemLinkToOtherSiteContent_ShouldReturnPrimaryHostOfOtherSite()
        {
            var hostName = "siteone.com";
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://{hostName}/"),
                    Name = "site one",
                    StartPage = startPage.ContentLink,
                    Hosts = new List<HostDefinition> { new HostDefinition { Name = hostName, Type = HostDefinitionType.Primary, Language = CultureInfo.GetCultureInfo("fi") }
                }
                };
                await _fixture.WithSite(testSite, async () =>
                {
                    var page2 = _fixture.GetWithDefaultName<StandardPage>(startPage.ContentLink, true);
                    var links = new LinkItemCollection { new LinkItem { Href = UrlResolver.Current.GetUrl(page2), Text = "Link to another page" } };
                    var page1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: page =>
                    {
                        page.Links = links;
                    });

                    var contentRes = await _fixture.Client.GetAsync(V2Uri + $"/{page1.ContentGuid}");
                    var content = JObject.Parse(await contentRes.Content.ReadAsStringAsync());
                    var linkItems = content["links"][0]["href"];
                    Assert.Contains(hostName, linkItems.ToString());

                });
            }, false);
        }

        [Fact]
        public async void Get_WhenHaveContentAccrossMultipleSites_AndPropertyContentAreaItemLinkToOtherSiteContent_ShouldReturnPrimaryHostOfOtherSite()
        {
            var hostName = "siteone.com";
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://{hostName}/"),
                    Name = "site one",
                    StartPage = startPage.ContentLink,
                    Hosts = new List<HostDefinition> { new HostDefinition { Name = hostName, Type = HostDefinitionType.Primary, Language = CultureInfo.GetCultureInfo("fi") }
                }
                };
                await _fixture.WithSite(testSite, async () =>
                {
                    var page2 = _fixture.GetWithDefaultName<StandardPage>(startPage.ContentLink, true);
                    var contentArea = new ContentArea();
                    contentArea.Items.Add(new ContentAreaItem { ContentLink = page2.ContentLink, ContentGuid = page2.ContentGuid });
                    
                    var page1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: page =>
                    {
                        page.MainContentArea = contentArea;
                    });

                    var contentRes = await _fixture.Client.GetAsync(V2Uri + $"/{page1.ContentGuid}?expand=*");
                    var content = JObject.Parse(await contentRes.Content.ReadAsStringAsync());

                    Assert.Contains(hostName, content["mainContentArea"][0]["contentLink"]["expanded"]["contentLink"]["url"].ToString());
                });
            }, false);
        }

        [Fact]
        public async void Get_WhenHaveContentAccrossMultipleSites_AndPropertyXhtmlStringLinkToOtherSiteContent_ShouldReturnPrimaryHostOfOtherSite()
        {
            var hostName = "siteone.com";
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://{hostName}/"),
                    Name = "site one",
                    StartPage = startPage.ContentLink,
                    Hosts = new List<HostDefinition> { new HostDefinition { Name = hostName, Type = HostDefinitionType.Primary, Language = CultureInfo.GetCultureInfo("fi") }
                }
                };
                await _fixture.WithSite(testSite, async () =>
                {
                    var page2 = _fixture.GetWithDefaultName<StandardPage>(startPage.ContentLink, true);
                    var permanentLink = _fixture.PermanentLinkMapper.Find(page2.ContentLink);
                    var page1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: page =>
                    {
                        page.MainBody = new XhtmlString($"<a href=\"{permanentLink.PermanentLinkUrl}\">{page2.Name}</a>");
                    });

                    var contentRes = await _fixture.Client.GetAsync(V2Uri + $"/{page1.ContentGuid}");
                    var content = JObject.Parse(await contentRes.Content.ReadAsStringAsync());
                    Assert.Contains(hostName, content["mainBody"].ToString());
                });
            }, false);
        }

        [Fact]
        public async void Get_WhenHaveContentAccrossMultipleSites_AndPropertyTargetReferenceLinkToOtherSiteContent_ShouldReturnPrimaryHostOfOtherSite()
        {
            var hostName = "siteone.com";
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://{hostName}/"),
                    Name = "site one",
                    StartPage = startPage.ContentLink,
                    Hosts = new List<HostDefinition> { new HostDefinition { Name = hostName, Type = HostDefinitionType.Primary, Language = CultureInfo.GetCultureInfo("fi") }
                }
                };
                await _fixture.WithSite(testSite, async () =>
                {
                    var page2 = _fixture.GetWithDefaultName<StandardPage>(startPage.ContentLink, true);
                    var page1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: page =>
                    {
                        page.TargetReference = page2.ContentLink;
                    });

                    var contentRes = await _fixture.Client.GetAsync(V2Uri + $"/{page1.ContentGuid}");
                    var content = JObject.Parse(await contentRes.Content.ReadAsStringAsync());
                    Assert.Contains(hostName, content["targetReference"]["url"].ToString());
                });
            }, false);
        }

        [Fact]
        public async void Get_WhenHaveContentAccrossMultipleSites_AndPropertyContentReferenceListLinkToOtherSiteContent_ShouldReturnPrimaryHostOfOtherSite()
        {
            var hostName = "siteone.com";
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var testSite = new SiteDefinition
                {
                    SiteUrl = new Uri($"http://{hostName}/"),
                    Name = "site one",
                    StartPage = startPage.ContentLink,
                    Hosts = new List<HostDefinition> { new HostDefinition { Name = hostName, Type = HostDefinitionType.Primary, Language = CultureInfo.GetCultureInfo("fi") }
                }
                };
                await _fixture.WithSite(testSite, async () =>
                {
                    var page2 = _fixture.GetWithDefaultName<StandardPage>(startPage.ContentLink, true);
                    var page1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: page =>
                    {
                        page.ContentReferenceList = new List<ContentReference>() { page2.ContentLink };
                    });

                    var contentRes = await _fixture.Client.GetAsync(V2Uri + $"/{page1.ContentGuid}");
                    var content = JObject.Parse(await contentRes.Content.ReadAsStringAsync());
                    Assert.Contains(hostName, content["contentReferenceList"][0]["url"].ToString());
                });
            }, false);
        }
    }
}
