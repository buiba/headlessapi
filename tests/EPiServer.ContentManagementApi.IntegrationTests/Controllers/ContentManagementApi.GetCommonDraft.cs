using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi
    {
        private const string FragmentString = "Personalized fragment";

        protected ContentArea CreateContentArea(bool createWithPersonalizeContentItem)
        {
            using var userContext = new UserScope("authenticated", AuthorizedRole);
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var contentArea = new ContentArea();
            contentArea.Items.Add(new ContentAreaItem
            {
                ContentLink = (block as IContent).ContentLink,
                ContentGuid = (block as IContent).ContentGuid,
                AllowedRoles = createWithPersonalizeContentItem ? new string[] { _visitorGroup.Id.ToString() } : null
            });

            return contentArea;
        }

        protected XhtmlString CreatePersonalizedXhtmlString()
        {
            var xhtmlString = new XhtmlString();
            var personalizedContentFactory = ServiceLocator.Current.GetInstance<EPiServer.Personalization.IPersonalizedContentFactory>();
            var securedMarkupGeneratorFactory = ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>();
            var securedMarkupGenerator = securedMarkupGeneratorFactory.CreateSecuredFragmentMarkupGenerator();
            securedMarkupGenerator.RoleSecurityDescriptor.RoleIdentities = new[] { _visitorGroup.Id.ToString() };

            var fragment = new PersonalizedContentFragment(personalizedContentFactory, securedMarkupGenerator)
            {
                Fragments = { new StaticFragment(FragmentString) }
            };

            xhtmlString.Fragments.Add(fragment);

            return xhtmlString;
        }

        [Fact]
        public async Task GetDraft_ByContentGuid_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                await _fixture.WithContentItems(new[] { contentItem }, async () =>
                {
                    var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + contentItem.ContentGuid);
                    AssertResponse.Forbidden(response);
                });
            }
        }

        [Fact]
        public async Task GetDraft_ByContentGuid_WhenContentIsNotExists_ShouldReturnNotFound()
        {                 
            var contentGuid = Guid.NewGuid();
            var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + contentGuid);
            AssertResponse.NotFound(response);
            Assert.Contains($"Content with guid {contentGuid} was not found", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetDraft_ByContentReference_WhenContentIsNotExists_ShouldReturnNotFound()
        {
            var contentLink = 99999;
            var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + contentLink);
            AssertResponse.NotFound(response);
            Assert.Contains($"Content with id {contentLink} was not found", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetDraft_ByContentReference_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                await _fixture.WithContentItems(new[] { contentItem }, async () =>
                {
                    var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + contentItem.ContentLink.ID);
                    AssertResponse.Forbidden(response);
                });
            }
        }

        [Fact]
        public async Task GetDraft_ByContentReference_WhenContentIsExposedToApi_ShouldReturnCommonDraft()
        {
            using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(contentItem.ContentLink, addContentApiWriteRole: true);
                await _fixture.WithContentItems(new[] { contentItem }, async () =>
                {
                    var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + contentItem.ContentLink.ID);
                    AssertResponse.OK(response);
                });
            }
        }

        [Fact]
        public async Task GetDraft_WhenHaveCommonDraft_ShouldReturnCommonDraft()
        {
            var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            await _fixture.WithContentItems(new[] { contentItem }, async () =>
            {
                contentItem.Heading = "New value";
                _fixture.CreateDraftWithFullAccessForEveryone(contentItem);
                var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + contentItem.ContentGuid);
                AssertResponse.OK(response);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal("New value", (string)content["heading"]["value"]);
            });
        }

        [Fact]
        public async Task GetDraft_WhenOnlyHavePublished_ShouldReturnPublished()
        {
            var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: c => c.Heading = "My Heading");
            await _fixture.WithContentItems(new[] { contentItem }, async () =>
            {
                var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + contentItem.ContentGuid);
                AssertResponse.OK(response);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal("My Heading", (string)content["heading"]["value"]);
            });
        }

        [Fact]
        public async Task GetDraft_WhenContainPersonalizedContent_ShouldReturnAll()
        {
            // initialize contentArea that contains content item with personalized setting
            var contentArea = CreateContentArea(true);
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.ContentArea = contentArea);
            await _fixture.WithContentItems(new[] { page }, async () =>
            {
                var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + page.ContentGuid);
                AssertResponse.OK(response);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync());
                Assert.NotNull(content["contentArea"]["value"]);
            });
        }

        [Fact]
        public async Task GetDraft_WhenRequestContainsPersonalizedXhtmlString_ShouldReturnAllPersonalizedContentProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);
            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + page.ContentGuid);
                AssertResponse.OK(response);

                var content = JObject.Parse(await response.Content.ReadAsStringAsync());
                var mainBody = content["mainBody"]["value"];

                Assert.NotNull(mainBody);
                Assert.Contains(FragmentString, mainBody.Value<string>());
            });
        }

        [Fact]
        public async Task GetDraft_WhenFallbackIsApplied_ShouldIgnoreFallbackSetting()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var pageFallbackSetting = new ContentLanguageSetting(page.ContentLink, "sv", null, new string[] { "en" });
            var contentLanguageSettingRepository = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
            contentLanguageSettingRepository.Save(pageFallbackSetting);

            var parentFallbackSetting = new ContentLanguageSetting(ContentReference.StartPage, "sv", null, new string[] { "en" });
            contentLanguageSettingRepository.Save(parentFallbackSetting);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentManagementApiController.RoutePrefix + page.ContentGuid);
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.NotFound(contentResponse);
            });
        }

        [Fact]
        public async Task GetDraft_WhenRequestNonMasterLanguage_ShouldIgnoreNonBranchSpecificProperties()
        {
            var enPage = _fixture.GetWithDefaultName<StandardPageWithNonBranchSpecificProperty>(ContentReference.StartPage, true);
            var svPage = _fixture.CreateLanguageBranchWithDefaultName<StandardPageWithNonBranchSpecificProperty>(enPage.ContentLink, true, "sv");
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentManagementApiController.RoutePrefix + svPage.ContentGuid);
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.WithContent(svPage, async () =>
            {
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var nonBranchSpecificProperty = content.SelectToken("nonBranchSpecificProperty");
                Assert.Null(nonBranchSpecificProperty);
            });
        }

        [Fact]
        public async Task GetDraft_WhenContentHasReferences_ShouldIgnorePermissionFiltering_AndContentExpanded()
        {
            var contentAreaItem1 = new ContentAreaItem { ContentLink = _publicContent.ContentLink, ContentGuid = _publicContent.ContentGuid };
            var contentAreaItem2 = new ContentAreaItem { ContentLink = _securedContent.ContentLink, ContentGuid = _securedContent.ContentGuid };

            var contentArea = new ContentArea() { Items = { contentAreaItem1, contentAreaItem2 } };

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.MainContentArea = contentArea;
            });
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + page.ContentGuid);
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(2, content["mainContentArea"].Count());
                var expanded = content.SelectToken("mainContentArea.expanded");
                Assert.Null(expanded);

            }, false);
        }

        [Fact]
        public async Task GetDraft_WhenMultipleLanguagesContentExist_ShouldReturnAllExistingLanguages()
        {
            var enPage = _fixture.GetWithDefaultName<StandardPageWithNonBranchSpecificProperty>(ContentReference.StartPage, true);
            var svPage = _fixture.CreateLanguageBranchWithDefaultName<StandardPageWithNonBranchSpecificProperty>(enPage.ContentLink, true, "sv");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentManagementApiController.RoutePrefix + svPage.ContentGuid);
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));
            await _fixture.WithContent(svPage, async () =>
            {
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var existingLanguage = content["existingLanguages"];
                Assert.NotNull(existingLanguage);

                var languages = JArray.Parse(existingLanguage.ToString());
                Assert.Equal(2, languages.Children().Count());
            });
        }

        [Fact]
        public async Task GetDraft_WhenLanguageHeaderIsNull_ShouldReturnMasterLanguageContent()
        {
            var enPage = _fixture.GetWithDefaultName<StandardPageWithNonBranchSpecificProperty>(ContentReference.StartPage, true);
            var svPage = _fixture.CreateLanguageBranchWithDefaultName<StandardPageWithNonBranchSpecificProperty>(enPage.ContentLink, true, "sv");
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ContentManagementApiController.RoutePrefix + svPage.ContentGuid);

            await _fixture.WithContent(svPage, async () =>
            {
                var contentResponse = await _fixture.Client.SendAsync(requestMessage);
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var language = content["language"];
                Assert.NotNull(language);

                Assert.Contains("en", language.ToString());
            });
        }

        [Fact]
        public async Task GetDraft_WhenUserHasNoPermission_ShouldReturnForbidden()
        {
            var contentResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid);
            AssertResponse.Forbidden(contentResponse);
        }

        [Fact]
        public async Task GetDraft_WhenParentIsNotPublished_ShouldReturnParentLink()
        {
            var parentPage = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var childPage = _fixture.GetWithDefaultName<StandardPage>(parentPage.ContentLink, true);

            await _fixture.WithContent(childPage, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + childPage.ContentGuid);
                AssertResponse.OK(contentResponse);

                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                var parentLink = content["parentLink"];

                Assert.NotNull(parentLink);
                Assert.Contains(parentPage.ContentGuid.ToString(), parentLink.ToString());
            });
        }

        [Fact]
        public async Task GetDraft_WhenContentIsRootPage_ShouldReturnCommonDraft()
        {
            var response = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + ContentReference.RootPage);
            AssertResponse.OK(response);
        }
    }
}
