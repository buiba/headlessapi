using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi
    {
        #region ContentGuid

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            var contentApiPatchModel = new
            {
                name = "name"
            };

            using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                await _fixture.WithContentItems(new[] { contentItem }, async () =>
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + contentItem.ContentGuid, new JsonContent(contentApiPatchModel));
                    AssertResponse.Forbidden(response);
                });
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenContentDoesNotExist_ShouldReturnNotFound()
        {
            var contentApiPatchModel = new
            {
                name = "name"
            };
            var contentGuid = Guid.NewGuid();
            var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + contentGuid, new JsonContent(contentApiPatchModel));
            AssertResponse.NotFound(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal($"The content with id ({contentGuid}) does not exist.", error.Error.Message);
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenMissingRequestBody_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid, null);
            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Request body is required.", error.Error.Message);
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenNameIsMoreThan255Characters_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = new string('A', 256),
            };
            var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid, new JsonContent(contentApiPatchModel));
            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains("'Name' has exceeded its limit of 255 characters.", error);
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenContentIsNotIRoutableAndProvidedRouteSegment_ShouldReturnBadRequest()
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            SetupSecurityDescriptor(block.Property.OwnerLink, true);
            var contentApiPatchModel = new
            {
                name = "name",
                routeSegment = "content-nested-block-route-segment"
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + block.Property.OwnerLink.ID, new JsonContent(contentApiPatchModel));

                AssertResponse.BadRequest(response);
                var error = await response.Content.ReadAs<ErrorResponse>();
                Assert.Equal(ProblemCode.ContentNotRoutable, error.Error.Code);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenHasNonBranchSpecificNestedBlock_AndNotProvideMasterLanguage_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                language = new { name = "sv" },
                routeSegment = "alloy-plan",
                nonBranchSpecificNestedBlock = new { title = new { value = "Test nested" } }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var svContent = _fixture.CreateLanguageBranchWithDefaultName<StandardPageWithNonBranchSpecificNestedBlock>(_securedContentWithNonBranchSpecificNestedBlock.ContentLink, true);

                await _fixture.WithContent(svContent, async () =>
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + svContent.ContentGuid, new JsonContent(contentApiPatchModel));
                    AssertResponse.BadRequest(response);
                    var error = await response.Content.ReadAs<ErrorResponse>();
                    Assert.Equal("Cannot provide non-branch specific property 'NonBranchSpecificNestedBlock' when not passing the master language", error.Error.Message);
                });
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenHasNonBranchSpecificProperty_AndNotPassingMasterLanguage_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "sv" },
                nonBranchSpecificProperty = new { value = "no branch specific test" }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var svContent = _fixture.CreateLanguageBranchWithDefaultName<StandardPageWithNonBranchSpecificProperty>(_securedContentWithNonBranchSpecific.ContentLink, true);

                await _fixture.WithContent(svContent, async () =>
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContentWithNonBranchSpecific.ContentGuid, new JsonContent(contentApiPatchModel));

                    AssertResponse.BadRequest(response);
                    var error = await response.Content.ReadAs<ErrorResponse>();
                    Assert.Equal(
                        $"Cannot provide non-branch specific property '{nameof(contentApiPatchModel.nonBranchSpecificProperty)}' when not passing the master language",
                        error.Error.Message, true);
                });
            }
        }

        [Theory]
        [MemberData(nameof(InvalidStatus))]
        public async Task PatchAsync_ByContentGuid_WhenStatusIsInvalid_ShouldReturnValidationError(object versionStatus, string expectedError)
        {
            var contentApiPatchModel = new
            {
                status = versionStatus
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid, new JsonContent(contentApiPatchModel));
                AssertResponse.ValidationError(response);
                Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenStatusIsDelayedPublishAndStartPublishMissing_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = "Test",
                status = "DelayedPublish"
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentGuid, new JsonContent(contentApiPatchModel));
                AssertResponse.ValidationError(response);
                var error = await response.Content.ReadAs<ErrorResponse>();
                Assert.Equal("StartPublish must be set when content item is set for scheduled publishing", error.Error.Message);

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenStatusIsDelayedPublishAndHaveStartPublish_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                status = "DelayedPublish",
                startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);

                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentGuid, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);
                var updatedContent = _fixture.ContentRepository.Get<StandardPage>(content.ContentGuid, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(content.Name, updatedContent.Name);
                Assert.Equal(VersionStatus.DelayedPublish, updatedContent.Status);
                Assert.NotNull(updatedContent.StartPublish);
                Assert.Equal(contentApiPatchModel.startPublish.UtcDateTime, updatedContent.StartPublish.Value.ToUniversalTime());

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenHasNonBranchSpecificNestedBlock_AndProvideMasterLanguage_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "en" },
                nonBranchSpecificNestedBlock = new { title = new { value = "Test nested" } }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContentWithNonBranchSpecificNestedBlock.ContentGuid, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);
                var updatedContent = _fixture.ContentRepository.Get<StandardPageWithNonBranchSpecificNestedBlock>(_securedContentWithNonBranchSpecificNestedBlock.ContentGuid, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                Assert.Equal(contentApiPatchModel.nonBranchSpecificNestedBlock.title.value, updatedContent.NonBranchSpecificNestedBlock.Title);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenHasNonBranchSpecificProperty_AndPassingMasterLanguage_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "en" },
                nonBranchSpecificProperty = new { value = "no branch specific test" }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContentWithNonBranchSpecific.ContentGuid, new JsonContent(contentApiPatchModel));

                AssertResponse.NoContent(response);
                var updatedContent = _fixture.ContentRepository.Get<StandardPageWithNonBranchSpecificProperty>(_securedContentWithNonBranchSpecific.ContentGuid, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                Assert.Equal(contentApiPatchModel.nonBranchSpecificProperty.value, updatedContent.NonBranchSpecificProperty);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenAwaitingApprovalAndApprovalSequenceNotExist_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                status = "AwaitingApproval",
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);

                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentGuid, new JsonContent(contentApiPatchModel));
                AssertResponse.BadRequest(response);
                var error = await response.Content.ReadAs<ErrorResponse>();
                Assert.Equal("Action RequestApproval requires that an approval definition is defined", error.Error.Message);

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenAwaitingApprovalAndApprovalSequenceExists_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "en" },
                status = "AwaitingApproval",
                mainContentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
                mainBody = new { value = "<p>Test</p>\n" },

            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);

                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);
                // set approval for parent link
                CreateApprovalSequence(content.ContentLink.ID);
                var approvalDefinitionRepo = ServiceLocator.Current.GetInstance<IApprovalDefinitionRepository>();
                _ = await approvalDefinitionRepo.GetAsync(content.ContentLink);

                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentGuid, new JsonContent(contentApiPatchModel));
                var updatedContent = _fixture.ContentRepository.Get<StandardPage>(content.ContentGuid, CultureInfo.GetCultureInfo("en"));

                AssertResponse.NoContent(response);
                Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                Assert.Equal(VersionStatus.AwaitingApproval, updatedContent.Status);
                Assert.Equal(contentApiPatchModel.language.name, (updatedContent as ILocale).Language.Name);
                Assert.Equal(contentApiPatchModel.mainContentArea.value.First().contentLink.id, updatedContent.MainContentArea.Items.First().ContentLink.ID);
                Assert.Equal(contentApiPatchModel.mainBody.value, updatedContent.MainBody.ToEditString());

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenContentIsExposedToApi_ShouldUpdateCommonDraft()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentApiPatchModel = new
                {
                    name = "Updated Content",
                    mainContentArea = new
                    {
                        value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                    },
                    mainBody = new { value = "<p>Test</p>\n" },
                };

                using (new UserScope("authenticated", AuthorizedRole))
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid, new JsonContent(contentApiPatchModel));
                    var updatedContent = _fixture.ContentRepository.Get<StandardPage>(_securedContent.ContentGuid, CultureInfo.GetCultureInfo("en"));

                    AssertResponse.NoContent(response);
                    Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                    Assert.Equal(contentApiPatchModel.mainContentArea.value.First().contentLink.id, updatedContent.MainContentArea.Items.First().ContentLink.ID);
                    Assert.Equal(contentApiPatchModel.mainBody.value, updatedContent.MainBody.ToEditString());
                }
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenMetaDataIsValid_ShouldUpdateCommonDraft()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentApiPatchModel = new
                {
                    name = "Test Content",
                    routeSegment = "updated-route",
                    status = "DelayedPublish",
                    startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                    stopPublish = (string) null,
                };

                using (new UserScope("authenticated", AuthorizedRole))
                {
                    var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                    SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentGuid, new JsonContent(contentApiPatchModel));
                    AssertResponse.NoContent(response);

                    var updatedContent = _fixture.ContentRepository.Get<StandardPage>(content.ContentGuid, CultureInfo.GetCultureInfo("en"));
                    Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                    Assert.Equal(contentApiPatchModel.routeSegment, updatedContent.URLSegment);
                    Assert.Equal(VersionStatus.DelayedPublish, updatedContent.Status);
                    Assert.NotNull(updatedContent.StartPublish);
                    Assert.Equal(contentApiPatchModel.startPublish.UtcDateTime, updatedContent.StartPublish.Value.ToUniversalTime());
                    Assert.Null(updatedContent.StopPublish);

                    _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
                }
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentGuid_WhenContentIsValid_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Test Content",
                status = "Published",
                heading = new
                {
                    value = "heading"
                },
                mainContentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
                mainBody = new { value = "<p>Test</p>\n" },
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);

                var updatedContent = _fixture.ContentRepository.Get<StandardPage>(_securedContent.ContentGuid, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(contentApiPatchModel.mainContentArea.value.First().contentLink.id, updatedContent.MainContentArea.Items.First().ContentLink.ID);
                Assert.Equal(contentApiPatchModel.mainBody.value, updatedContent.MainBody.ToEditString());
                Assert.Equal(contentApiPatchModel.heading.value, updatedContent.Heading);
            }
        }

        [Fact]
        [UserScope("authenticated", AuthorizedRole)]
        public async Task PatchAsync_WhenPropertyIsMissing_ShouldNotUpdateProperty()
        {
            var category1 = new Category() { Name = "Category 1", Parent = _rootCategory };
            await _fixture.WithCategories(new[] { category1 }, async () =>
            {
                // Setup some properties of content to make sure it has non-null value 
                var versionContentCreated = new
                {
                    name = "new version",
                    contentType = new[] { nameof(StandardPage) },
                    parentLink = new { id = ContentReference.StartPage.ID },
                    status = "checkedOut",
                    language = new { name = "en" },
                    heading = new { value = "Heading" },
                    links = new { value = new[] { new { href = "https://episerver.com" } } },
                    mainContentArea = new
                    {
                        value = new[] { new { contentLink = new { id = ContentReference.StartPage.ID } } },
                    },
                    category = new { value = new[] { new { id = category1.ID } } }
                };
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_securedContent.ContentGuid}", new JsonContent(versionContentCreated));
                AssertResponse.Created(response);                

                var contentApiPatchModel = new
                {
                    name = "Test Content"                    
                };
                response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);                

                var createdContentResponse = await _fixture.Client.GetAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentGuid);
                var content = JObject.Parse(await createdContentResponse.Content.ReadAsStringAsync());

                content["mainContentArea"]["value"].Should().HaveCount(1);
                Assert.Equal(versionContentCreated.mainContentArea.value[0].contentLink.id, content["mainContentArea"]["value"][0]["contentLink"]["id"]);                
                content["heading"]["value"].Should().HaveValue(versionContentCreated.heading.value);                
                content["links"]["value"].Should().HaveCount(1);
                content["links"]["value"][0]["href"].Should().HaveValue(versionContentCreated.links.value[0].href);
                content["category"]["value"].Should().HaveCount(1);
                content["category"]["value"][0]["id"].Should().HaveValue(versionContentCreated.category.value[0].id.ToString());                
            });                               
        }

        #endregion

        #region ContentReference

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            var contentApiPatchModel = new
            {
                name = "name"
            };

            using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                await _fixture.WithContentItems(new[] { contentItem }, async () =>
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + contentItem.ContentLink.ID, new JsonContent(contentApiPatchModel));
                    AssertResponse.Forbidden(response);
                });
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenContentDoesNotExist_ShouldReturnNotFound()
        {
            var contentApiPatchModel = new
            {
                name = "name"
            };
            var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + 99999, new JsonContent(contentApiPatchModel));
            AssertResponse.NotFound(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal($"The content with id ({99999}) does not exist.", error.Error.Message);
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenMissingRequestBody_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentLink.ID, null);
            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAs<ErrorResponse>();
            Assert.Equal("Request body is required.", error.Error.Message);
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenNameIsMoreThan255Characters_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = new string('A', 256)
            };
            var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentLink.ID, new JsonContent(contentApiPatchModel));
            AssertResponse.BadRequest(response);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains("'Name' has exceeded its limit of 255 characters.", error);
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenContentIsNotIRoutableAndProvidedRouteSegment_ShouldReturnBadRequest()
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            SetupSecurityDescriptor(block.Property.OwnerLink, true);
            var contentApiPatchModel = new
            {
                name = "name",
                routeSegment = "content-nested-block-route-segment"
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + block.Property.OwnerLink.ID, new JsonContent(contentApiPatchModel));

                AssertResponse.BadRequest(response);
                var error = await response.Content.ReadAs<ErrorResponse>();
                Assert.Equal(ProblemCode.ContentNotRoutable, error.Error.Code);

                _fixture.ContentRepository.Delete(block.Property.OwnerLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenHasNonBranchSpecificNestedBlock_AndNotProvideMasterLanguage_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "sv" },
                routeSegment = "alloy-plan",
                nonBranchSpecificNestedBlock = new { title = new { value = "Test nested" } }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var svContent = _fixture.CreateLanguageBranchWithDefaultName<StandardPageWithNonBranchSpecificNestedBlock>(_securedContentWithNonBranchSpecificNestedBlock.ContentLink, true);

                await _fixture.WithContent(svContent, async () =>
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + svContent.ContentLink.ID, new JsonContent(contentApiPatchModel));
                    AssertResponse.BadRequest(response);
                    var error = await response.Content.ReadAs<ErrorResponse>();
                    Assert.Contains("Cannot provide non-branch specific property 'NonBranchSpecificNestedBlock' when not passing the master language", error.Error.Message);
                });
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenHasNonBranchSpecificProperty_AndNotPassingMasterLanguage_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "sv" },
                nonBranchSpecificProperty = new { value = "no branch specific test" }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var svContent = _fixture.CreateLanguageBranchWithDefaultName<StandardPageWithNonBranchSpecificProperty>(_securedContentWithNonBranchSpecific.ContentLink, true);

                await _fixture.WithContent(svContent, async () =>
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContentWithNonBranchSpecific.ContentLink.ID, new JsonContent(contentApiPatchModel));

                    AssertResponse.BadRequest(response);
                    var error = await response.Content.ReadAs<ErrorResponse>();
                    Assert.Equal(
                        $"Cannot provide non-branch specific property '{nameof(contentApiPatchModel.nonBranchSpecificProperty)}' when not passing the master language",
                        error.Error.Message, true);
                });
            }
        }

        [Theory]
        [MemberData(nameof(InvalidStatus))]
        public async Task PatchAsync_ByContentReference_WhenStatusIsInvalid_ShouldReturnValidationError(object versionStatus, string expectedError)
        {
            var contentApiPatchModel = new
            {
                status = versionStatus
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentLink.ID, new JsonContent(contentApiPatchModel));
                AssertResponse.ValidationError(response);
                Assert.Contains(expectedError, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenStatusIsDelayedPublishAndStartPublishMissing_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = "Test",
                status = "DelayedPublish"
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);

                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentLink.ID, new JsonContent(contentApiPatchModel));
                AssertResponse.ValidationError(response);
                var error = await response.Content.ReadAs<ErrorResponse>();
                Assert.Equal("StartPublish must be set when content item is set for scheduled publishing", error.Error.Message);

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenStatusIsDelayedPublishAndHaveStartPublish_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Name",
                status = "DelayedPublish",
                startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                stopPublish = DateTimeOffset.Parse("2020-11-20 09:15:42+02:00"),
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);

                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentLink.ID, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);
                var updatedContent = _fixture.ContentRepository.Get<StandardPage>(content.ContentGuid, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                Assert.Equal(VersionStatus.DelayedPublish, updatedContent.Status);
                Assert.NotNull(updatedContent.StartPublish);
                Assert.Equal(contentApiPatchModel.startPublish.UtcDateTime, updatedContent.StartPublish.Value.ToUniversalTime());

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenHasNonBranchSpecificNestedBlock_AndProvideMasterLanguage_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "en" },
                nonBranchSpecificNestedBlock = new { title = new { value = "Test nested" } }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContentWithNonBranchSpecificNestedBlock.ContentLink.ID, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);
                var updatedContent = _fixture.ContentRepository.Get<StandardPageWithNonBranchSpecificNestedBlock>(_securedContentWithNonBranchSpecificNestedBlock.ContentLink, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                Assert.Equal(contentApiPatchModel.nonBranchSpecificNestedBlock.title.value, updatedContent.NonBranchSpecificNestedBlock.Title);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenHasNonBranchSpecificProperty_AndPassingMasterLanguage_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "en" },
                nonBranchSpecificProperty = new { value = "no branch specific test" }
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContentWithNonBranchSpecific.ContentLink.ID, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);

                var updatedContent = _fixture.ContentRepository.Get<StandardPageWithNonBranchSpecificProperty>(_securedContentWithNonBranchSpecific.ContentLink, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                Assert.Equal(contentApiPatchModel.nonBranchSpecificProperty.value, updatedContent.NonBranchSpecificProperty);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenAwaitingApprovalAndApprovalSequenceNotExist_ShouldReturnBadRequest()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                status = "AwaitingApproval",
            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);

                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentLink.ID, new JsonContent(contentApiPatchModel));
                AssertResponse.BadRequest(response);
                var error = await response.Content.ReadAs<ErrorResponse>();
                Assert.Contains("Action RequestApproval requires that an approval definition is defined", error.Error.Message);

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenAwaitingApprovalAndApprovalSequenceExists_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Updated Content",
                language = new { name = "en" },
                status = "AwaitingApproval",
                mainContentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
                mainBody = new { value = "<p>Test</p>\n" },

            };

            using (new UserScope("authenticated", AuthorizedRole))
            {
                var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);
                // set approval for parent link
                CreateApprovalSequence(content.ContentLink.ID);
                var approvalDefinitionRepo = ServiceLocator.Current.GetInstance<IApprovalDefinitionRepository>();
                _ = await approvalDefinitionRepo.GetAsync(content.ContentLink);

                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + $"{content.ContentLink.ID}", new JsonContent(contentApiPatchModel));
                var updatedContent = _fixture.ContentRepository.Get<StandardPage>(content.ContentGuid, CultureInfo.GetCultureInfo("en"));

                AssertResponse.NoContent(response);
                Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                Assert.Equal(VersionStatus.AwaitingApproval, updatedContent.Status);
                Assert.Equal(contentApiPatchModel.language.name, (updatedContent as ILocale).Language.Name);
                Assert.Equal(contentApiPatchModel.mainContentArea.value.First().contentLink.id, updatedContent.MainContentArea.Items.First().ContentLink.ID);
                Assert.Equal(contentApiPatchModel.mainBody.value, updatedContent.MainBody.ToEditString());

                _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenContentIsExposedToApi_ShouldUpdateCommonDraft()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_securedContent.ContentLink, addContentApiWriteRole: true);

                var contentApiPatchModel = new
                {
                    name = "Updated Content",
                    mainContentArea = new
                    {
                        value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                    },
                    mainBody = new { value = "<p>Test</p>\n" },
                };

                using (new UserScope("authenticated", AuthorizedRole))
                {
                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentLink.ID, new JsonContent(contentApiPatchModel));
                    var updatedContent = _fixture.ContentRepository.Get<StandardPage>(_securedContent.ContentGuid, CultureInfo.GetCultureInfo("en"));

                    AssertResponse.NoContent(response);
                    Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                    Assert.Equal(contentApiPatchModel.mainContentArea.value.First().contentLink.id, updatedContent.MainContentArea.Items.First().ContentLink.ID);
                    Assert.Equal(contentApiPatchModel.mainBody.value, updatedContent.MainBody.ToEditString());
                }
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenMetaDataIsValid_ShouldUpdateCommonDraft()
        {
            using (new OptionsScope(m => m.SetRequiredRole(_contentApiWriteRole)))
            {
                var contentApiPatchModel = new
                {
                    name = "Test Content",
                    routeSegment = "updated-route-1",
                    status = "DelayedPublish",
                    startPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                    stopPublish = (string) null,
                };

                using (new UserScope("authenticated", AuthorizedRole))
                {
                    var content = _fixture.GetDraftWithDefaultName<StandardPage>(ContentReference.StartPage, true);
                    SetupSecurityDescriptor(content.ContentLink, addContentApiWriteRole: true);

                    var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + content.ContentLink.ID, new JsonContent(contentApiPatchModel));
                    AssertResponse.NoContent(response);

                    var updatedContent = _fixture.ContentRepository.Get<StandardPage>(content.ContentLink, CultureInfo.GetCultureInfo("en"));
                    Assert.Equal(contentApiPatchModel.name, updatedContent.Name);
                    Assert.Equal(contentApiPatchModel.routeSegment, updatedContent.URLSegment);
                    Assert.Equal(VersionStatus.DelayedPublish, updatedContent.Status);
                    Assert.NotNull(updatedContent.StartPublish);
                    Assert.Equal(contentApiPatchModel.startPublish.UtcDateTime, updatedContent.StartPublish.Value.ToUniversalTime());
                    Assert.Null(updatedContent.StopPublish);

                    _fixture.ContentRepository.Delete(content.ContentLink, true, AccessLevel.NoAccess);
                }
            }
        }

        [Fact]
        public async Task PatchAsync_ByContentReference_WhenContentIsValid_ShouldUpdateCommonDraft()
        {
            var contentApiPatchModel = new
            {
                name = "Test Content",
                status = "Published",
                heading = new
                {
                    value = "heading"
                },
                mainContentArea = new
                {
                    value = new[] { new { contentLink = new { id = _securedContent.ContentLink.ID } } },
                },
                mainBody = new { value = "<p>Test</p>\n" },
            };
            using (new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PatchAsync(ContentManagementApiController.RoutePrefix + _securedContent.ContentLink.ID, new JsonContent(contentApiPatchModel));
                AssertResponse.NoContent(response);
                var updatedContent = _fixture.ContentRepository.Get<StandardPage>(_securedContent.ContentGuid, CultureInfo.GetCultureInfo("en"));
                Assert.Equal(contentApiPatchModel.mainContentArea.value.First().contentLink.id, updatedContent.MainContentArea.Items.First().ContentLink.ID);
                Assert.Equal(contentApiPatchModel.mainBody.value, updatedContent.MainBody.ToEditString());
                Assert.Equal(contentApiPatchModel.heading.value, updatedContent.Heading);
            }
        }

        #endregion
    }
}
