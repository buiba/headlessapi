using System;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.Core;
using EPiServer.Web;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi
    {

        private IContent _sourceContent;
        private IContent _destinationContent;

        [Fact]
        public async Task Move_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, ContentManagementApiController.RoutePrefix + $"{Guid.NewGuid()}/move");
            var response = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(response);
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenSourceContentDoesNotExist_ShouldReturnNotFound()
        {
            var requestBody = new MoveContentModel
            {
                ParentLink = new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{Guid.NewGuid()}/move", new JsonContent(requestBody));
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenParentGuidIsInvalid_ShouldReturnBadRequest()
        {
            var requestBody = new MoveContentModel
            {
                ParentLink = new ContentReferenceInputModel { GuidValue = null }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));
            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenDestinationDoesNotExist_ShouldReturnBadRequest()
        {
            var requestBody = new MoveContentModel
            {
                ParentLink = new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));
            AssertResponse.BadRequest(response);
            Assert.Contains("Cannot get parent content", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenSourceContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
                {
                    var requestBody = new MoveContentModel
                    {
                        ParentLink = new ContentReferenceInputModel { GuidValue = _destinationContent.ContentGuid }
                    };

                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));
                    AssertResponse.Forbidden(response);
                }
            }
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenDestinationContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
                {
                    var requestBody = new MoveContentModel
                    {
                        ParentLink = new ContentReferenceInputModel { GuidValue = _destinationContent.ContentGuid }
                    };

                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));
                    AssertResponse.Forbidden(response);
                }
            }
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenSourceContentAndDestinationContentAreExposedToApi_ShouldReturnNoContent()
        {
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
                {
                    SetupSecurityDescriptor(_sourceContent.ContentLink, addContentApiWriteRole: true);
                    SetupSecurityDescriptor(_destinationContent.ContentLink, addContentApiWriteRole: true);

                    var requestBody = new MoveContentModel
                    {
                        ParentLink = new ContentReferenceInputModel { GuidValue = _destinationContent.ContentGuid }
                    };

                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));
                    AssertResponse.NoContent(response);
                }
            }
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenUserHaveSufficientAccessToMove_ShouldReturnNoContent()
        {
            var requestBody = new MoveContentModel
            {
                ParentLink = new ContentReferenceInputModel { GuidValue = _destinationContent.ContentGuid }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));
                AssertResponse.NoContent(response);
            }
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenUserDoesNotHaveSufficientAccessToMove_ShouldReturnForbidden()
        {
            var requestBody = new MoveContentModel
            {
                ParentLink = new ContentReferenceInputModel { GuidValue = _destinationContent.ContentGuid }
            };

            using (var userContext = new UserScope("anonymous", _anonymousRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));
                AssertResponse.Forbidden(response);
            }
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenMovingActionIsInvalid_ShouldReturnBadRequest()
        {
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move",
                    new JsonContent(new MoveContentModel
                    {
                        ParentLink = new ContentReferenceInputModel { GuidValue = _sourceContent.ContentGuid }
                    }));

                AssertResponse.BadRequest(response);
            }
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenSourceContentIsSystemContentOfAnotherSite_ShouldReturnBadRequest()
        {
            var startPage = _fixture.GetWithDefaultName<StartPage>(ContentReference.RootPage, true);
            await _fixture.WithContent(startPage, async () =>
            {
                var anotherSite = new SiteDefinition
                {
                    Id = Guid.NewGuid(),
                    SiteUrl = new Uri($"http://one.com/"),
                    Name = "Another site",
                    StartPage = startPage.ContentLink,
                    Hosts = { new HostDefinition { Name = "one.com" } }
                };

                await _fixture.WithSite(anotherSite, async () =>
                {
                    var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{startPage.ContentGuid}/move", new JsonContent(new MoveContentModel
                    {
                        ParentLink = new ContentReferenceInputModel { GuidValue = _sourceContent.ContentGuid }
                    }));
                    AssertResponse.BadRequest(response);
                    Assert.Contains("Cannot move system content.", await response.Content.ReadAsStringAsync());
                });
            }, false);
        }

        [Fact]
        public async Task Move_ByContentGuid_WhenBodyNull_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{Guid.NewGuid()}/move", new JsonContent(null));
            AssertResponse.BadRequest(response);
            Assert.Contains("Request body is required.", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [MemberData(nameof(InvalidMovingDestinationModel))]
        public async Task Move_ByContentGuid_WhenDestinationIsNotValidModel_ShouldReturnBadRequest(object requestBody, string invalidTarget)
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentGuid}/move", new JsonContent(requestBody));

            AssertResponse.BadRequest(response);
            Assert.Contains(invalidTarget, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Move_ByContentReference_WhenBodyNull_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_publicContent.ContentLink.ID}/move", new JsonContent(null));
            AssertResponse.BadRequest(response);
            Assert.Contains("Request body is required.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Move_ByContentReference_WhenSourceContentIsNotContentReference_ShouldReturnBadRequest()
        {
            var contentReference = "blargh";
            var requestBody = new MoveContentModel() { ParentLink = new ContentReferenceInputModel() };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{contentReference}/move", new JsonContent(requestBody));
            AssertResponse.BadRequest(response);
            Assert.Contains($"'{contentReference}' is incorrect format.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Move_ByContentReference_WhenDestinationDoesNotExist_ShouldReturnBadRequest()
        {
            var requestBody = new MoveContentModel
            {
                ParentLink = new ContentReferenceInputModel { Id = int.MaxValue }
            };

            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentLink.ID}/move", new JsonContent(requestBody));
            AssertResponse.BadRequest(response);
            Assert.Contains("Cannot get parent content", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Move_ByContentReference_WhenUserHaveSufficientAccessToMove_ShouldReturnNoContent()
        {
            var requestBody = new MoveContentModel
            {
                ParentLink = new ContentReferenceInputModel { Id = _destinationContent.ContentLink.ID }
            };

            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            {
                var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentLink.ID}/move", new JsonContent(requestBody));
                AssertResponse.NoContent(response);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidMovingDestinationModel))]
        public async Task Move_ByContentReference_WhenDestinationIsNotValidModel_ShouldReturnBadRequest(object requestBody, string invalidTarget)
        {
            var response = await _fixture.Client.PostAsync(ContentManagementApiController.RoutePrefix + $"{_sourceContent.ContentLink.ID}/move", new JsonContent(requestBody));

            AssertResponse.BadRequest(response);
            Assert.Contains(invalidTarget, await response.Content.ReadAsStringAsync());
        }

        public static TheoryData InvalidMovingDestinationModel => new TheoryData<object, string>
        {
            { new { ParentLink = new { Id = true } }, "ParentLink.Id"},
            { new { ParentLink = new { Id = new int[]{ 123 } } }, "ParentLink.Id"},
            { new { ParentLink = new { Id = new {Id = 123} } }, "ParentLink.Id"},
            { new { ParentLink = new { GuidValue = true } }, "ParentLink.GuidValue"},
            { new { ParentLink = new { GuidValue = new int[] { 123 } } }, "ParentLink.GuidValue"},
            { new { ParentLink = new { GuidValue = new {Id = 123} } }, "ParentLink.GuidValue"}
        };
    }
}
