using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.IntegrationTests;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using FluentAssertions.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi : IAsyncLifetime
    {
        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "en" },
                status = "Published"
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("The Name field is required", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentTypeIsNull_ShouldReturnValidationError()
        {
            var contentApiCreateModel = new
            {
                name = "Page",
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "en" },
                status = "Published"
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains("Content Type is required.", await response.Content.ReadAsStringAsync());
                Assert.Contains("Property 'ContentType' should be an array of strings.", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenNameIsMoreThan255Characters_ShouldReturnBadRequest()
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
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                var error = await response.Content.ReadAsStringAsync();
                Assert.Contains("'Name' has exceeded its limit of 255 characters.", error);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenParentIsEmpty_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Page",
                contentType = new string[] { nameof(StandardPage) },
                language = new { name = "en" },
                status = "Published"
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("The ParentLink field is required", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentIsNotIVersionable_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Folder",
                contentType = new string[] { "SysContentFolder" },
                parentLink = new { id = ContentReference.GlobalBlockFolder.ID }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedFolder.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("Cannot create new version of non-versionable content", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentGuidNotAlreadyExists_ShouldReturnNotFound()
        {
            var emptyGuid = Guid.Empty;

            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "en" },
                status = "Published",
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{emptyGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.NotFound(response);
                Assert.Contains($"The content with id ({emptyGuid}) does not exist.", await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [MemberData(nameof(InvalidLanguageModel))]
        public async Task CreateVersionAsync_ByContentGuid_WhenLanguageIsInvalid_ShouldReturnValidationError(LanguageModel languageModel, string expectedError)
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = languageModel
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenLanguageIsNullAndContentTypeIsLocalizable_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("Language should not be null when content type is localizable", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_ContentIsNotLocalizableAndProvidedLanguage_ShouldReturnBadRequest()
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278313");

            var contentVersion = new
            {
                name = "New Content",
                contentLink = new { guivalue = contentGuid.ToString() },
                contentType = new string[] { "GenericFile" },
                parentLink = new { guivalue = ContentReference.GlobalBlockFolder.ID },
                status = "Published",
                language = new { name = "sv" }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedFile.ContentGuid}", new JsonContent(contentVersion));
                AssertResponse.BadRequest(response);
                Assert.Contains("Language cannot be set when content type is not localizable", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentIsVersionableAndStatusMissing_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
                Assert.Contains("Missing status value on IVersionable content", await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [MemberData(nameof(InvalidStatus))]
        public async Task CreateVersionAsync_ByContentGuid_WhenStatusIsInvalid_ShouldReturnValidationError(object versionStatus, string expectedError)
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = versionStatus
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenStatusIsDelayedPublishAndStartPublishMissing_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "DelayedPublish",
                language = new { name = "en" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains("StartPublish must be set when content item is set for scheduled publishing", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenMissingRequestBody_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", null);
            AssertResponse.BadRequest(response);
            Assert.Contains("Request body is required", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenAwaitingApprovalAndApprovalSequenceNotExist_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentLink = new { guidValue = _securedContent.ContentGuid },
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "en" },
                status = "AwaitingApproval",
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("Action RequestApproval requires that an approval definition is defined", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenAwaitingApprovalAndApprovalSequenceExists_ShouldCreateContentVersion()
        {
            // set approval for parent link
            CreateApprovalSequence(_securedContent.ContentLink.ID);
            var approvalDefinitionRepo = ServiceLocator.Current.GetInstance<IApprovalDefinitionRepository>();
            var definition = await approvalDefinitionRepo.GetAsync(_securedContent.ContentLink);

            var contentApiCreateModel = new
            {
                name = "New Version Content",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "sv" },
                status = "AwaitingApproval",
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                var contentIsSaved = _fixture.ContentRepository.TryGet<StandardPage>(_securedContent.ContentGuid, CultureInfo.GetCultureInfo("sv"), out var savedContent); ;

                AssertResponse.Created(response);
                Assert.True(contentIsSaved);
                Assert.NotNull(savedContent);
                Assert.Equal(contentApiCreateModel.name, savedContent.Name);
                Assert.Equal(VersionStatus.AwaitingApproval, savedContent.Status);
                Assert.Equal(contentApiCreateModel.parentLink.id, savedContent.ParentLink.ID);
                Assert.Equal(contentApiCreateModel.language.name, (savedContent as ILocale).Language.Name);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentGuidIsNotMatchWithURL_ShouldReturnBadRequest()
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278313");

            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentLink = new { guidValue = Guid.NewGuid().ToString() },
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                status = "checkedOut",
                language = new { name = "en" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
                Assert.Contains($"The guid {contentApiCreateModel.contentLink.guidValue} on the provided content does not match the resource location", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenHasNonBranchSpecificPropertyAndNotPassingMasterLanguage_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "New Content",
                contentType = new string[] { nameof(StandardPageWithNonBranchSpecificProperty) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "sv" },
                status = "Published",
                nonBranchSpecificProperty = new { value = "no branch specific test" }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContentWithNonBranchSpecific.ContentGuid}", new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
                Assert.Contains($"Cannot provide non-branch specific property '{nameof(contentApiCreateModel.nonBranchSpecificProperty)}' when not passing the master language", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenHasNonBranchSpecificNestedBlock_AndNotProvideMasterLanguage_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPageWithNonBranchSpecificNestedBlock) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "sv" },
                status = "Published",
                routeSegment = "alloy-plan",
                nonBranchSpecificNestedBlock = new { title = new { value = "Test nested" } }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContentWithNonBranchSpecificNestedBlock.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("Cannot provide non-branch specific property 'NonBranchSpecificNestedBlock' when not passing the master language", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenModelIsValidAndNewLanguageBranchIsProvided_ShouldCreateContentVersion()
        {
            var contentApiCreateModel = new
            {
                name = "New Content Version",
                contentLink = new { guidValue = _securedContent.ContentGuid },
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "sv" },
                status = "Published",
                heading = new { value = "Heading" },
                links = new { value = new[] { new { href = "https://episerver.com" } } },
                mainContentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var contentVersionIsExisting = _fixture.ContentRepository.TryGet<IContent>(_securedContent.ContentGuid, CultureInfo.GetCultureInfo("sv"), out var existingContent);
                Assert.False(contentVersionIsExisting);

                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                var contentIsSaved = _fixture.ContentRepository.TryGet<StandardPage>(_securedContent.ContentGuid, CultureInfo.GetCultureInfo("sv"), out var savedContent);

                AssertResponse.Created(response);
                Assert.True(contentIsSaved);
                Assert.NotNull(savedContent);
                Assert.Equal(contentApiCreateModel.name, savedContent.Name);
                Assert.Equal(_fixture.ContentTypeRepository.Load(nameof(StandardPage)).ID, savedContent.ContentTypeID);
                Assert.Equal(contentApiCreateModel.name, savedContent.Name);
                Assert.Equal(contentApiCreateModel.language.name, savedContent.Language.Name);
                Assert.Equal(contentApiCreateModel.heading.value, savedContent.Heading);
                Assert.Equal(contentApiCreateModel.links.value.First().href, savedContent.Links[0].Href);
                Assert.Equal(contentApiCreateModel.mainContentArea.value.First().contentLink.id, savedContent.MainContentArea.Items.First().ContentLink.ID);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
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

                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentIsExposedToApi_ShouldReturnCreateContent()
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
                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                    AssertResponse.Created(response);
                }
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentIsValid_ShouldReturnLocationHeader()
        {
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
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
                var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                Assert.True(response.Headers.Contains("Location"));

                var id = AssertResponse.Created("api/episerver/v2.0/contentmanagement/", response);

                Assert.Equal(new Guid(jObject["contentLink"]["guidValue"].ToString()), id);
            }
        }

        [Fact]
        public async Task CreateVersionAsync_ByContentGuid_WhenContentIsValid_ShouldCreateCommonDraft()
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
            var response1 = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));

            var response2 = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiCreateModel));
            var jObject = JObject.Parse(await response2.Content.ReadAsStringAsync());

            var commonDraft = _contentVersionRepository.LoadCommonDraft(_securedContent.ContentLink, "en");
            Assert.Equal(commonDraft.ContentLink.WorkID.ToString(), jObject["contentLink"]["workId"].ToString());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentGuid_WhenPropertyHasLeftOut_ShouldCreateVersionWithEmptyProperty()
        {
            var category1 = new Category() { Name = "Category 1", Parent = _rootCategory };
            await _fixture.WithCategories(new[] { category1 }, async () =>
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
                    category = new { value = new[] { new { id = category1.ID } } }
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

                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentGuid}", new JsonContent(versionContentCreated));

                var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                Assert.True(response.Headers.Contains("Location"));
                AssertResponse.Created(response);
                Assert.Equal(response.Headers.Location.AbsolutePath, $"/api/episerver/v2.0/contentmanagement/{contentGuid}");
                jObject["appSettings"]["value"].Should().HaveValue(string.Empty);
                jObject["url"]["value"].Should().HaveValue(null);
                jObject["nestedBlock"]["title"]["value"].Should().HaveValue(string.Empty);
                jObject["xhtmlString"]["value"].Should().HaveValue(null);
                jObject["cultureSpecific"]["value"].Should().HaveValue(string.Empty);
                jObject["category"]["value"].Should().BeEquivalentTo(new JArray());

                CleanupContent(contentGuid);
            });            
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_WhenValidationModeIsInvalid_ShouldThrowBadRequest()
        {
            var createdContent = await CreateContent();

            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix + $"{createdContent.GuidValue}")
            {
                Content = new JsonContent(GetContentVersionInputModel())
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "invalid");

            var response = await _fixture.Client.SendAsync(request);
            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("The value 'invalid' is not valid for ContentValidationMode.", error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_WhenValidationModeIsMissing_ShouldRunAllValidation()
        {
            var createdContent = await CreateContent();
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{createdContent.GuidValue}", new JsonContent(GetContentVersionInputModel()));
            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Always Fail!", error.Error.Message);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_WhenValidationModeIsComplete_ShouldRunAllValidation()
        {
            var createdContent = await CreateContent();

            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix + $"{createdContent.GuidValue}")
            {
                Content = new JsonContent(GetContentVersionInputModel())
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "complete");

            var response = await _fixture.Client.SendAsync(request);
            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Always Fail!", error.Error.Message);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_WhenValidationModeIsMinimal_ShouldSkipValidation()
        {
            var createdContent = await CreateContent();

            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix + $"{createdContent.GuidValue}")
            {
                Content = new JsonContent(GetContentVersionInputModel())
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "minimal");

            var response = await _fixture.Client.SendAsync(request);
            AssertResponse.Created(response);
        }

        [Theory]
        [InlineData("UnknownContentType")]
        [InlineData(nameof(StartPage))]
        [InlineData(nameof(AllPropertyPage))]
        [InlineData(nameof(LocalBlockPage))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentGuid_WhenPassAnyContentType_ShouldCreateContentVersionByCurrentContentType(string contentType)
        {
            var contentApiCreateModel = new
            {
                name = "Content Test",
                language = new { name = "en" },
                contentType = new string[] { contentType },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "CheckedOut"
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentGuid}", new JsonContent(contentApiCreateModel));

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
        public async Task CreateVersionAsync_ByContentGuid_WhenExistingCommonDraft_ShouldCreateVersionFromTheCommonDraft()
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

            //create new common draft version
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentGuid}", new JsonContent(versionContentCreated));
            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            var commonDraftResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + $"{contentGuid}");
            var commonDraftObject = JObject.Parse(await commonDraftResponse.Content.ReadAsStringAsync());

            var commonDraft = _contentVersionRepository.LoadCommonDraft(new ContentReference(Convert.ToInt32(publishedContentObject["contentLink"]["id"])), "en");
            var commonDraftContent = (_contentRepository.Get<IContent>(commonDraft.ContentLink) as AllPropertyPageWithValidationAttribute).CreateWritableClone();

            var publishedContent = _contentRepository.Get<IContent>(contentGuid) as AllPropertyPageWithValidationAttribute;

            //update a property to distinguish the published version from the common draft version
            commonDraftContent.VisibleInMenu = false;

            var updatedCommonDraftContentReference = _contentRepository.Save(commonDraftContent);
            var updatedCommonDraft = _contentRepository.Get<IContent>(updatedCommonDraftContentReference) as AllPropertyPageWithValidationAttribute;

            //create new common draft version
            response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentGuid}", new JsonContent(versionContentCreated));
            var newVersionObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var newCommonDraft = _contentVersionRepository.LoadCommonDraft(new ContentReference(Convert.ToInt32(newVersionObject["contentLink"]["id"])), "en");
            var newCommonDraftContent = (_contentRepository.Get<IContent>(newCommonDraft.ContentLink) as AllPropertyPageWithValidationAttribute).CreateWritableClone();

            Assert.True(publishedContent.VisibleInMenu);
            Assert.False(updatedCommonDraft.VisibleInMenu);
            Assert.Equal(commonDraftContent.VisibleInMenu, newCommonDraftContent.VisibleInMenu);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateVersionAsync_ByContentGuid_WhenPageTypeNullOrEmpty_ShouldCreateVersion()
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

            response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentGuid}", new JsonContent(pageTypeNullVersionContentCreated));

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

            response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentGuid}", new JsonContent(pageTypeNullVersionContentCreated));

            AssertResponse.Created(response);

            var newVersionObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            newVersionObject["pageType"]["value"].Should().HaveValue(null);
                        
            CleanupContent(contentGuid);           
        }

        private async Task<ContentModelReference> CreateContent()
        {
            var createContentModel = GetContentApiCreateModel();

            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix)
            {
                Content = new JsonContent(createContentModel)
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "minimal");
            var response = await _fixture.Client.SendAsync(request);
            var createdContent = JsonConvert.DeserializeObject<ContentApiModel>(await response.Content.ReadAsStringAsync());

            return createdContent.ContentLink;
        }

        private object GetContentVersionInputModel() =>
            new
            {
                name = "Test Content",
                contentType = new[] { nameof(PageWithCustomValidator) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "CheckedOut",
                language = new { name = "en" }
            };
    }
}
