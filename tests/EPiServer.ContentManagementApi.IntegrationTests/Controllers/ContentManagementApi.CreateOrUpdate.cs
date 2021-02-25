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
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi
    {
        [Fact]
        public async Task CreateOrUpdateAsync_WhenMissingRequestBody_ShouldReturnBadRequest()
        {
            var response = await CallCreateOrUpdateAsync(_securedContent.ContentGuid, null);

            AssertResponse.BadRequest(response);
            Assert.Contains("Request body is required", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            var contentApiUpsertModel = new { contentType = new string[] { "StartPage" }, status = "Published" };

            var response = await CallCreateOrUpdateAsync(_securedContent.ContentGuid, contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("The Name field is required", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentGuidDoesNotMatchLocation_ShouldReturnBadRequest(string contentGuid)
        {
            var guid = new Guid("4B395558-9BD3-444B-B525-05778414C561");
            var contentApiUpsertModel = new
            {
                name = "New Content",
                parentLink = new { id = _securedContent.ParentLink.ID },
                contentLink = new { guidValue = guid },
                contentType = new string[] { "StartPage" },
                status = "Published"
            };

            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains($"The guid value '{guid}' on the provided content does not match the resource location and cannot be changed.",
                await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenUnknownContentType_AndContentDoesNotExist_ShouldReturnBadRequest()
        {
            var contentApiUpsertModel = new
            {
                name = "Content Test",
                language = new { name = "en" },
                contentType = new string[] { "UnknownContentType" },
                parentLink = new { id = _securedContent.ParentLink.ID },
                status = "CheckedOut"
            };

            var response = await CallCreateOrUpdateAsync(new Guid("9F9F9F0D-7467-4389-ABA6-873604D0C051"), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("Content type 'UnknownContentType' does not exist.", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenLanguageIsNullAndContentTypeIsLocalizable_ShouldReturnBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
            };
            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("Language should not be null when content type is localizable", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentDoesNotExist_AndProvidedParentLinkDoesNotExist_ShouldReturnBadRequest()
        {
            var contentApiUpsertModel = new
            {
                name = "Content1",
                language = new { name = "en" },
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = -1 }
            };
            var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains(ProblemCode.InvalidParent, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(InvalidLanguageModel))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenLanguageIsInvalid_ShouldReturnValidationError(LanguageModel languageModel, string expectedError)
        {
            var contentApiUpsertModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = languageModel
            };

            var response = await CallCreateOrUpdateAsync(_securedContent.ContentGuid, contentApiUpsertModel);
            AssertResponse.ValidationError(response);
            Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(InvalidStatus))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenStatusIsInvalid_ShouldReturnValidationError(object versionStatus, string expectedError)
        {
            var contentApiUpsertModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = versionStatus
            };

            var response = await CallCreateOrUpdateAsync(_securedContent.ContentGuid, contentApiUpsertModel);
            AssertResponse.ValidationError(response);
            Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());

        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentAlreadyExists_AndNotExposedToApi_ShouldReturnForbidden()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentApiUpsertModel = GetContentApiCreateModel();

                var response = await CallCreateOrUpdateAsync(_securedContent.ContentGuid, contentApiUpsertModel);
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentAlreadyExists_AndContentIsExposedToApi_ShouldUpdateContent()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentApiUpsertModel = new
                {
                    name = "Test Content",
                    contentType = new string[] { nameof(StandardPage) },
                    parentLink = new { id = _securedContent.ParentLink.ID },
                    status = "checkedOut",
                    language = new { name = "en" }
                };

                var response = await CallCreateOrUpdateAsync(_securedContent.ContentGuid, contentApiUpsertModel);
                AssertResponse.OK(response);
                var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(contentApiUpsertModel.name, jObject["name"]);
                Assert.Equal(contentApiUpsertModel.language.name, jObject["language"]["name"]);

            }
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentIsVersionable_AndMissingStatus_ShouldReturnBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" }
            };

            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("Missing status value on IVersionable content", await response.Content.ReadAsStringAsync());

        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentDoesNotExists_AndContentIsNotLocalizable_AndProvidedLanguage_ShouldReturnBadRequest()
        {
            var contentApiUpsertModel = new
            {
                name = "New Content",
                contentType = new string[] { "GenericFile" },
                parentLink = new { id = _securedFolder.ParentLink.ID },
                status = "Published",
                language = new { name = "sv" }
            };

            var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("Language cannot be set when content type is not localizable", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentAlreadyExist_AndContentIsNotIVersionable_ShouldUpdate()
        {
            var contentApiUpsertModel = new
            {
                name = "New Folder",
                contentType = new string[] { "SysContentFolder" },
                parentLink = new { id = _securedFolder.ParentLink.ID }
            };

            var response = await CallCreateOrUpdateAsync(_securedFolder.ContentGuid, contentApiUpsertModel);
            AssertResponse.OK(response);

            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(contentApiUpsertModel.name, jObject["name"]);
        }

        [Theory]
        [MemberData(nameof(InvalidVersionable))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentDoesNotExist_AndContentIsNotVersionable_AndProvidedVersionProperties_ShouldReturnBadRerequest(string startPublish, string stopPublish, string status)
        {
            var contentApiUpsertModel = new
            {
                name = "Test folder",
                contentType = new string[] { "SysContentFolder" },
                parentLink = new { id = ContentReference.GlobalBlockFolder.ID },
                status,
                startPublish,
                stopPublish
            };

            var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains(ProblemCode.ContentNotVersionable, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(InvalidVersionable))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentAlreadyExists_AndContentIsNotVersionable_AndProvidedVersionProperties_ShouldReturnBadRerequest(string startPublish, string stopPublish, string status)
        {
            var contentApiUpsertModel = new
            {
                name = "Test folder",
                contentType = new string[] { "SysContentFolder" },
                parentLink = new { id = ContentReference.GlobalBlockFolder.ID },
                status,
                startPublish,
                stopPublish
            };
            var response = await CallCreateOrUpdateAsync(_securedFolder.ContentGuid, contentApiUpsertModel);

            AssertResponse.BadRequest(response);
            Assert.Contains(ProblemCode.ContentNotVersionable, await response.Content.ReadAsStringAsync());
            Assert.Contains("Cannot set (StartPublish or StopPublish or Status) for content that isn't versionable", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentAlreadyExists_AndContentIsNotIRoutable_AndProvidedRouteSegment_ShouldReturnBadRequest()
        {
            var contentGuid = new Guid("0D75B076-A1B0-4C40-B6A5-5D9377EC3C14");
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(ContentReference.GlobalBlockFolder, addContentApiWriteRole: true);

                var content = new
                {
                    name = "Test Content",
                    contentLink = new { guidValue = contentGuid.ToString() },
                    contentType = new string[] { nameof(NestedBlock) },
                    language = new { name = "en" },
                    status = "Published",
                    parentLink = new { id = ContentReference.GlobalBlockFolder.ID },
                };

                var contentApiUpsertModel = new
                {
                    name = "Test Content",
                    contentLink = new { guidValue = contentGuid.ToString() },
                    contentType = new string[] { nameof(NestedBlock) },
                    language = new { name = "en" },
                    parentLink = new { id = ContentReference.GlobalBlockFolder.ID },
                    routeSegment = "content-nested-block-route-segment"
                };

                await CreateContent(content);

                var response = await CallCreateOrUpdateAsync(contentGuid, contentApiUpsertModel);

                AssertResponse.BadRequest(response);
                Assert.Contains(ProblemCode.ContentNotRoutable, await response.Content.ReadAsStringAsync());
                Assert.Contains("Cannot set route segment for the content that isn't IRoutable", await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenInvalidValueInCategoryProperty_ShouldReturnBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                status = "Published",
                routeSegment = "alloy-plan",
                category = "test"
            };

            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("Invalid category", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenPropertyContentReferenceDoestNotExist_ShouldReturnBadRequest()
        {
            var contentGuid = new Guid("43A3E4E5-E67E-4EDB-A09C-B7594B953EC0");

            var contentApiUpsertModel = new
            {
                name = "Update Content",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                status = "CheckedOut",
                targetReference = new { value = new { id = -1 } },
            };

            var response = await CallCreateOrUpdateAsync(contentGuid, contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains(ProblemCode.PropertyReferenceNotFound, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenModelIsValidAndContentApprovalNotExists_ShouldReturnBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
                status = "AwaitingApproval",
                routeSegment = "alloy-plan"
            };

            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("Action RequestApproval requires that an approval definition is defined", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentTypeIsEmpty_ShouldReturnValidationError(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "Page",
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "en" },
                status = "Published"
            };

            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.ValidationError(response);
            Assert.Contains("Content Type is required", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenParentIsEmpty_ShouldReturnBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "Page",
                contentType = new string[] { nameof(StandardPage) },
                language = new { name = "en" },
                status = "Published"
            };

            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("The ParentLink field is required", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenDisplayOptionDoesNotExist_ShouldReturnBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "Page",
                parentLink = new { id = _securedContent.ContentLink.ID },
                contentType = new string[] { nameof(StandardPage) },
                language = new { name = "en" },
                mainContentArea = new
                {
                    value = new[] { new { displayOption = "test", contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
                status = "Published"
            };

            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.BadRequest(response);
            Assert.Contains("The provided display option with the id 'test' does not exist", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenStatusIsDelayedPublishAndStartPublishMissing_ShouldReturnBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = new
            {
                name = "New Content Version",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "DelayedPublish",
                language = new { name = "en" }
            };
            var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
            AssertResponse.ValidationError(response);
            Assert.Contains("StartPublish must be set when content item is set for scheduled publishing", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(ContentGuidData))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenAwaitingApprovalAndApprovalSequenceNotExist_ShouldReturnBadRequest(string contentGuid)
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);
                SetupSecurityDescriptor(_securedContentWithSpecifiedGuid.ContentLink, addContentApiWriteRole: true);

                var contentApiUpsertModel = new
                {
                    name = "New Content Version",
                    contentType = new string[] { nameof(StandardPage) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    language = new { name = "en" },
                    status = "AwaitingApproval",
                };

                var response = await CallCreateOrUpdateAsync(new Guid(contentGuid), contentApiUpsertModel);
                AssertResponse.BadRequest(response);
                Assert.Contains("Action RequestApproval requires that an approval definition is defined", await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [UserScope("authenticated", AuthorizedRole)]
        [MemberData(nameof(ContentGuidData))]
        public async Task CreateOrUpdateAsync_WhenValidationModeIsInvalid_ShouldThrowBadRequest(string contentGuid)
        {
            var contentApiUpsertModel = GetContentApiCreateModel();
            var request = new HttpRequestMessage(HttpMethod.Put, ContentManagementApiController.RoutePrefix + contentGuid)
            {
                Content = new JsonContent(contentApiUpsertModel)
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "invalid");
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("The value 'invalid' is not valid for ContentValidationMode.", error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenValidationModeIsMissing_ShouldRunAllValidation()
        {
            var createContent = await CreateContent();
            var response = await CallCreateOrUpdateAsync(createContent.GuidValue, GetContentApiCreateModel());

            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Always Fail!", error.Error.Message);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenValidationModeIsComplete_ShouldRunAllValidation()
        {
            var createContent = await CreateContent();

            var request = new HttpRequestMessage(HttpMethod.Put, ContentManagementApiController.RoutePrefix + createContent.GuidValue)
            {
                Content = new JsonContent(GetContentApiCreateModel())
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "complete");

            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Always Fail!", error.Error.Message);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenValidationModeIsMinimal_ShouldSkipValidation()
        {
            var contentApiUpsertModel = GetContentApiCreateModel();
            var request = new HttpRequestMessage(HttpMethod.Put, ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid)
            {
                Content = new JsonContent(contentApiUpsertModel)
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "minimal");

            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentApiUpsertModel = new
                {
                    name = "Test Content",
                    contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    status = "checkedOut",
                    language = new { name = "en" }
                };

                var response = await _fixture.Client.PutAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(contentApiUpsertModel));
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentDoesNotExist_AndParentIsExposedToApi_ShouldReturnCreateContent()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentApiUpsertModel = new
                {
                    name = "Test Content",
                    contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    requiredProperty = new { value = "Test" },
                    status = "checkedOut",
                    language = new { name = "en" }
                };

                var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentApiUpsertModel);
                AssertResponse.Created(response);
            }
        }

        [Theory]
        [MemberData(nameof(ValidStatus))]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentDoesNotExist_AndModelIsValid_ShouldReturnCreateContent(string versionStatus, VersionStatus expectedStatus)
        {
            var contentGuid = new Guid("2C9EBA1B-AF20-43C9-A586-AE3708092E52");
            var languageName = "en";

            var category1 = new Category() { Name = "Category 1", Parent = _rootCategory };
            var category2 = new Category() { Name = "Category 2", Parent = _rootCategory };

            await _fixture.WithCategories(new[] { category1, category2 }, async () =>
            {
                var contentApiUpsertModel = new
                {
                    name = "Test Content",
                    contentLink = new { guidValue = contentGuid.ToString() },
                    contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    language = new { name = languageName },
                    startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                    stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
                    status = versionStatus,
                    routeSegment = "alloy-plan",
                    category = new { value = new[] { new { id = category1.ID }, new { id = category2.ID } } },
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
                };

                var response = await CallCreateOrUpdateAsync(contentGuid, contentApiUpsertModel);
                var contentExisting = _contentManagementRepository.GetCommonDraft(contentGuid, languageName) as AllPropertyPageWithValidationAttribute;

                AssertResponse.Created(response);
                Assert.NotNull(contentExisting);
                Assert.Equal(contentApiUpsertModel.name, contentExisting.Name);
                Assert.Equal(expectedStatus, contentExisting.Status);
                Assert.Equal(contentApiUpsertModel.parentLink.id, contentExisting.ParentLink.ID);
                Assert.Equal(contentApiUpsertModel.language.name, (contentExisting as ILocale).Language.Name);
                Assert.Equal(contentApiUpsertModel.stopPublish.ToLocalTime(), (contentExisting as IVersionable).StopPublish.Value);
                Assert.Equal(contentApiUpsertModel.startPublish.ToLocalTime(), (contentExisting as IVersionable).StartPublish.Value);
                Assert.Equal(contentApiUpsertModel.routeSegment, (contentExisting as IRoutable).RouteSegment.ToString());
                contentApiUpsertModel.category.value.Select(x => x.id).Should().Equals((contentExisting as ICategorizable).Category.ToArray());
                Assert.Equal(contentApiUpsertModel.nestedBlock.title.value, contentExisting.NestedBlock.Title);
                Assert.Equal(contentApiUpsertModel.xhtmlString.value, contentExisting.XhtmlString.ToEditString());
                Assert.Equal(contentApiUpsertModel.pageType.value, contentExisting.PageType.Name);
                Assert.Equal(contentApiUpsertModel.contentArea.value.First().contentLink.id, contentExisting.ContentArea.Items.First().ContentLink.ID);
                Assert.Equal(contentApiUpsertModel.contentReference.value.id, contentExisting.ContentReference.ID);
                Assert.Equal(contentApiUpsertModel.links.value.First().href, contentExisting.Links[0].Href);
                Assert.True(response.Headers.Contains("Location"));
                var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                var id = AssertResponse.Created("api/episerver/v2.0/contentmanagement/", response);
                Assert.Equal(new Guid(jObject["contentLink"]["guidValue"].ToString()), id);
                Assert.NotEqual(0, jObject["contentLink"]["workID"]);
                Assert.Null(contentExisting.String);
                Assert.Null(contentExisting.AppSettings);
                Assert.Null(contentExisting.Url);
            });
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenAwaitingApproval_AndApprovalSequenceExists_ShouldUpdateContent()
        {
            // set approval for content
            CreateApprovalSequence(_securedContent.ContentLink.ID);
            var approvalDefinitionRepo = ServiceLocator.Current.GetInstance<IApprovalDefinitionRepository>();
            var definition = await approvalDefinitionRepo.GetAsync(_securedContent.ContentLink);

            var contentApiUpsertModel = new
            {
                name = "New Version Content",
                contentType = new string[] { nameof(StandardPage) },
                parentLink = new { id = _securedContent.ParentLink.ID },
                language = new { name = "en" },
                status = "AwaitingApproval",
            };

            var response = await CallCreateOrUpdateAsync(_securedContent.ContentGuid, contentApiUpsertModel);

            var savedContent = _contentManagementRepository.GetCommonDraft(_securedContent.ContentGuid, "en");

            AssertResponse.OK(response);
            Assert.NotNull(savedContent);
            Assert.Equal(contentApiUpsertModel.name, savedContent.Name);
            Assert.Equal(VersionStatus.AwaitingApproval, (savedContent as StandardPage).Status);
            Assert.Equal(contentApiUpsertModel.parentLink.id, savedContent.ParentLink.ID);
            Assert.Equal(contentApiUpsertModel.language.name, (savedContent as ILocale).Language.Name);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_ContentAlreadyExists_AndModelIsValid_ShouldUpdateContentAndReturnInTheCamelCase()
        {
            var contentGuid = new Guid("C8A865C4-73C6-46D6-BA7D-66C9C867EC32");
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);
                var displayOption = "narrow";

                var contentApiModel = new
                {
                    name = "Test Content",
                    contentLink = new { guidValue = contentGuid.ToString() },
                    contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    language = new { name = "en" },
                    startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                    stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
                    status = "Published",
                    routeSegment = "alloy-plan",
                    requiredProperty = new { value = "Test" },
                };

                await _fixture.WithDisplayOption("narrow", "Narrow", "span8", async () =>
                {
                    var contentApiUpsertModel = new
                    {
                        name = "Content Updated",
                        contentLink = new { guidValue = contentGuid.ToString() },
                        contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                        parentLink = new { id = _securedContent.ContentLink.ID },
                        language = new { name = "en" },
                        startPublish = DateTimeOffset.Parse("2020-11-20 08:15:42+02:00"),
                        stopPublish = DateTimeOffset.Parse("2020-12-20 09:15:42+02:00"),
                        status = "Published",
                        routeSegment = "alloy-plan",
                        requiredProperty = new { value = "Test" },
                        nestedBlock = new { title = new { value = "Test nested" } },
                        xhtmlString = new { value = "<p>Test</p>\n" },
                        pageType = new { value = "PropertyPage" },
                        contentArea = new
                        {
                            value = new[] { new { displayOption, contentLink = new { id = _securedContent.ContentLink.ID } } },
                        },
                        contentReference = new { value = new { id = _securedContent.ContentLink.ID } },
                        links = new { value = new[] { new { href = "https://episerver.com" } } },
                    };

                    await CreateContent(contentApiModel);

                    var response = await CallCreateOrUpdateAsync(contentGuid, contentApiUpsertModel);

                    AssertResponse.OK(response);

                    var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    jObject.Properties().Any(x => x.Name.Equals("contentLink")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("contentType")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("parentLink")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("language")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("startPublish")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("stopPublish")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("status")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("routeSegment")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("requiredProperty")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("nestedBlock")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("xhtmlString")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("pageType")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("contentArea")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("contentReference")).Should().BeTrue();
                    jObject.Properties().Any(x => x.Name.Equals("links")).Should().BeTrue();
                    jObject["contentLink"]["guidValue"].Should().NotBeNull();
                    jObject["contentArea"]["value"][0]["contentLink"]["id"].Should().NotBeNull();
                    Assert.Equal(displayOption, jObject["contentArea"]["value"][0]["displayOption"]);
                });
            };
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenModelIsValid_AndNewLanguageBranchIsProvided_ShouldCreateContent()
        {
            var contentGuid = new Guid("13879AFF-54CA-4779-B04F-1947F9A4B501");

            var contentApiUpsertModel = new
            {
                name = "New Content Version",
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

            var contentIsExisting = _fixture.ContentRepository.TryGet<IContent>(contentGuid, CultureInfo.GetCultureInfo("sv"), out var existingContent);
            Assert.False(contentIsExisting);

            var response = await _fixture.Client.PutAsync(ContentManagementApiController.RoutePrefix + contentGuid, new JsonContent(contentApiUpsertModel));
            var contentIsSaved = _fixture.ContentRepository.TryGet<StandardPage>(contentGuid, CultureInfo.GetCultureInfo("sv"), out var savedContent);

            AssertResponse.Created(response);
            Assert.True(contentIsSaved);
            Assert.NotNull(savedContent);
            Assert.Equal(contentApiUpsertModel.name, savedContent.Name);
            Assert.Equal(_fixture.ContentTypeRepository.Load(nameof(StandardPage)).ID, savedContent.ContentTypeID);
            Assert.Equal(contentApiUpsertModel.name, savedContent.Name);
            Assert.Equal(contentApiUpsertModel.language.name, savedContent.Language.Name);
            Assert.Equal(contentApiUpsertModel.heading.value, savedContent.Heading);
            Assert.Equal(contentApiUpsertModel.links.value.First().href, savedContent.Links[0].Href);
            Assert.Equal(contentApiUpsertModel.mainContentArea.value.First().contentLink.id, savedContent.MainContentArea.Items.First().ContentLink.ID);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentDoesNotExist_AndContentIsValid_ShouldCreateContentAndReturnLocationHeader()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentApiUpsertModel = new
                {
                    name = "Test Content",
                    contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    status = "checkedOut",
                    requiredProperty = new { value = "Test" },
                    language = new { name = "en" }
                };

                var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentApiUpsertModel);
                var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                Assert.True(response.Headers.Contains("Location"));
                var id = AssertResponse.Created("api/episerver/v2.0/contentmanagement/", response);
                Assert.Equal(new Guid(jObject["contentLink"]["guidValue"].ToString()), id);
            }
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenContentIsValid_ShouldUpdateCommonDraft()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentGuid = new Guid("12FB7D40-4309-4E12-B906-F89D8D464E15");

                var contentApiModel = new
                {
                    name = "Test Content",
                    contentLink = new { guidValue = contentGuid.ToString() },
                    contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    status = "CheckedOut",
                    requiredProperty = new { value = "Test" },
                    language = new { name = "en" }
                };

                var contentApiUpsertModel = new
                {
                    name = "New Content",
                    contentLink = new { guidValue = contentGuid.ToString() },
                    contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    status = "CheckedOut",
                    requiredProperty = new { value = "Updated" },
                    language = new { name = "en" }
                };

                await CreateContent(contentApiUpsertModel);

                // Create 2 consecutive content version with CheckedOut status to make sure that the API always create common draft
                var response1 = await CallCreateOrUpdateAsync(contentGuid, contentApiModel);
                var response2 = await CallCreateOrUpdateAsync(contentGuid, contentApiUpsertModel);

                var jObject = JObject.Parse(await response2.Content.ReadAsStringAsync());
                var commonDraft = _contentManagementRepository.GetCommonDraft(contentGuid, "en") as AllPropertyPageWithValidationAttribute;

                Assert.Equal(commonDraft.ContentLink.WorkID.ToString(), jObject["contentLink"]["workId"].ToString());
                Assert.Equal(commonDraft.Name, jObject["name"]);
                Assert.Equal(commonDraft.RequiredProperty, jObject["requiredProperty"]["value"]);
            }
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateOrUpdateAsync_WhenPropertyHasLeftOut_ShouldUpdateContentWithEmptyProperty()
        {
            var contentGuid = Guid.NewGuid();
            var contentApiModel = new
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
            };

            var createContentResponse = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiModel));
            var contentId = JObject.Parse(await createContentResponse.Content.ReadAsStringAsync())["contentLink"]["id"];

            var contentApiUpsertModel = new
            {
                name = "new version",
                contentType = new[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = "checkedOut",
                requiredProperty = new { value = "Test required property" },
                language = new { name = "en" }
            };

            var response = await CallCreateOrUpdateAsync(contentGuid, contentApiUpsertModel);

            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            AssertResponse.OK(response);
            jObject["appSettings"]["value"].Should().HaveValue(string.Empty);
            jObject["url"]["value"].Should().HaveValue(null);
            jObject["nestedBlock"]["title"]["value"].Should().HaveValue(string.Empty);
            jObject["xhtmlString"]["value"].Should().HaveValue(null);
        }

        private async Task CreateContent(object content)
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(content));
            response.EnsureSuccessStatusCode();
        }

        private Task<HttpResponseMessage> CallCreateOrUpdateAsync(Guid? contentGuid, object content) => _fixture.Client.PutAsync(ContentManagementApiController.RoutePrefix + contentGuid, new JsonContent(content));

        public static TheoryData ContentGuidData => new TheoryData<string>
        {
            { "9F9F9F0D-7467-4389-ABA6-873604D0C051" },
            { ContentGuid }
        };
    }
}
