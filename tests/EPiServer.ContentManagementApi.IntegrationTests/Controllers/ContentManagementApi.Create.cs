using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Newtonsoft.Json.Linq;
using Xunit;
using FluentAssertions;
using System.Net.Http;
using EPiServer.ContentApi.IntegrationTests;
using EPiServer.ContentApi.Error.Internal;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi : IAsyncLifetime
    {
        [Fact]
        public async Task CreateAsync_ShouldAllowOptionsMethod()
        {            
            var request = new HttpRequestMessage(HttpMethod.Options, ContentManagementApiController.RoutePrefix);
            var response = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(response);
        }

        [Fact]
        public async Task CreateAsync_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new { contentType = new string[] { "StartPage" }, status = "Published" };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("The Name field is required", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenUnknownContentType_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Content Test",
                language = new { name = "en" },
                contentType = new string[] { "UnknownContentType" },
                parentLink = new { id = _securedContent.ContentLink.ID },
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("Content type 'UnknownContentType' does not exist.", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenContentTypeIsEmpty_ShouldReturnValidationError()
        {
            var contentApiCreateModel = new { name = "Test Content", parentLink = new { id = _securedContent.ContentLink.ID }, contentType = new string[] { } };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains("Content Type is required.", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenContentTypeIsNull_ShouldReturnValidationError()
        {
            var contentApiCreateModel = new { name = "Test Content", parentLink = new { id = _securedContent.ContentLink.ID } };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains("Content Type is required.", await response.Content.ReadAsStringAsync());
                Assert.Contains("Property 'ContentType' should be an array of strings.", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenParentIsEmpty_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                requiredProperty = new { value = "Test" }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("The ParentLink field is required", await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [MemberData(nameof(InvalidLanguageModel))]
        public async Task CreateAsync_WhenLanguageIsInvalid_ShouldReturnValidationError(LanguageModel languageModel, string expectedError)
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = languageModel,
                requiredProperty = new { value = "Test" },
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenLanguageIsNullAndContentTypeIsLocalizable_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                requiredProperty = new { value = "Test" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.BadRequest(response);
                Assert.Contains("Language should not be null when content type is localizable", await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [MemberData(nameof(InvalidStatus))]
        public async Task CreateAsync_WhenStatusIsInvalid_ShouldReturnValidationError(object versionStatus, string expectedError)
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                status = versionStatus,
                requiredProperty = new { value = "Test" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.ValidationError(response);
                Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenParentLinkDoesNotExits_ShouldReturnBadRequest()
        {
            var content = new
            {
                name = "Content1",
                language = new { name = "en" },
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = -1 },
                requiredProperty = new { value = "Test" }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(content));
                AssertResponse.BadRequest(response);
                Assert.Contains(ProblemCode.InvalidParent, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenParentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentApiCreateModel = new
                {
                    name = "Test Content",
                    contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    status = "checkedOut",
                    language = new { name = "en" }
                };

                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenParentIsExposedToApi_ShouldReturnCreateContent()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentApiCreateModel = new
                {
                    name = "Test Content",
                    contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    requiredProperty = new { value = "Test" },
                    status = "checkedOut",
                    language = new { name = "en" }
                };

                using (var userContext = new UserScope("authenticated", AuthorizedRole))
                {
                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                    AssertResponse.Created(response);
                }
            }
        }

        [Fact]
        public async Task CreateAsync_WhenContentGuidIsNotSpecified_ShouldReturnCreateContent()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                requiredProperty = new { value = "Test" },
                status = "checkedOut",
                language = new { name = "en" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                var contentIsSaved = _fixture.ContentRepository.TryGet<IContent>(new Guid(jObject["contentLink"]["guidValue"].ToString()), out var savedContent);

                AssertResponse.Created(response);
                Assert.True(contentIsSaved);
                Assert.NotNull(savedContent);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenContentIsValid_ShouldReturnLocationHeader()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                requiredProperty = new { value = "Test" },
                status = "checkedOut",
                language = new { name = "en" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                Assert.True(response.Headers.Contains("Location"));

                var id = AssertResponse.Created("api/episerver/v2.0/contentmanagement/", response);

                Assert.Equal(new Guid(jObject["contentLink"]["guidValue"].ToString()), id);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenStatusIsNullAndContentIsVersionable_ShouldReturnBadRequest()
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278314");

            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid.ToString() },
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                requiredProperty = new { value = "Test" },
                language = new { name = "en" }
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                var contentIsSaved = _fixture.ContentRepository.TryGet<IContent>(contentGuid, out var savedContent);

                AssertResponse.BadRequest(response);
                Assert.Contains("Missing status value on IVersionable content", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenIdAlreadyExist_ShouldReturnConflict()
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278313");

            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid.ToString() },
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                startPublish = "2019-08-22T15:10:48Z",
                stopPublish = "2019-08-22T15:10:48Z",
                requiredProperty = new { value = "Test" },
                status = "checkedOut",
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                var contentIsSaved = _fixture.ContentRepository.TryGet<IContent>(contentGuid, out var savedContent);

                response.EnsureSuccessStatusCode();
            }

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.Conflict(response);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsValidWithLanguageIsSV_ShouldCreateContent()
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278312");

            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid.ToString() },
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "sv" },
                status = "Published",
                requiredProperty = new { value = "Test" },
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                var contentIsSaved = _fixture.ContentRepository.TryGet<IContent>(contentGuid, out var savedContent);

                AssertResponse.Created(response);
                Assert.True(contentIsSaved);
                Assert.NotNull(savedContent);
                Assert.Equal(contentApiCreateModel.name, savedContent.Name);
                Assert.Equal(_fixture.ContentTypeRepository.Load(nameof(AllPropertyPageWithValidationAttribute)).ID, savedContent.ContentTypeID);
                Assert.Equal(contentApiCreateModel.name, savedContent.Name);
                Assert.Equal(contentApiCreateModel.language.name, (savedContent as ILocale).Language.Name);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenContentIsNotLocalizableAndProvidedLanguage_ShouldReturnBadRerequest()
        {

            var contentApiCreateModel = new
            {
                name = "Test folder",
                contentType = new string[] { "SysContentFolder" },
                parentLink = new { id = ContentReference.GlobalBlockFolder.ID },
                language = new { name = "en" },
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
                Assert.Contains("Language cannot be set when content type is not localizable", await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [MemberData(nameof(InvalidVersionable))]
        public async Task CreateAsync_WhenContentIsNotVersionableAndProvidedVersionProperties_ShouldReturnBadRerequest(string startPublish, string stopPublish, string status)
        {
            var contentApiCreateModel = new
            {
                name = "Test folder",
                contentType = new string[] { "SysContentFolder" },
                parentLink = new { id = ContentReference.GlobalBlockFolder.ID },
                status,
                startPublish,
                stopPublish
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
                Assert.Contains(ProblemCode.ContentNotVersionable, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenMissingRequestBody_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, null);
            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task CreateAsync_WhenContentIsNotIRoutableAndProvidedRouteSegment_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(NestedBlock) },
                parentLink = new { id = ContentReference.GlobalBlockFolder.ID },
                language = new { name = "en" },
                status = "Published",
                routeSegment = "content-nested-block-route-segment"
            };
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
                Assert.Contains(ProblemCode.ContentNotRoutable, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task CreateAsync_WhenInvalidValueInCategory_ShouldReturnBadRequest()
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
                category = "test",
                requiredProperty = new { value = "Test" }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyContentReferenceDoestNotExist_ShouldReturnBadRequest()
        {
            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                status = "Published",
                requiredProperty = new { value = "Test" },
                contentReference = new { value = new { id = -1 } },
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
                Assert.Contains(ProblemCode.PropertyReferenceNotFound, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateAsync_WhenContentIsSharedBlockAndContainsCategoryProperty_ShouldCreateContent()
        {
            var category1 = new Category() { Name = "Category 1", Parent = _rootCategory };
            SetupSecurityDescriptor(ContentReference.GlobalBlockFolder);

            await _fixture.WithCategories(new[] { category1 }, async () =>
            {
                var contentApiCreateModel = new
                {
                    name = "Test Block",
                    contentType = new string[] { nameof(TextBlock) },
                    parentLink = new { id = ContentReference.GlobalBlockFolder },
                    language = new { name = "en" },
                    status = "Published",
                    category = new { value = new[] { new { id = category1.ID } } }
                };

                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));                
                var createdContentGuid = AssertResponse.Created(ContentManagementApiController.RoutePrefix, response);

                var createdContentResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + createdContentGuid);
                var content = JObject.Parse(await createdContentResponse.Content.ReadAsStringAsync());
                Assert.Equal(category1.ID.ToString(), (string)content["category"]["value"][0]["id"]);

                CleanupContent(createdContentGuid);
            });
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateAsync_WhenCategoryIsNull_ShouldCreateContentWithNullCategoryProperty()
        {
            var category1 = new Category() { Name = "Category 1", Parent = _rootCategory };
            SetupSecurityDescriptor(ContentReference.GlobalBlockFolder);

            await _fixture.WithCategories(new[] { category1 }, async () =>
            {
                var contentApiCreateModel = new
                {
                    name = "Test Block",
                    contentType = new string[] { nameof(TextBlock) },
                    parentLink = new { id = ContentReference.GlobalBlockFolder },
                    language = new { name = "en" },
                    status = "Published"                   
                };

                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                var createdContentGuid = AssertResponse.Created(ContentManagementApiController.RoutePrefix, response);

                var createdContentResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + createdContentGuid);
                var content = JObject.Parse(await createdContentResponse.Content.ReadAsStringAsync());
                Assert.Empty(content["category"]["value"]);

                CleanupContent(createdContentGuid);
            });
        }

        [Theory]
        [MemberData(nameof(ValidStatus))]
        public async Task CreateAsync_WhenModelIsValid_ShouldCreateContent(string versionStatus, VersionStatus expectedStatus)
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278313");

            var category1 = new Category() { Name = "Category 1", Parent = _rootCategory };
            var category2 = new Category() { Name = "Category 2", Parent = _rootCategory };

            await _fixture.WithCategories(new[] { category1, category2 }, async () =>
            {
                var contentApiCreateModel = new
                {
                    name = "Test Content",
                    contentLink = new { guidValue = contentGuid.ToString() },
                    contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                    parentLink = new { id = _securedContent.ContentLink.ID },
                    language = new { name = "en" },
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

                using (var userContext = new UserScope("authenticated", AuthorizedRole))
                {
                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                    var contentIsSaved = _fixture.ContentRepository.TryGet<AllPropertyPageWithValidationAttribute>(contentGuid, out var savedContent);

                    AssertResponse.Created(response);
                    Assert.True(contentIsSaved);
                    Assert.NotNull(savedContent);
                    Assert.Equal(contentApiCreateModel.name, savedContent.Name);
                    Assert.Equal(expectedStatus, savedContent.Status);
                    Assert.Equal(contentApiCreateModel.parentLink.id, savedContent.ParentLink.ID);
                    Assert.Equal(contentApiCreateModel.language.name, (savedContent as ILocale).Language.Name);
                    Assert.Equal(contentApiCreateModel.stopPublish.ToLocalTime(), (savedContent as IVersionable).StopPublish.Value);
                    Assert.Equal(contentApiCreateModel.startPublish.ToLocalTime(), (savedContent as IVersionable).StartPublish.Value);
                    Assert.Equal(contentApiCreateModel.routeSegment, (savedContent as IRoutable).RouteSegment.ToString());
                    contentApiCreateModel.category.value.Select(x => x.id).Should().Equals((savedContent as ICategorizable).Category.ToArray());
                    Assert.Equal(contentApiCreateModel.nestedBlock.title.value, savedContent.NestedBlock.Title);
                    Assert.Equal(contentApiCreateModel.xhtmlString.value, savedContent.XhtmlString.ToEditString());
                    Assert.Equal(contentApiCreateModel.pageType.value, savedContent.PageType.Name);
                    Assert.Equal(contentApiCreateModel.contentArea.value.First().contentLink.id, savedContent.ContentArea.Items.First().ContentLink.ID);
                    Assert.Equal(contentApiCreateModel.contentReference.value.id, savedContent.ContentReference.ID);
                    Assert.Equal(contentApiCreateModel.links.value.First().href, savedContent.Links[0].Href);

                    Assert.Null(savedContent.String);
                    Assert.Null(savedContent.AppSettings);
                    Assert.Null(savedContent.Url);
                }
            });
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsValidAndContentApprovalNotExists_ShouldReturnBadRequest()
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278313");

            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid.ToString() },
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
                status = "AwaitingApproval",
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

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.BadRequest(response);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsValidAndContentApprovalExists_ShouldCreateContent()
        {
            // set approval for parent link
            CreateApprovalSequence(_securedContent.ContentLink.ID);
            var approvalDefinitionRepo = ServiceLocator.Current.GetInstance<IApprovalDefinitionRepository>();
            var definition = await approvalDefinitionRepo.GetAsync(_securedContent.ContentLink);

            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278313");

            var contentApiCreateModel = new
            {
                name = "Test Content",
                contentLink = new { guidValue = contentGuid.ToString() },
                contentType = new string[] { nameof(AllPropertyPageWithValidationAttribute) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
                status = "AwaitingApproval",
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

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));
                var contentIsSaved = _fixture.ContentRepository.TryGet<AllPropertyPageWithValidationAttribute>(contentGuid, out var savedContent);

                AssertResponse.Created(response);
                Assert.True(contentIsSaved);
                Assert.NotNull(savedContent);
                Assert.Equal(contentApiCreateModel.name, savedContent.Name);
                Assert.Equal(VersionStatus.AwaitingApproval, savedContent.Status);
                Assert.Equal(contentApiCreateModel.parentLink.id, savedContent.ParentLink.ID);
                Assert.Equal(contentApiCreateModel.language.name, (savedContent as ILocale).Language.Name);
                Assert.Equal(contentApiCreateModel.stopPublish.ToLocalTime(), (savedContent as IVersionable).StopPublish.Value);
                Assert.Equal(contentApiCreateModel.startPublish.ToLocalTime(), (savedContent as IVersionable).StartPublish.Value);
                Assert.Equal(contentApiCreateModel.routeSegment, (savedContent as IRoutable).RouteSegment.ToString());
                Assert.Equal(contentApiCreateModel.nestedBlock.title.value, savedContent.NestedBlock.Title);
                Assert.Equal(contentApiCreateModel.xhtmlString.value, savedContent.XhtmlString.ToEditString());
                Assert.Equal(contentApiCreateModel.pageType.value, savedContent.PageType.Name);
                Assert.Equal(contentApiCreateModel.contentArea.value.First().contentLink.id, savedContent.ContentArea.Items.First().ContentLink.ID);
                Assert.Equal(contentApiCreateModel.contentReference.value.id, savedContent.ContentReference.ID);
                Assert.Equal(contentApiCreateModel.links.value.First().href, savedContent.Links[0].Href);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsValid_ShouldReturnInTheCamelCase()
        {
            var contentGuid = new Guid("1125A86E-FDD4-44BF-A105-3FA556278313");

            var contentApiCreateModel = new
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

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));

                AssertResponse.Created(response);

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
            }
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateAsync_WhenValidationModeIsInvalid_ShouldThrowBadRequest()
        {
            var contentApiCreateModel = GetContentApiCreateModel();
            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix)
            {
                Content = new JsonContent(contentApiCreateModel)
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "invalid");
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("The value 'invalid' is not valid for ContentValidationMode.", error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateAsync_WhenValidationModeIsMissing_ShouldRunAllValidation()
        {
            var contentApiCreateModel = GetContentApiCreateModel();
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix, new JsonContent(contentApiCreateModel));              

            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Always Fail!", error.Error.Message);
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateAsync_WhenValidationModeIsComplete_ShouldRunAllValidation()
        {
            var contentApiCreateModel = GetContentApiCreateModel();
            
            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix)
            {
                Content = new JsonContent(contentApiCreateModel)                    
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "complete");

            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Always Fail!", error.Error.Message);            
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task CreateAsync_WhenValidationModeIsMinimal_ShouldSkipValidation()
        {
            var contentApiCreateModel = GetContentApiCreateModel();
            var request = new HttpRequestMessage(HttpMethod.Post, ContentManagementApiController.RoutePrefix)
            {
                Content = new JsonContent(contentApiCreateModel)
            };
            request.Headers.Add(HeaderConstants.ValidationMode, "minimal");

            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.Created(response);     
        }

        private object GetContentApiCreateModel() =>
            new {
                name = "Test Content",
                contentType = new string[] { nameof(PageWithCustomValidator) },
                parentLink = new { id = _securedContent.ContentLink.ID },
                language = new { name = "en" },
                startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
                status = "Published",
                routeSegment = "alloy-plan"               
            };
        

        private void CreateApprovalSequence(int contentReference, string language = "en")
        {
            var approvalDefinitionRepo = ServiceLocator.Current.GetInstance<IApprovalDefinitionRepository>();
            var contentApprovalDefinition = new ContentApprovalDefinition()
            {
                ContentLink = new ContentReference(contentReference),
                IsEnabled = true,
                RequireCommentOnApprove = false,
                RequireCommentOnReject = false,
                RequireCommentOnStart = false,
                SelfApprove = true,
                Steps = new List<ApprovalDefinitionStep>()
                {
                    new ApprovalDefinitionStep()
                    {
                        Name = "step 01",
                        Reviewers = new List<ApprovalDefinitionReviewer>()
                        {
                            new ApprovalDefinitionReviewer("DefaultReviewer", new[] { CultureInfo.GetCultureInfo(language)})
                        }
                    }
                }
            };

            approvalDefinitionRepo.SaveAsync(contentApprovalDefinition).Wait();
        }

        public static TheoryData InvalidLanguageModel => new TheoryData<LanguageModel, string>
        {
            { new LanguageModel(), "Language name cannot be null or empty" },
            { new LanguageModel() { Name = "Test" }, "The Language 'Test' doesn't exist" },
            { new LanguageModel() { Name = "en-US" }, "The Language 'en-US' is not enabled in CMS" }
        };

        public static TheoryData InvalidStatus => new TheoryData<object, string>
        {
            { 10, "The status '10' is invalid" },
            { "NotCreated", "The status 'NotCreated' is invalid" },
            { "PreviouslyPublished", "The status 'PreviouslyPublished' is invalid" }
        };

        public static TheoryData ValidStatus => new TheoryData<string, VersionStatus>
        {
            { "CheckedIn", VersionStatus.CheckedIn },
            { "CheckedOut", VersionStatus.CheckedOut },
            { "Published", VersionStatus.Published },
            { "DelayedPublish", VersionStatus.DelayedPublish },
            { "Rejected ", VersionStatus.Rejected }
        };

        public static TheoryData InvalidVersionable => new TheoryData<string, string, string>
        {
            { "2020-10-20 08:15:42+02:00", null, null },
            { null, "2020-10-20 09:15:42+02:00", null },
            { null, null, "published" },
        };
    }    
}
