using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi
    {
        [Theory]
        [InlineData("UnknownContentType")]
        [InlineData(nameof(StartPage))]
        [InlineData(nameof(AllPropertyPage))]
        [InlineData(nameof(LocalBlockPage))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenPassAnyContentType_ShouldCreateContentVersionByCurrentContentType(string contentType)
        {
            var contentApiCreateModel = new
            {
                name = "Content Test",
                language = new { name = "en" },
                contentType = new string[] { contentType },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "CheckedOut"
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));

            var contentModelReference = JObject.Parse(await response.Content.ReadAsStringAsync())["contentLink"];
            var savedContentLink = new ContentReference(int.Parse(contentModelReference["id"].ToString()), int.Parse(contentModelReference["workId"].ToString()));
            var savedContent = _fixture.ContentRepository.Get<StandardPage>(savedContentLink);

            AssertResponse.Created(response);
            Assert.NotNull(savedContent);
            Assert.Equal(contentApiCreateModel.name, savedContent.Name);
            Assert.Equal(contentApiCreateModel.language.name, savedContent.Language.Name);
            Assert.Equal(_fixture.ContentTypeRepository.Load(nameof(StandardPage)).ID, savedContent.ContentTypeID);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenContentTypeIsNull_ShouldReturnValidationError()
        {
            var contentApiCreateModel = new { name = "Test Content", parentLink = new { id = _securedContent.ContentLink.ID } };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            AssertResponse.ValidationError(response);
            Assert.Contains("Content Type is required.", await response.Content.ReadAsStringAsync());
            Assert.Contains("Property 'ContentType' should be an array of strings.", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(InvalidLanguageModel))]
        public async Task CreateVersionAsync_ByContentLink_WhenLanguageIsInvalid_ShouldReturnValidationError(LanguageModel languageModel, string expectedError)
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = languageModel,
                requiredProperty = new { value = "Test" },
            };

            using var userContext = new UserScope("authenticated", AuthorizedRole);

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            AssertResponse.ValidationError(response);
            Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(InvalidStatus))]
        public async Task CreateVersionAsync_ByContentLink_WhenStatusIsInvalid_ShouldReturnValidationError(object versionStatus, string expectedError)
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = versionStatus,
                requiredProperty = new { value = "Test" }
            };
            using var userContext = new UserScope("authenticated", AuthorizedRole);

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            AssertResponse.ValidationError(response);
            Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenLanguageIsNullAndContentTypeIsLocalizable_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            AssertResponse.BadRequest(response);
            Assert.Contains("Language should not be null when content type is localizable", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenNameIsMoreThan255Characters_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = new string('A', 256),
                contentType = new string[] { nameof(StandardPage) },
                language = new { name = "en" },
                status = "Published"
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                var error = await response.Content.ReadAsStringAsync();
                Assert.Contains("'Name' has exceeded its limit of 255 characters.", error);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenModelIsValid_ShouldCreateContentVersion()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "sv" },
                status = "Published",
                heading = new { value = "Heading" },
                links = new { value = new[] { new { href = "https://episerver.com" } } },
                mainContentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                }
            };
            using var userContext = new UserScope("authenticated", AuthorizedRole);

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));

            var contentModelReference = JObject.Parse(await response.Content.ReadAsStringAsync())["contentLink"];
            var savedContentLink = new ContentReference(int.Parse(contentModelReference["id"].ToString()), int.Parse(contentModelReference["workId"].ToString()));
            var savedContent = _fixture.ContentRepository.Get<StandardPage>(savedContentLink);

            AssertResponse.Created(response);
            Assert.NotNull(savedContent);
            Assert.Equal(contentApiCreateModel.name, savedContent.Name);
            Assert.Equal(_fixture.ContentTypeRepository.Load(nameof(StandardPage)).ID, savedContent.ContentTypeID);
            Assert.Equal(contentApiCreateModel.language.name, ((ILocale)savedContent).Language.Name);
            Assert.Equal(contentApiCreateModel.heading.value, savedContent.Heading);
            Assert.Equal(contentApiCreateModel.links.value.First().href, savedContent.Links[0].Href);
            Assert.Equal(contentApiCreateModel.mainContentArea.value.First().contentLink.id, savedContent.MainContentArea.Items.First().ContentLink.ID);
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenMissingRequestBody_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", null);
            AssertResponse.BadRequest(response);
            Assert.Contains("Request body is required.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenInvalidValueInCategory_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
                status = "Published",
                routeSegment = "alloy-plan",
                category = "test"
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));

            AssertResponse.BadRequest(response);
            Assert.Contains("Invalid category", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenNestedBlockHasNonBranchSpecificProperty_AndNotProvideMasterLanguage_ShouldReturnBadRequest()
        {
            var newContentVersion = new
            {
                name = "New Content Version",
                contentType = new[] { nameof(StandardPageWithNonBranchSpecificNestedBlock) },
                language = new { name = "sv" },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "Published",
                routeSegment = "alloy-plan",
                nonBranchSpecificNestedBlock = new { title = new { value = "Test nested" } }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContentWithNonBranchSpecificNestedBlock.ContentLink.ID}", new JsonContent(newContentVersion));
            AssertResponse.BadRequest(response);
            Assert.Contains("Cannot provide non-branch specific property 'NonBranchSpecificNestedBlock' when not passing the master language", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenProvidedContentReferenceNotMatchResourceLocation_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { Id = 123 },
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                requiredProperty = new { value = "Test" }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            AssertResponse.BadRequest(response);
            Assert.Contains($"The content reference {contentApiCreateModel.contentLink.Id} on the provided content does not match the resource location.", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenProvidedContentReferenceIsIncorrectFormat_ShouldReturnBadRequest()
        {
            var incorrectContentLinkFormat = "incorrectFormat";
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { Id = 123 },
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                requiredProperty = new { value = "Test" }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{incorrectContentLinkFormat}", new JsonContent(contentApiCreateModel));
            AssertResponse.BadRequest(response);
            Assert.Contains($"'{incorrectContentLinkFormat}' is incorrect format.", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentApiCreateModel = new
                {
                    name = "Test Content",
                    contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    status = "checkedOut",
                    language = new { name = "en" }
                };

                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenContentIsExposedToApi_ShouldReturnCreateContent()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentApiCreateModel = new
                {
                    name = "Test Content",
                    contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    status = "checkedOut",
                    language = new { name = "en" }
                };

                using (var userContext = new UserScope("authenticated", AuthorizedRole))
                {
                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
                    AssertResponse.Created(response);
                }
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenContentIsValid_ShouldReturnLocationHeader()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "checkedOut",
                language = new { name = "en" }
            };
            using var userContext = new UserScope("authenticated", AuthorizedRole);

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.True(response.Headers.Contains("Location"));
            AssertResponse.Created(response);
            Assert.Equal(response.Headers.Location.AbsolutePath, $"/api/episerver/v2.0/contentmanagement/{_securedContent.ContentLink.ID}");
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenPropertyHasLeftOut_ShouldCreateVersionWithEmptyProperty()
        {
            var contentGuid = Guid.NewGuid();
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid },
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "published",
                language = new { name = "en" },
                routeSegment = "alloy-plan",
                requiredProperty = new { value = "Test" },
                nestedBlock = new { title = new { value = "Test nested" } },
                xhtmlString = new { value = "<p>Test</p>\n" },
                pageType = new { value = "PropertyPage" },
                contentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
                contentReference = new { value = new { id = _securedContent.ContentLink.ID } },
                links = new { value = new[] { new { href = "https://episerver.com" } } },
                appSettings = new { value = "app settings" },
                url = new { value = "http://test.com" },
                cultureSpecific = new { value = "dump culture" },
            };

            var createContentResponse = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
            var contentId = JObject.Parse(await createContentResponse.Content.ReadAsStringAsync())["contentLink"]["id"];

            var versionContentCreated = new
            {
                name = "new version",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "checkedOut",
                requiredProperty = new { value = "Test required property" },
                language = new { name = "en" }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentId}", new JsonContent(versionContentCreated));

            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.True(response.Headers.Contains("Location"));
            AssertResponse.Created(response);
            Assert.Equal(response.Headers.Location.AbsolutePath, $"/api/episerver/v2.0/contentmanagement/{contentId}");
            jObject["appSettings"]["value"].Should().HaveValue(string.Empty);
            jObject["url"]["value"].Should().HaveValue(null);
            jObject["nestedBlock"]["title"]["value"].Should().HaveValue(string.Empty);
            jObject["xhtmlString"]["value"].Should().HaveValue(null);
            jObject["cultureSpecific"]["value"].Should().HaveValue(string.Empty);
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenContentIsValid_ShouldCreateCommonDraft()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "CheckedOut",
                language = new { name = "en" }
            };
            using var userContext = new UserScope("authenticated", AuthorizedRole);

            // Create 2 consecutive content version with CheckedOut status to make sure that the API always create common draft
            var response1 = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));

            var response2 = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            var jObject = JObject.Parse(await response2.Content.ReadAsStringAsync());

            var commonDraft = _contentVersionRepository.LoadCommonDraft(_securedContent.ContentLink, "en");
            Assert.Equal(commonDraft.ContentLink.WorkID.ToString(), jObject["contentLink"]["workId"].ToString());
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentLink_WhenProvideContentReferenceWithWorkID_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "CheckedOut",
                language = new { name = "en" }
            };
            using var userContext = new UserScope("authenticated", AuthorizedRole);

            var response1 = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}", new JsonContent(contentApiCreateModel));
            var jObject = JObject.Parse(await response1.Content.ReadAsStringAsync());

            var response2 = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentLink.ID}_{jObject["contentLink"]["workId"]}", new JsonContent(contentApiCreateModel));

            AssertResponse.BadRequest(response2);
            Assert.Contains($"Provide a ContentReference with a WorkID as {_securedContent.ContentLink.ID}_{jObject["contentLink"]["workId"]} is invalid.", await response2.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenValidationModeIsMinimal_ShouldSkipValidation()
        {           
            var createdContent = await CreateContent();

            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix + $"{createdContent.Id}")
            {
                Content = new JsonContent(GetContentVersionInputModel())
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "minimal");

            var response = await _fixture.Client.SendAsync(request);
            AssertResponse.Created(response);            
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenExistingCommonDraft_ShouldCreateVersionFromTheCommonDraft()
        {
            //create a published content
            var contentGuid = Guid.NewGuid();
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid },
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "Published",
                language = new { name = "en" },
                routeSegment = "alloy-plan",
                requiredProperty = new { value = "Test" },
                nestedBlock = new { title = new { value = "Test nested" } },
                xhtmlString = new { value = "<p>Test</p>\n" },
                pageType = new { value = "PropertyPage" },
                contentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
                contentReference = new { value = new { id = _securedContent.ContentLink.ID } },
                links = new { value = new[] { new { href = "https://episerver.com" } } },
                appSettings = new { value = "app settings" },
                url = new { value = "http://test.com" },
                cultureSpecific = new { value = "dump culture" },
            };

            var publishedContentResponse = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
            var publishedContentObject = JObject.Parse(await publishedContentResponse.Content.ReadAsStringAsync());

            var versionContentCreated = new
            {
                name = "new version",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "checkedOut",
                requiredProperty = new { value = "Test required property" },
                language = new { name = "en" }
            };

            var contentId = publishedContentObject["contentLink"]["id"];

            //create new common draft version
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentId}", new JsonContent(versionContentCreated));
            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            var commonDraftResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + $"{contentId}");
            var commonDraftObject = JObject.Parse(await commonDraftResponse.Content.ReadAsStringAsync());

            var commonDraft = _contentVersionRepository.LoadCommonDraft(new ContentReference(Convert.ToInt32(publishedContentObject["contentLink"]["id"])), "en");
            var commonDraftContent = (_contentRepository.Get<IContent>(commonDraft.ContentLink) as AllPropertyPageWithValidationAttribute).CreateWritableClone();

            var publishedContent = _contentRepository.Get<IContent>(contentGuid) as AllPropertyPageWithValidationAttribute;

            //update a property to distinguish the published version from the common draft version
            commonDraftContent.VisibleInMenu = false;

            var updatedCommonDraftContentReference = _contentRepository.Save(commonDraftContent);
            var updatedCommonDraft = _contentRepository.Get<IContent>(updatedCommonDraftContentReference) as AllPropertyPageWithValidationAttribute;

            //create new common draft version
            response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentId}", new JsonContent(versionContentCreated));
            var newVersionObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var newCommonDraft = _contentVersionRepository.LoadCommonDraft(new ContentReference(Convert.ToInt32(newVersionObject["contentLink"]["id"])), "en");
            var newCommonDraftContent = (_contentRepository.Get<IContent>(newCommonDraft.ContentLink) as AllPropertyPageWithValidationAttribute).CreateWritableClone();

            Assert.True(publishedContent.VisibleInMenu);
            Assert.False(updatedCommonDraft.VisibleInMenu);
            Assert.Equal(commonDraftContent.VisibleInMenu, newCommonDraftContent.VisibleInMenu);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentLink_WhenPageTypeNullOrEmpty_ShouldCreateVersion()
        {
            //create a published content
            var contentGuid = Guid.NewGuid();
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid },
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "Published",
                language = new { name = "en" },
                routeSegment = "alloy-plan",
                requiredProperty = new { value = "Test" }
            };

            var publishedContentResponse = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
            var publishedContentObject = JObject.Parse(await publishedContentResponse.Content.ReadAsStringAsync());

            var versionContentCreated = new
            {
                name = "new version",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "checkedOut",
                requiredProperty = new { value = "Test required property" },
                language = new { name = "en" }
            };

            var contentId = publishedContentObject["contentLink"]["id"];

            //create new common draft version
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentId}", new JsonContent(versionContentCreated));
            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            //create new common draft version when the page type is null
            var pageTypeNullVersionContentCreated = new
            {
                name = "new version",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "checkedOut",
                requiredProperty = new { value = "Test required property" },
                language = new { name = "en" },
                pageType = new { value = (PageType)null },
            };

            response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentId}", new JsonContent(pageTypeNullVersionContentCreated));

            AssertResponse.Created(response);

            //create new common draft version when the page type is empty string
            var pageTypeEmptyVersionContentCreated = new
            {
                name = "new version",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "checkedOut",
                requiredProperty = new { value = "Test required property" },
                language = new { name = "en" },
                pageType = new { value = string.Empty },
            };

            response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentId}", new JsonContent(pageTypeNullVersionContentCreated));

            AssertResponse.Created(response);

            var newVersionObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            newVersionObject["pageType"]["value"].Should().HaveValue(null);

            CleanupContent(contentGuid);
        }
    }
}
