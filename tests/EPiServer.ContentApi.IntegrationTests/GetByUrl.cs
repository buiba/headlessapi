
using EPiServer.Cms.Shell;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class GetByUrl
    {
        private const string BaseUri = "api/episerver/v2.0/content";
        private readonly ServiceFixture _fixture;

        public GetByUrl(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        private static string EndpointUrl(Uri contentUrl, bool? matchExact = null, string select = null, string expand = null)
            => BaseUri + $"?contentUrl={HttpUtility.UrlEncode(contentUrl.ToString())}" + (matchExact.HasValue ? $"&matchExact={matchExact}" : "") + (select is object ? $"&select={select}" : "") + (expand is object ? $"&expand={expand}" : "");

        private Uri ContentUrl(ContentReference contentLink, ContextMode contextMode = ContextMode.Default, string additionalSegments = null, string language = null) => new Uri(_fixture.UrlResolver.GetUrl(contentLink, language, new VirtualPathArguments { ContextMode = contextMode, ValidateTemplate = false, ForceAbsolute = true }) + additionalSegments);

        [Fact]
        public async Task GetByUrl_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, EndpointUrl(ContentUrl(new ContentReference(1))));

            var contentResponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(contentResponse);
        }

        [Fact]
        public async Task GetByUrl_WhenUrlMatchesContent_ShouldReturnContent()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
          
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink)));
                AssertResponse.OK(response);
                var contents = JArray.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(child.Name, (string)contents[0]["name"]);
                Assert.Equal(ContentUrl(child.ContentLink).ToString(), (string)contents[0]["url"]);
            });
        }

        [Fact]
        public async Task GetByUrl_WhenUrlMatchesContent_ShouldReturnContentMetadataHeaders()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink)));
                Assert.Equal(child.ContentGuid, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.ContentGUIDMetadataHeaderName).Single()));
                Assert.Equal(child.Language.Name, response.Headers.GetValues(MetadataHeaderConstants.BranchMetadataHeaderName).Single());
            });
        }

        [Fact]
        public async Task GetByUrl_WhenUrlMatchesContent_ShouldReturnSiteMetadataHeaders()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage);

            await _fixture.WithContent(page, async () =>
            {
                var site = new SiteDefinition
                {
                    Id = Guid.NewGuid(),
                    SiteUrl = new Uri("http://one.com/"),
                    Name = "Test site",
                    StartPage = page.ContentLink,
                    Hosts = { new HostDefinition { Name = "one.com" } }
                };

                await _fixture.WithSite(site, async () =>
                {
                    var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(page.ContentLink)));

                    Assert.Equal(site.Id, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.SiteIdMetadataHeaderName).Single()));
                });
            });
        }

        [Fact]
        public async Task GetByUrl_WhenContentNotFound_ShouldReturnAvailableContentMetadataHeaders()
        {
            //construct a wrong contenturl, but still contains the right site info to return metadata
            var uri = new Uri("http://one.com?epieditmode=True");
            var response = await _fixture.Client.GetAsync(EndpointUrl(uri));
            AssertResponse.OK(response);
            var contents = JArray.Parse(await response.Content.ReadAsStringAsync());
            Assert.Empty(contents);

            var siteId = Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.SiteIdMetadataHeaderName).First());
            Assert.True(Guid.Equals(SiteDefinition.Current.Id, siteId));

            var startPage = ServiceLocator.Current.GetInstance<IContentRepository>().Get<IContent>(SiteDefinition.Current.StartPage);
            Assert.True(Guid.Equals(startPage.ContentGuid, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.StartPageMetadataHeaderName).First())));

            Assert.Equal("Edit", response.Headers.GetValues(MetadataHeaderConstants.ContextModeMetadataHeaderName).First());
        }

        [Fact]
        public async Task GetByUrl_WhenUrlMatchesContentButUserIsNotAuthenticatedAndContentIsNotForEveryone_ShouldReturnContentMetadataHeaders()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                //set access right restriction
                var contentAccessRepository = ServiceLocator.Current.GetInstance<IContentSecurityRepository>();
                var securityDescriptor = contentAccessRepository.Get(child.ContentLink).CreateWritableClone() as IContentSecurityDescriptor;
                var authorizedRole = "Authorized";
                securityDescriptor.ToLocal(true);
                securityDescriptor.RemoveEntry(securityDescriptor.Entries.Single(e => e.Name.Equals(EveryoneRole.RoleName)));
                securityDescriptor.AddEntry(new AccessControlEntry(authorizedRole, AccessLevel.Read, SecurityEntityType.Role));
                contentAccessRepository.Save(child.ContentLink, securityDescriptor, SecuritySaveType.Replace);

                var currentSiteStartPage = ServiceLocator.Current.GetInstance<IContentRepository>().Get<IContent>(SiteDefinition.Current.StartPage);
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink)));
                AssertResponse.Unauthorized(response);
                Assert.Equal(SiteDefinition.Current.Id, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.SiteIdMetadataHeaderName).Single()));
                Assert.Equal(currentSiteStartPage.ContentGuid, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.StartPageMetadataHeaderName).Single()));
                Assert.Equal(child.LanguageBranch(), response.Headers.GetValues(MetadataHeaderConstants.BranchMetadataHeaderName).Single());
                Assert.False(response.Headers.Contains(MetadataHeaderConstants.ContentGUIDMetadataHeaderName));

                //reset access right restriction
                securityDescriptor.RemoveEntry(securityDescriptor.Entries.Single(e => e.Name.Equals(authorizedRole)));
                securityDescriptor.AddEntry(new AccessControlEntry(EveryoneRole.RoleName, AccessLevel.Read, SecurityEntityType.Role));
                contentAccessRepository.Save(child.ContentLink, securityDescriptor, SecuritySaveType.Replace);
            });
        }

        [Fact]
        public async Task GetByUrl_WhenUrlMatchesContentButUserIsNotAuthorised_ShouldReturnContentMetadataHeaders()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var currentSiteStartPage = ServiceLocator.Current.GetInstance<IContentRepository>().Get<IContent>(SiteDefinition.Current.StartPage);
                using (var userContext = new UserScope("authenticated",  null))
                {
                    //set access right restriction
                    var contentAccessRepository = ServiceLocator.Current.GetInstance<IContentSecurityRepository>();
                    var securityDescriptor = contentAccessRepository.Get(child.ContentLink).CreateWritableClone() as IContentSecurityDescriptor;
                    var authorizedRole = "Authorized";
                    securityDescriptor.ToLocal(true);
                    securityDescriptor.RemoveEntry(securityDescriptor.Entries.Single(e => e.Name.Equals(EveryoneRole.RoleName)));
                    securityDescriptor.AddEntry(new AccessControlEntry(authorizedRole, AccessLevel.Read, SecurityEntityType.Role));
                    contentAccessRepository.Save(child.ContentLink, securityDescriptor, SecuritySaveType.Replace);

                    var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink)));
                    AssertResponse.Forbidden(response);
                    Assert.Equal(SiteDefinition.Current.Id, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.SiteIdMetadataHeaderName).Single()));
                    Assert.Equal(currentSiteStartPage.ContentGuid, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.StartPageMetadataHeaderName).Single()));
                    Assert.Equal(child.LanguageBranch(), response.Headers.GetValues(MetadataHeaderConstants.BranchMetadataHeaderName).Single());
                    Assert.False(response.Headers.Contains(MetadataHeaderConstants.ContentGUIDMetadataHeaderName));

                    //reset access right restriction
                    securityDescriptor.RemoveEntry(securityDescriptor.Entries.Single(e => e.Name.Equals(authorizedRole)));
                    securityDescriptor.AddEntry(new AccessControlEntry(EveryoneRole.RoleName, AccessLevel.Read, SecurityEntityType.Role));
                    contentAccessRepository.Save(child.ContentLink, securityDescriptor, SecuritySaveType.Replace);
                }
            });
        }

        [Fact]
        public async Task GetByUrl_WhenRelativeUrlMatchesContentOnCurrentSite_ShouldReturnSiteIdHeader()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var route = new Uri(ContentUrl(child.ContentLink).AbsolutePath, UriKind.Relative);
                var response = await _fixture.Client.GetAsync(EndpointUrl(route));

                AssertResponse.OK(response);
                Assert.Equal(SiteDefinition.Current.Id, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.SiteIdMetadataHeaderName).Single()));
            });
        }

        [Fact]
        public async Task GetByUrl_WhenRelativeUrlMatchesContentOnCurrentSite_ShouldReturnStartPageHeader()
        {
            var startPage = _fixture.ContentRepository.Get<IContent>(ContentReference.StartPage);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var route = new Uri(ContentUrl(child.ContentLink).AbsolutePath, UriKind.Relative);
                var response = await _fixture.Client.GetAsync(EndpointUrl(route));

                AssertResponse.OK(response);
                Assert.Equal(startPage.ContentGuid, Guid.Parse(response.Headers.GetValues(MetadataHeaderConstants.StartPageMetadataHeaderName).Single()));
            });
        }

        [Fact]
        public async Task GetByUrl_WhenRelativeUrlMatchesContentOnOtherSite_ShouldReturnEmtyList()
        {
            var startPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage);

            await _fixture.WithContent(startPage, async () =>
            {
                var site = new SiteDefinition
                {
                    Id = Guid.NewGuid(),
                    SiteUrl = new Uri("http://one.com/"),
                    Name = "Test site",
                    StartPage = startPage.ContentLink,
                    Hosts = { new HostDefinition { Name = "one.com", Language = CultureInfo.GetCultureInfo("en") } }
                };

                await _fixture.WithSite(site, async () =>
                {
                    var child = _fixture.GetWithDefaultName<StandardPage>(startPage.ContentLink);

                    await _fixture.WithContent(child, async () =>
                    {
                        var route = new Uri(ContentUrl(child.ContentLink).AbsolutePath, UriKind.Relative);
                        var response = await _fixture.Client.GetAsync(EndpointUrl(route));

                        AssertResponse.OK(response);
                        var contents = JArray.Parse(await response.Content.ReadAsStringAsync());
                        Assert.Empty(contents);
                    });
                });
            });
        }

        [Fact]
        public async Task GetByUrl_WhenUrlMatchesContent_ShouldReturnContentAndBranch()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink), matchExact: false));
                AssertResponse.OK(response);
                var contents = JArray.Parse(await response.Content.ReadAsStringAsync());

                Assert.Equal(child.Name, (string)contents[0]["name"]);
                Assert.Equal(ContentUrl(child.ContentLink).ToString(), (string)contents[0]["url"]);
                Assert.Equal("en", response.Headers.GetValues(MetadataHeaderConstants.BranchMetadataHeaderName).Single());
            });
        }

        [Fact]
        public async Task GetByUrl_WhenNotRequiringExactMatchAndUrlIsPartialMatch_ShouldReturnContentAndBranch()
        {
            var actionSegments = "action/withparameter";
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink, additionalSegments: actionSegments), matchExact: false));
                AssertResponse.OK(response);
                var contents = JArray.Parse(await response.Content.ReadAsStringAsync());

                Assert.Equal(child.Name, (string)contents[0]["name"]);
                Assert.Equal(ContentUrl(child.ContentLink).ToString(), (string)contents[0]["url"]);
                Assert.Equal("en", response.Headers.GetValues(MetadataHeaderConstants.BranchMetadataHeaderName).Single());
            });
        }

        [Fact]
        public async Task GetByUrl_WhenNotRequiringExactMatchAndFallBackIsApply_ShouldReturnContentAndBranch()
        {
            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();

            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, language: "en", init: p => {
                p.Name = "parent";
            });

            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true, init: p => {
                p.Name = "child";
            });

            var parentFallbackSetting = new ContentLanguageSetting(child.ContentLink, "sv", "sv", new string[] { "en" }, true);
            contentLanguageSettingRepository.Save(parentFallbackSetting);

            await _fixture.WithContent(child, async () =>
            {
                var response = await _fixture.Client.GetAsync("api/episerver/v2.0/content?contentUrl=http://localhost/sv/parent/child/&matchExact=False");
                AssertResponse.OK(response);
                var contents = JArray.Parse(await response.Content.ReadAsStringAsync());

                Assert.Equal(child.Name, (string)contents[0]["name"]);
                Assert.Equal(ContentUrl(child.ContentLink).ToString(), (string)contents[0]["contentLink"]["url"]);
                Assert.Equal(ContentUrl(child.ContentLink, ContextMode.Default, null, "sv").ToString(), (string)contents[0]["url"]);
                Assert.Equal("sv", response.Headers.GetValues(MetadataHeaderConstants.BranchMetadataHeaderName).Single());
            });
        }

        [Fact]
        public async Task GetByUrl_WhenNotRequiringExactMatchAndUrlIsPartialMatch_ShouldReturnContentAndRemainingRoute()
        {
            var actionSegments = "action/withparameter";
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink, additionalSegments: actionSegments), matchExact: false));
                AssertResponse.OK(response);
                var contents = JArray.Parse(await response.Content.ReadAsStringAsync());

                Assert.Equal(child.Name, (string)contents[0]["name"]);
                Assert.Equal(ContentUrl(child.ContentLink).ToString(), (string)contents[0]["url"]);
                Assert.Equal("/" + actionSegments, response.Headers.GetValues(MetadataHeaderConstants.RemainingRouteMetadataHeaderName).Single());
            });
        }

        [Fact]
        public async Task GetByUrl_WhenRequiringExaxtMatchAndUrlIsPartialMatch_ShouldReturnEmptyList()
        {
            var actionSegments = "action/withparameter";
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(child.ContentLink, additionalSegments: actionSegments), matchExact: true));
                AssertResponse.OK(response);
                var contents = JArray.Parse(await response.Content.ReadAsStringAsync());
                Assert.Empty(contents);
            });
        }

        [Fact]
        public async Task GetByUrl_WhenDraftIsRequested_ShouldReturnNonPublishedDraft()
        {
            var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            await _fixture.WithContentItems(new[] { contentItem }, async () =>
            {
                using (new OptionsScope(o => o.SetEnablePreviewMode(true)))
                {
                    var draftReference = _fixture.CreateDraftWithFullAccessForEveryone(contentItem);
                    var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(draftReference, ContextMode.Preview)));
                    AssertResponse.OK(response);
                }
            });
        }

        [Fact]
        public async Task GetByUrl_WhenPreviewVersionIsRequested_ShouldReturnContextModeAsHeader()
        {
            var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            await _fixture.WithContentItems(new[] { contentItem }, async () =>
            {
                using (new OptionsScope(o => o.SetEnablePreviewMode(true)))
                {
                    var draftReference = _fixture.CreateDraftWithFullAccessForEveryone(contentItem);
                    var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(draftReference, ContextMode.Preview)));
                    AssertResponse.OK(response);

                    Assert.Equal(nameof(ContextMode.Preview), response.Headers.GetValues(MetadataHeaderConstants.ContextModeMetadataHeaderName).Single());
                }
            });
        }

        [Fact]
        public async Task GetByUrl_WhenEditVersionIsRequested_ShouldReturnContextModeAsHeader()
        {
            var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            await _fixture.WithContentItems(new[] { contentItem }, async () =>
            {
                using (new OptionsScope(o => o.SetEnablePreviewMode(true)))
                {
                    var draftReference = _fixture.CreateDraftWithFullAccessForEveryone(contentItem);
                    var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(draftReference, ContextMode.Edit)));
                    AssertResponse.OK(response);

                    Assert.Equal(nameof(ContextMode.Edit), response.Headers.GetValues(MetadataHeaderConstants.ContextModeMetadataHeaderName).Single());
                }
            });
        }

        [Fact]
        public async Task GetByUrl_WhenDraftIsRequestedAndEnablePreviewModeIsFalse_ShouldReturnEmptyList()
        {
            var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            await _fixture.WithContentItems(new[] { contentItem }, async () =>
            {
                using (new OptionsScope(o => o.SetEnablePreviewMode(false)))
                {
                    var draftReference = _fixture.CreateDraftWithFullAccessForEveryone(contentItem);
                    var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(draftReference, ContextMode.Preview)));
                    AssertResponse.OK(response);
                    var contents = JArray.Parse(await response.Content.ReadAsStringAsync());
                    Assert.Empty(contents);
                }
            });
        }

        [Fact]
        public async Task GetByUrl_WhenDraftIsRequested_WithExpandedPropertyAndMissingTranslatedContent_ShouldReturnContentWithMasterlanguageAsFallback()
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
                using (new OptionsScope(true))
                {
                    var draftReference = _fixture.CreateDraftWithFullAccessForEveryone(page);
                    var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(draftReference, ContextMode.Edit), expand: nameof(StandardPage.MainContentArea)));
                    var contents = JArray.Parse(await response.Content.ReadAsStringAsync());

                    var expandedGuids = contents[0]
                        .SelectTokens("mainContentArea..contentLink.expanded.contentLink.guidValue")
                        .Values<string>();

                    Assert.Contains(targetEnglish.ContentGuid.ToString(), expandedGuids);
                    Assert.Contains(targetSwedish.ContentGuid.ToString(), expandedGuids);
                }
            });
        }

        [Fact]
        public async Task GetByUrl_WhenPropertyIsSelected_ShouldIncludeProperty()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Heading = "The heading";
                p.MainBody = new XhtmlString("<p>The Main Body</p>");
            });
            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(EndpointUrl(ContentUrl(page.ContentLink), select: "heading"));
                AssertResponse.OK(response);
                var contents = JArray.Parse(await response.Content.ReadAsStringAsync());
                Assert.Null(contents[0]["mainBody"]);
                Assert.Equal(page.Heading, (string)contents[0]["heading"]);
            });
        }
    }
}
