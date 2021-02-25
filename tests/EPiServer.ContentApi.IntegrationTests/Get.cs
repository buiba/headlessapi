using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Media;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class Get
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private readonly ServiceFixture _fixture;

        public Get(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_WhenContentReferenceIsInvalid_ShouldThrowBadRequest()
        {            
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/invalid-content-reference");
            AssertResponse.BadRequest(contentResponse);
            var errorResponse = await contentResponse.Content.ReadAs<ErrorResponse>();
            Assert.Equal(ErrorCode.InvalidParameter, errorResponse.Error.Code);
            Assert.Equal("The content reference is not in a valid format", errorResponse.Error.Message);
        }

        [Fact]
        public async Task Get_WhenParentDoesNotExistInCurrentLanguage_ShouldGetParentFromMaster()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, language: "en");
            var page = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true, language: "sv");
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(parent.ContentGuid, new Guid((string)content["parentLink"]["guidValue"]));
            }, false);
        }

        [Fact]
        public async Task Get_ShouldAllowOptionsMethod()
        {            
            var request = new HttpRequestMessage(HttpMethod.Options, V2Uri + "/some-content-link");

            var contentResponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(contentResponse);    
        }

        [Fact]
        public async Task Get_WhenRequestedWithContentReferenceAsString_ShouldReturnContent()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(page.Name, (string)content["name"]);
            });
        }

        [Fact]
        public async Task Get_WhenRequestedWithContentReferenceAsString_ShouldReturnContentWithoutMetadataHeaders()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                Assert.False(contentResponse.Headers.TryGetValues(MetadataHeaderConstants.ContentGUIDMetadataHeaderName, out var _));
                Assert.False(contentResponse.Headers.TryGetValues(MetadataHeaderConstants.BranchMetadataHeaderName, out var _));
            });
        }

        [Fact]
        public async Task Get_WithCustomProperty_ShouldReturnContentWithProperty()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.Heading = "My Heading");
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(page.Heading, (string)content["heading"]);
            });
        }

        [Fact]
        public async Task Get_WithExpandedProperty_ShouldReturnContentWithReferencedContent()
        {
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var contentArea = new ContentArea { Items = { new ContentAreaItem { ContentLink = target.ContentLink } } };
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);
            await _fixture.WithContentItems(new[] { page, target }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand={nameof(StandardPage.MainContentArea)}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var contentAreaItem = Assert.Single(content["mainContentArea"]);
                var expanded = contentAreaItem.SelectToken("contentLink.expanded");
                Assert.NotNull(expanded);
                Assert.Equal(target.ContentGuid.ToString(), expanded.SelectToken("contentLink.guidValue")?.Value<string>());
            });
        }

        [Fact]
        public async Task Get_WithExpandedPropertyAndNoTranslatedContent_ShouldReturnEmptyProperty()
        {
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "sv");
            var contentArea = new ContentArea { Items = { new ContentAreaItem { ContentLink = target.ContentLink } } };
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);
            await _fixture.WithContentItems(new[] { page, target }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand={nameof(StandardPage.MainContentArea)}");
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Empty(content["mainContentArea"]);
            });
        }

        [Fact]
        public async Task Get_WithExpandedPropertyAndMissingTranslatedContent_ShouldReturnOnyTranslatedContent()
        {
            var targetEnglish = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var targetSwedish = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "sv");

            var contentArea = new ContentArea
            {
                Items =
                {
                    new ContentAreaItem { ContentLink = targetEnglish.ContentLink },
                    new ContentAreaItem { ContentLink = targetSwedish.ContentLink }
                }
            };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContentItems(new[] { page, targetEnglish, targetSwedish }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand={nameof(StandardPage.MainContentArea)}");
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());

                var expandedGuids = content
                    .SelectTokens("mainContentArea..contentLink.expanded.contentLink.guidValue")
                    .Values<string>();

                Assert.Single(expandedGuids);
                Assert.Contains(targetEnglish.ContentGuid.ToString(), expandedGuids);
                Assert.DoesNotContain(targetSwedish.ContentGuid.ToString(), expandedGuids);
            });
        }

        [Fact]
        public async Task Get_WhenRequestedWithoutExpandQuery_ShouldExpandOnlyOneLevelProperty()
        {
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var contentArea = new ContentArea { Items = { new ContentAreaItem { ContentLink = target.ContentLink } } };
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => { c.MainContentArea = contentArea; c.Heading = "My Heading"; });
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(page.Heading, (string)content["heading"]);
                var contentAreaItem = Assert.Single(content["mainContentArea"]);
                var expanded = contentAreaItem.SelectToken("contentLink.expanded");
                Assert.Null(expanded);

            });
        }

        [Fact]
        public async Task Get_WhenThereIsCircularDependency_ShouldWorks()
        {
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var source = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.TargetReference = target.ContentLink);

            await _fixture.WithContent(source, async () =>
            {
                ConnectTargetToSource(target, source);

                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{source.ContentGuid}?expand={nameof(StandardPage.TargetReference)}");
                await ValidateReference(contentResponse, source.ContentGuid, target.ContentGuid);

                contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{target.ContentGuid}?expand={nameof(StandardPage.TargetReference)}");
                await ValidateReference(contentResponse, target.ContentGuid, source.ContentGuid);

            });

            void ConnectTargetToSource(StandardPage des, StandardPage src)
            {
                var targetPage = (des.CreateWritableClone() as StandardPage);
                targetPage.TargetReference = src.ContentLink.ToPageReference();
                _fixture.ContentRepository.Save(targetPage, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);
            }

            static async Task ValidateReference(HttpResponseMessage contentResponse, Guid expectedSource, Guid expectedTarget)
            {
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var targetPageReferece = content[ToCamelCase(nameof(StandardPage.TargetReference))];
                var targetLink = targetPageReferece.SelectToken("expanded")[ToCamelCase(nameof(StandardPage.ContentLink))];
                Assert.Equal(expectedTarget, new Guid((string)targetLink["guidValue"]));
                var sourceReference = targetPageReferece.SelectToken("expanded")[ToCamelCase(nameof(StandardPage.TargetReference))];
                Assert.NotNull(sourceReference);
                Assert.Equal(expectedSource, new Guid((string)sourceReference["guidValue"]));
            }

            static string ToCamelCase(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

        }

        [Fact]
        public async Task Get_WhenABooleanIsNotAssigned_ShouldNotBeIncluded()
        {
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage);
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content[nameof(PropertyPage.Boolean).ToLower()]);
            });
        }

        [Fact]
        public async Task Get_WhenABooleanIsAssignedToFalse_ShouldBeIncluded()
        {
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, init: p => p.Boolean = false);
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.False((bool)content[nameof(PropertyPage.Boolean).ToLower()]);
            });
        }

        [Fact]
        public async Task Get_WhenAComplexPropertyIsNull_ShouldNotIncludeProperty()
        {
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage);
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content["contentArea"]);
            });
        }

        [Fact]
        public async Task Get_WhenANestedComplexPropertyIsNull_ShouldNotIncludeProperty()
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var contentArea = new ContentArea();
            contentArea.Items.Add(new ContentAreaItem { ContentLink = (block as IContent).ContentLink, ContentGuid = (block as IContent).ContentGuid });
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.ContentArea = contentArea);
            await _fixture.WithContentItems(new IContent[] { (IContent)block, page }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand=contentArea");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.NotNull((content["contentArea"] as JArray)[0]["contentLink"]);
                Assert.Null((content["contentArea"] as JArray)[0]["contentLink"]["expanded"]["heading"]);
            }, false);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenConfiguredToIncludeNullvalues_ShouldIncludeNullValue(bool optimizeForDerlivery)
        {
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage);
            await _fixture.WithContent(page, async () =>
            {
                ServiceLocator.Current.GetInstance<JsonSerializer>().Settings = null;
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    if (!optimizeForDerlivery)
                        Assert.NotNull(content["contentArea"]);
                    else
                        Assert.Null(content["contentArea"]);
                }
                ServiceLocator.Current.GetInstance<JsonSerializer>().Settings = null;
            });
        }

        [Fact]
        public async Task Get_WhenPropertyIsUrl_ShouldBeSerialized()
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var contentArea = new ContentArea();
            contentArea.Items.Add(new ContentAreaItem { ContentLink = (block as IContent).ContentLink, ContentGuid = (block as IContent).ContentGuid });
            var contentRef = _fixture.GetWithDefaultName<PropertyPage>(SiteDefinition.Current.StartPage, true);
            var url = _fixture.PermanentLinkMapper.Find(contentRef.ContentLink).PermanentLinkUrl;
            block.TextLink = url.ToString();
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.ContentArea = contentArea);
            await _fixture.WithContentItems(new IContent[] { (IContent)block, page }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand=contentArea");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.NotNull((content["contentArea"] as JArray)[0]["contentLink"]["expanded"]["textLink"]);
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenPropertiesAreSelected_ShouldIncludeSpecifiedProperties(bool optimizeForDerlivery)
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
            });
            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?select=heading");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Null(content["mainBody"]);
                    var headingValue = optimizeForDerlivery ? (string)content["heading"] : (string)content["heading"]["value"];
                    Assert.Equal(page.Heading, headingValue);
                }
            });
        }

        [Fact]
        public async Task Get_WhenMetadataPropertiesAreSelected_ShouldIncludeSpecifiedProperties()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
            });
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?select=url");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content["mainBody"]);
                Assert.Equal(UrlResolver.Current.GetUrl(page.ContentLink, null, new VirtualPathArguments { ValidateTemplate = false }), (string)content["url"]);
            });
        }

        [Fact]
        public async Task Get_WhenPropertiesAreSelected_ShouldIncludeContentLinkAndName()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
            });
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?select=heading");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(page.Name, (string)content["name"]);
                Assert.Equal(page.ContentGuid, new Guid((string)content["contentLink"]["guidValue"]));
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_ByContentReference_WhenPropertiesAreSelected_ShouldIncludeSpecifiedProperties(bool optimizeForDerlivery)
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
            });
            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?select=heading");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Null(content["mainBody"]);
                    var headingValue = optimizeForDerlivery ? (string)content["heading"] : (string)content["heading"]["value"];
                    Assert.Equal(page.Heading, headingValue);
                }
            });
        }

        [Fact]
        public async Task Get_ByContentReference_WhenMetadataPropertiesAreSelected_ShouldIncludeSpecifiedProperties()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
            });
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?select=url");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content["mainBody"]);
                Assert.Equal(UrlResolver.Current.GetUrl(page.ContentLink, null, new VirtualPathArguments { ValidateTemplate = false }), (string)content["url"]);
            });
        }

        [Fact]
        public async Task Get_ByContentReference_WhenPropertiesAreSelected_ShouldIncludeContentLinkAndName()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
            });
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?select=heading");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(page.Name, (string)content["name"]);
                Assert.Equal(page.ContentGuid, new Guid((string)content["contentLink"]["guidValue"]));
            });
        }

        [Fact]
        public async Task Get_WhenContentIsMedia_AndHaveBothPrimaryAndEdit_ShouldReturnTheAbsoluteEditUrl()
        {
            var primaryHost = "primary.com";
            var hosts = new List<HostDefinition>
            {
                new HostDefinition {Name="localhost", Type = HostDefinitionType.Edit},
                new HostDefinition {Name=primaryHost, Type = HostDefinitionType.Primary},
                new HostDefinition {Name=HostDefinition.WildcardHostName}
            };
            await _fixture.WithHosts(hosts, async () =>
            {
                var parents = new[] { ContentReference.StartPage, ContentReference.GlobalBlockFolder, ContentReference.SiteBlockFolder, SiteDefinition.Current.SiteAssetsRoot };
                foreach (var parent in parents)
                {
                    var media = _fixture.GetWithDefaultName<DefaultMedia>(parent, true, init: p =>
                    {
                        p.Description = "media description";
                    });

                    await _fixture.WithContent(media, async () =>
                    {
                        var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{media.ContentGuid}");
                        AssertResponse.OK(contentResponse);

                        var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                        Assert.StartsWith($"http://localhost", (string)content["url"]);
                    }, false);
                }
            }, true);
        }

        [Fact]
        public async Task Get_WhenContentIsMedia_AndHaveOnlyPrimary_ShouldReturnTheAbsolutePrimaryUrl()
        {
            var parents = new[] { ContentReference.StartPage, ContentReference.GlobalBlockFolder, ContentReference.SiteBlockFolder, SiteDefinition.Current.SiteAssetsRoot };
            foreach (var parent in parents)
            {
                var media = _fixture.GetWithDefaultName<DefaultMedia>(parent, true, init: p =>
                {
                    p.Description = "media description";
                });

                await _fixture.WithContent(media, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{media.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.StartsWith($"http://localhost", (string)content["url"]);
                }, false);
            }
        }
        
        [Fact]
        public async Task Get_WhenContentMediaInXhtmlString_ShouldReturnTheAbsoluteEditUrl()
        {
            var primaryHost = "primary.com";
            var media = _fixture.GetWithDefaultName<DefaultMedia>(ContentReference.RootPage, true, init: p =>
            {
                p.Description = "media description";
            });

            var mediaLink = _fixture.PermanentLinkMapper.Find(media.ContentLink);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString($"<p><a href=\"{mediaLink.PermanentLinkUrl.ToString()}\">link to media</a></p>");
            });

            var hosts = new List<HostDefinition>
            {
                new HostDefinition {Name="localhost", Type = HostDefinitionType.Edit},
                new HostDefinition {Name=primaryHost, Type = HostDefinitionType.Primary},
                new HostDefinition {Name=HostDefinition.WildcardHostName}
            };

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHosts(hosts, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost", (string)content["mainBody"]);
                }, true);
            }, false);
        }

        [Fact]
        public async Task Get_WhenContentMediaIsTargetContentReference_ShouldReturnTheAbsoluteEditUrl()
        {
            var primaryHost = "primary.com";
            var media = _fixture.GetWithDefaultName<DefaultMedia>(ContentReference.RootPage, true, init: p =>
            {
                p.Description = "media description";
            });

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
                p.TargetReference = media.ContentLink;


            });
            var hosts = new List<HostDefinition>
            {
                new HostDefinition {Name="localhost", Type = HostDefinitionType.Edit},
                new HostDefinition {Name=primaryHost, Type = HostDefinitionType.Primary},
                new HostDefinition {Name=HostDefinition.WildcardHostName}
            };

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHosts(hosts, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost", (string)content["targetReference"]["url"]);
                }, true);
            }, false);
        }

        [Fact]
        public async Task Get_WhenContentMediaInLinkCollection_ShouldReturnTheAbsoluteEditUrl()
        {
            var primaryHost = "primary.com";
            var media = _fixture.GetWithDefaultName<DefaultMedia>(ContentReference.RootPage, true, init: p =>
            {
                p.Description = "media description";
            });

            var links = new LinkItemCollection { new LinkItem { Href = UrlResolver.Current.GetUrl(media), Text = "Link to media" } };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
                p.Links = links;


            });

            var hosts = new List<HostDefinition>
            {
                new HostDefinition {Name="localhost", Type = HostDefinitionType.Edit},
                new HostDefinition {Name=primaryHost, Type = HostDefinitionType.Primary},
                new HostDefinition {Name=HostDefinition.WildcardHostName}
            };

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHosts(hosts, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost", (string)content["links"][0]["href"]);
                }, true);
            }, false);
        }

        [Fact]
        public async Task Get_WhenContentMediaIsUrl_ShouldReturnTheAbsoluteEditUrl()
        {
            var primaryHost = "primary.com";
            var media = _fixture.GetWithDefaultName<DefaultMedia>(ContentReference.RootPage, true, init: p =>
            {
                p.Description = "media description";
            });

            var links = new LinkItemCollection { new LinkItem { Href = UrlResolver.Current.GetUrl(media), Text = "Link to media" } };

            var page = _fixture.GetWithDefaultName<StandardPageWithPageImage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
                p.PageImage = new Url(UrlResolver.Current.GetUrl(media));
            });

            var hosts = new List<HostDefinition>
            {
                new HostDefinition {Name="localhost", Type = HostDefinitionType.Edit},
                new HostDefinition {Name=primaryHost, Type = HostDefinitionType.Primary},
                new HostDefinition {Name=HostDefinition.WildcardHostName}
            };

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.WithHosts(hosts, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Contains($"http://localhost", (string)content["pageImage"]);
                }, true);
            }, false);
        }

        [Fact]
        public async Task Get_WhenFallbackIsApplied_ShouldReturnContentUrlFollowingHeaderLanguage()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var parentFallbackSetting = new ContentLanguageSetting(ContentReference.StartPage, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(parentFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal("http://localhost/sv/", content["parentLink"]["url"].ToString());
            });
        }


        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenFallbackIsApplied_AndLinkItemPropertySetsAutomaticLanguage_ShouldReturnLinkItemValueWithRequestLanguage(bool optimizeForDerlivery)
        {
            // only has content on `en` branch 
            var pageOne = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
            });

            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageOne.ContentLink, "sv", "sv", new string[] { "en" }, true));
            var permanentLLinkPageOne = _fixture.PermanentLinkMapper.Find(pageOne.ContentLink);

            // page two links to page one
            var pageTwo = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                var links = new List<LinkItem> {new LinkItem
                {
                    Target = null,
                    Text = "Go to Page 2",
                    Title = " Page 2",
                    // no specific language is set on href => automatically
                    Href = permanentLLinkPageOne.PermanentLinkUrl.ToString()
                 }};
                p.Links = new SpecializedProperties.LinkItemCollection(links);
            });
            // setup fallback language sv to en
            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageTwo.ContentLink, "sv", "sv", new string[] { "en" }, true));

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{pageTwo.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { pageOne, pageTwo }, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var linkItems = optimizeForDerlivery ? (JArray)content["links"] : (JArray)content["links"]["value"];
                    var linkHref = linkItems.FirstOrDefault()?["href"]?.Value<string>();

                    Assert.Equal("http://localhost/sv/my-page/", linkHref);
                }
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenFallbackIsApplied_AndLinkItemPropertySetsSpecificLanguage_ShouldReturnLinkItemValueWithSpecificLanguage(bool optimizeForDerlivery)
        {
            // only has content on `en` branch 
            var pageOne = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
            });

            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageOne.ContentLink, "sv", "sv", new string[] { "en" }, true));
            var permanentLLinkPageOne = _fixture.PermanentLinkMapper.Find(pageOne.ContentLink);

            // page two links to page one
            var pageTwo = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                var links = new List<LinkItem> {new LinkItem
                {
                    Target = null,
                    Text = "Go to Page 2",
                    Title = " Page 2",
                    // Point to a specific language
                    Href = permanentLLinkPageOne.PermanentLinkUrl.ToString() + "?epslanguage=en"
                 }};
                p.Links = new SpecializedProperties.LinkItemCollection(links);
            });
            // setup fallback language sv to en
            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageTwo.ContentLink, "sv", "sv", new string[] { "en" }, true));

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{pageTwo.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { pageOne, pageTwo }, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var linkItems = optimizeForDerlivery ? (JArray)content["links"] : (JArray)content["links"]["value"];
                    var linkHref = linkItems.FirstOrDefault()?["href"]?.Value<string>();

                    Assert.Equal("http://localhost/en/my-page/", linkHref);
                }
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenFallbackIsApplied_AndUrlPropertySetsAutomaticLanguage_ShouldReturnLinkItemValueWithRequestLanguage(bool optimizeForDerlivery)
        {
            // only has content on `en` branch 
            var pageOne = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
            });

            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageOne.ContentLink, "sv", "sv", new string[] { "en" }, true));
            var permanentLLinkPageOne = _fixture.PermanentLinkMapper.Find(pageOne.ContentLink);

            // page two links to page one
            var pageTwo = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Uri = permanentLLinkPageOne.PermanentLinkUrl.ToString();
            });
            // setup fallback language sv to en
            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageTwo.ContentLink, "sv", "sv", new string[] { "en" }, true));

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{pageTwo.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { pageOne, pageTwo }, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var uriProperty = optimizeForDerlivery ? (string)content["uri"] : (string)content["uri"]["value"];

                    Assert.Equal("http://localhost/sv/my-page/", uriProperty);
                }
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenFallbackIsApplied_AndUrlPropertySetSpecificLanguage_ShouldReturnLinkItemValueWithSpecificLanguage(bool optimizeForDerlivery)
        {
            // only has content on `en` branch 
            var pageOne = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
            });

            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageOne.ContentLink, "sv", "sv", new string[] { "en" }, true));
            var permanentLLinkPageOne = _fixture.PermanentLinkMapper.Find(pageOne.ContentLink);

            // page two links to page one
            var pageTwo = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Uri = permanentLLinkPageOne.PermanentLinkUrl.ToString() + "?epslanguage=en";
            });
            // setup fallback language sv to en
            _fixture.SaveContentLanguageSetting(new ContentLanguageSetting(pageTwo.ContentLink, "sv", "sv", new string[] { "en" }, true));

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{pageTwo.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { pageOne, pageTwo }, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var uriProperty = optimizeForDerlivery ? (string)content["uri"] : (string)content["uri"]["value"];

                    Assert.Equal("http://localhost/en/my-page/", uriProperty);
                }
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenFallbackIsApplied_ShouldReturnURLInRequestLanguageForContentReferenceProperty(bool optimizeForDerlivery)
        {
            var linkedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "Linked Content";
            });
            var childFallbackSetting = new ContentLanguageSetting(linkedContent.ContentLink, "sv", null, new string[] { "en" });
            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(childFallbackSetting);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
                p.TargetReference = linkedContent.ContentLink;
            });
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { linkedContent, page }, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var targetReferenceUrl = optimizeForDerlivery ? content["targetReference"]["url"] : content["targetReference"]["value"]["url"];

                    Assert.Equal("http://localhost/sv/linked-content/", targetReferenceUrl);
                }
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenFallbackIsApplied_ShouldReturnURLInRequestLanguageForItemsInContentReferenceListProperty(bool optimizeForDerlivery)
        {
            var firstContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "first content";
            });
            var itemFallbackSetting = new ContentLanguageSetting(firstContent.ContentLink, "sv", null, new string[] { "en" });
            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(itemFallbackSetting);

            var secondContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "second content";
            });
            itemFallbackSetting = new ContentLanguageSetting(secondContent.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(itemFallbackSetting);

            var thirdContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "third content";
            });
            itemFallbackSetting = new ContentLanguageSetting(thirdContent.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(itemFallbackSetting);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
                p.ContentReferenceList = new List<ContentReference>() { firstContent.ContentLink, secondContent.ContentLink, thirdContent.ContentLink };
            });
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { firstContent, secondContent, thirdContent, page }, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var allItems = optimizeForDerlivery ? (JArray)content["contentReferenceList"] : (JArray)content["contentReferenceList"]["value"];

                    Assert.Equal("http://localhost/sv/first-content/", allItems[0]["url"]);
                    Assert.Equal("http://localhost/sv/second-content/", allItems[1]["url"]);
                    Assert.Equal("http://localhost/sv/third-content/", allItems[2]["url"]);
                }
            });
        }


        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenFallbackIsApplied_AndXhtmlString_ShouldReturnURLItemsInRequestLanguage(bool optimizeForDerlivery)
        {
            var item = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "Page item";
            });
            var childFallbackSetting = new ContentLanguageSetting(item.ContentLink, "sv", null, new string[] { "en" });
            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(childFallbackSetting);
            var pẻmanentLink = _fixture.PermanentLinkMapper.Find(item.ContentLink);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
                p.MainBody = new XhtmlString($"<a href=\"{pẻmanentLink.PermanentLinkUrl}\">{item.Name}</a>");
            });
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentLink.ToReferenceWithoutVersion()}?expand=*");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { item, page }, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);

                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var mainBodyValue = optimizeForDerlivery ? content["mainBody"].ToString() : content["mainBody"]["value"].ToString();

                    Assert.Equal("<a href=\"http://localhost/sv/page-item/\">Page item</a>", mainBodyValue);
                }
            });
        }

        [Theory]
        [InlineData(ExpandedLanguageBehavior.ContentLanguage)]
        [InlineData(ExpandedLanguageBehavior.RequestedLanguage)]
        public async Task Get_ContentArea_WhenFallbackApplied_WithExpandedProperty_ShouldReturnContent_UsingExpandedBehaviour(ExpandedLanguageBehavior expandedLanguageBehaviour)
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var blockSv = _fixture.CreateLanguageBranchWithDefaultName<TextBlock>((block as IContent).ContentLink, true, "sv");
            var contentArea = new ContentArea
            {
                Items = { new ContentAreaItem { ContentLink = (block as IContent).ContentLink, ContentGuid = (block as IContent).ContentGuid } }
            };
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: c => c.MainContentArea = contentArea);

            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentGuid}?expand=*");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new IContent[] { (IContent)block, page }, async () =>
            {
                using (new OptionsScope(o => o.SetExpandedBehavior(expandedLanguageBehaviour)))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var contentAreaItem = Assert.Single(content["mainContentArea"]);
                    var expanded = contentAreaItem.SelectToken("contentLink.expanded");
                    var expectedValue = expandedLanguageBehaviour == ExpandedLanguageBehavior.RequestedLanguage ? "sv" : "en";

                    Assert.NotNull(expanded);
                    Assert.Equal(expectedValue, expanded.SelectToken("language.name")?.Value<string>());
                }
            });
        }

        [Theory]
        [InlineData(ExpandedLanguageBehavior.ContentLanguage)]
        [InlineData(ExpandedLanguageBehavior.RequestedLanguage)]
        public async Task Get_ContentArea_WhenFallbackApplied_WithExpandedPropertyAlsoHavingFallbackBehavior_ShouldReturnContent_UsingExpandedBehaviour(ExpandedLanguageBehavior expandedLanguageBehaviour)
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var blockFallbackSetting = new ContentLanguageSetting((block as IContent).ContentLink, "sv", null, new string[] { "en" });

            var contentArea = new ContentArea
            {
                Items = { new ContentAreaItem { ContentLink = (block as IContent).ContentLink, ContentGuid = (block as IContent).ContentGuid } }
            };
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: c => c.MainContentArea = contentArea);

            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });

            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(pageFallbackSetting);
            contentLanguageSettingRepository.Save(blockFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentGuid}?expand=*");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new IContent[] { (IContent)block, page }, async () =>
            {
                using (new OptionsScope(o => o.SetExpandedBehavior(expandedLanguageBehaviour)))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var contentAreaItem = Assert.Single(content["mainContentArea"]);
                    var expanded = contentAreaItem.SelectToken("contentLink.expanded");
                    var expectedValue = "en";

                    Assert.NotNull(expanded);
                    Assert.Equal(expectedValue, expanded.SelectToken("language.name")?.Value<string>());
                }
            });
        }

        [Theory]
        [InlineData(ExpandedLanguageBehavior.ContentLanguage)]
        [InlineData(ExpandedLanguageBehavior.RequestedLanguage)]
        public async Task Get_ContentArea_WhenFallbackApplied_WithExpandedPropertyAlsoHavingLanguageReplacementBehavior_ShouldReturnContent_UsingExpandedBehaviour(ExpandedLanguageBehavior expandedLanguageBehaviour)
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var blockSv = _fixture.CreateLanguageBranchWithDefaultName<TextBlock>((block as IContent).ContentLink, true, "sv");

            var blockReplacementLanguageSetting = new ContentLanguageSetting((block as IContent).ContentLink, "sv", "en", null);

            var contentArea = new ContentArea
            {
                Items = { new ContentAreaItem { ContentLink = (block as IContent).ContentLink, ContentGuid = (block as IContent).ContentGuid } }
            };
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: c => c.MainContentArea = contentArea);

            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });

            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(pageFallbackSetting);
            contentLanguageSettingRepository.Save(blockReplacementLanguageSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentGuid}?expand=*");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new IContent[] { (IContent)block, page }, async () =>
            {
                using (new OptionsScope(o => o.SetExpandedBehavior(expandedLanguageBehaviour)))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var contentAreaItem = Assert.Single(content["mainContentArea"]);
                    var expanded = contentAreaItem.SelectToken("contentLink.expanded");
                    var expectedValue = "en";

                    Assert.NotNull(expanded);
                    Assert.Equal(expectedValue, expanded.SelectToken("language.name")?.Value<string>());
                }
            });
        }

        [Theory]
        [InlineData(ExpandedLanguageBehavior.ContentLanguage)]
        [InlineData(ExpandedLanguageBehavior.RequestedLanguage)]
        public async Task Get_TargetReference_WhenFallbackApplied_WithExpandedProperty_ShouldReturnContent_UsingExpandedBehaviour(ExpandedLanguageBehavior expandedLanguageBehaviour)
        {
            var target = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var targetSv = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(target.ContentLink, true, "sv");

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: c => c.TargetReference = target.ContentLink);

            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentGuid}?expand=*");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { target, page }, async () =>
            {
                using (new OptionsScope(o => o.SetExpandedBehavior(expandedLanguageBehaviour)))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var expanded = content.SelectToken("targetReference.expanded");
                    var expectedValue = expandedLanguageBehaviour == ExpandedLanguageBehavior.RequestedLanguage ? "sv" : "en";

                    Assert.NotNull(expanded);
                    Assert.Equal(expectedValue, expanded.SelectToken("language.name")?.Value<string>());
                }
            });
        }

        [Theory]
        [InlineData(ExpandedLanguageBehavior.ContentLanguage)]
        [InlineData(ExpandedLanguageBehavior.RequestedLanguage)]
        public async Task Get_ContentReferenceList_WhenFallbackApplied_WithExpandedProperty_ShouldReturnContent_UsingExpandedBehaviour(ExpandedLanguageBehavior expandedLanguageBehaviour)
        {
            var firstContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "first content";
            });
            var firstContentSv = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(firstContent.ContentLink, true, "sv");

            var secondContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "second content";
            });
            var secondContentSv = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(secondContent.ContentLink, true, "sv");

            var thirdContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "third content";
            });
            var thirdContentSv = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(thirdContent.ContentLink, true, "sv");

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
                p.ContentReferenceList = new List<ContentReference>() { firstContent.ContentLink, secondContent.ContentLink, thirdContent.ContentLink };
            });

            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentGuid}?expand=*");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContentItems(new[] { firstContent, secondContent, thirdContent, page }, async () =>
            {
                using (new OptionsScope(x =>x.SetExpandedBehavior(expandedLanguageBehaviour)))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var contentReferenceList = (JArray)content["contentReferenceList"];
                    var expectedValue = expandedLanguageBehaviour == ExpandedLanguageBehavior.RequestedLanguage ? "sv" : "en";

                    Assert.All(contentReferenceList.ToList(), p => Assert.Equal(expectedValue, p.SelectToken("expanded.language.name")?.Value<string>()));
                }
            });
        }

        [Theory]
        [InlineData(ExpandedLanguageBehavior.ContentLanguage)]
        [InlineData(ExpandedLanguageBehavior.RequestedLanguage)]
        public async Task Get_Links_WhenFallbackApplied_WithExpandedProperty_ShouldReturnContent_UsingExpandedBehaviour(ExpandedLanguageBehavior expandedLanguageBehaviour)
        {
            var firstLinkContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "first link content";
            });
            var firstLinkContentSv = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(firstLinkContent.ContentLink, true, "sv");

            var secondLinkContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "second link content";
            });
            var secondLinkContentSv = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(secondLinkContent.ContentLink, true, "sv");

            var thirdLinkContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "third link content";
            });
            var thirdLinkContentSv = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(thirdLinkContent.ContentLink, true, "sv");

            var links = new LinkItemCollection
            {
                new LinkItem { Href = UrlResolver.Current.GetUrl(firstLinkContent), Text = "First Link" },
                new LinkItem { Href = UrlResolver.Current.GetUrl(secondLinkContent), Text = "Second Link" },
                new LinkItem { Href = UrlResolver.Current.GetUrl(thirdLinkContent), Text = "Third Link" }
            };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = "My Page";
                p.Links = links;
            });

            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{page.ContentGuid}?expand=*");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(x => x.SetExpandedBehavior(expandedLanguageBehaviour)))
                {
                    var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var links = (JArray)content["links"];
                    var expectedValue = expandedLanguageBehaviour == ExpandedLanguageBehavior.RequestedLanguage ? "sv" : "en";

                    Assert.All(links.ToList(), p => Assert.Equal(expectedValue, p.SelectToken("contentLink.expanded.language.name")?.Value<string>()));
                }
            });
        }

        [Fact]
        public async Task Get_WhenHavingDuplicatedContent_InLinkItemCollection_AndFlattenPropertyModelIsTrue_ShouldReturnAllTheContentExpanded()
        {
            var linkedPage1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var linkedPage2 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var linkItem1 = new LinkItem { Href = UrlResolver.Current.GetUrl(linkedPage1), Text = "Link to standard page" };
            var linkItem2 = new LinkItem { Href = UrlResolver.Current.GetUrl(linkedPage2), Text = "Link to standard page" };

            var links = new LinkItemCollection { linkItem1, linkItem1, linkItem2 };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Links = links;
            });

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand=*");
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(3, content["links"].Count());
                Assert.Equal(content["links"][0], content["links"][1]);

            }, false);
        }

        [Fact]
        public async Task Get_WhenHavingDuplicatedContent_InMainContentArea_AndFlattenPropertyModelIsTrue_ShouldReturnAllTheContentExpanded()
        {
            var linkedPage1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var linkedPage2 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var contentAreaItem1 = new ContentAreaItem { ContentLink = linkedPage1.ContentLink, ContentGuid = linkedPage1.ContentGuid };
            var contentAreaItem2 = new ContentAreaItem { ContentLink = linkedPage2.ContentLink, ContentGuid = linkedPage2.ContentGuid };

            var contentArea = new ContentArea() { Items = { contentAreaItem1, contentAreaItem1, contentAreaItem2 } };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.MainContentArea = contentArea;
            });

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand=*");
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(3, content["mainContentArea"].Count());
                Assert.Equal(content["mainContentArea"][0], content["mainContentArea"][1]);

            }, false);
        }

        [Fact]
        public async Task Get_WhenHavingDuplicatedContent_InContentReferenceList_AndFlattenPropertyModelIsTrue_ShouldReturnAllTheContentExpanded()
        {
            var linkedPage1 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var linkedPage2 = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var contentReferenceList = new List<ContentReference>() { linkedPage1.ContentLink, linkedPage1.ContentLink, linkedPage2.ContentLink };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.ContentReferenceList = contentReferenceList;
            });

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand=*");
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(3, content["contentReferenceList"].Count());
                Assert.Equal(content["contentReferenceList"][0], content["contentReferenceList"][1]);

            }, false);
        }
    }
}
