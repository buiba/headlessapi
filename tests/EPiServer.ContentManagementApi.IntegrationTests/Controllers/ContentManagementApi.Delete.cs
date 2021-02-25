using System;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using EPiServer.Web;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    public partial class ContentManagementApi
    {
        [Fact]
        public async Task Delete_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, ContentManagementApiController.RoutePrefix + Guid.NewGuid());
            var response = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(response);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentDoesNotExists_ShouldReturnNotFound()
        {
            var response = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + Guid.NewGuid());
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentIsSystemContent_ShouldReturnBadRequest()
        {
            var wasteBasketGuid = "2f40ba47-f4fc-47ae-a244-0b909d4cf988";
            var response = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + wasteBasketGuid);
            AssertResponse.BadRequest(response);
            Assert.Contains("Cannot delete system content.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentIsSystemContentOnAnotherSite_ShouldReturnBadRequest()
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
                    var response = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + startPage.ContentGuid);
                    AssertResponse.BadRequest(response);
                    Assert.Contains("Cannot delete system content.", await response.Content.ReadAsStringAsync());
                });
            }, false);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentExistsAndAuthenticated_ShouldReturnNoContent()
        {
            using var userContext = new UserScope("authenticated", AuthorizedRole);
            var moveToTrash =
                await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + _deletedContent.ContentGuid);
            AssertResponse.NoContent(moveToTrash);

            // make sure the created content is move to trash
            var contentWastebasket = _fixture.ContentRepository.Get<IContent>(_deletedContent.ContentGuid);
            Assert.True(contentWastebasket.IsDeleted);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentExistsAndUnAuthenticated_ShouldReturnAccessDenied()
        {
            using var userContext = new UserScope("anonymous", _anonymousRole);
            var moveToTrash =
                await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + _deletedContent.ContentGuid);
            AssertResponse.Forbidden(moveToTrash);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenPermanentDeleteHeaderIsTrue_ShouldPermanentDeleteWithoutMovingToRecycleBin()
        {
            using var userContext = new UserScope("authenticated", AuthorizedRole);
            var deletedContentGuid = _deletedContent.ContentGuid;

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, ContentManagementApiController.RoutePrefix + deletedContentGuid);
            requestMessage.Headers.Add(HeaderConstants.PermanentDeleteHeaderName, "true");

            var contentResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NoContent(contentResponse);

            // make sure the page is deleted permanently
            var deletedPage =
                _fixture.ContentRepository.TryGet<IContent>(deletedContentGuid, out var permanentDeletedContent);
            Assert.False(deletedPage);
            Assert.Null(permanentDeletedContent);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenPermanentDeleteHeaderIsFalse_ShouldNotPermanentDelete()
        {
            using var userContext = new UserScope("authenticated", AuthorizedRole);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, ContentManagementApiController.RoutePrefix + _deletedContent.ContentGuid);
            requestMessage.Headers.Add(HeaderConstants.PermanentDeleteHeaderName, "false");

            var contentResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NoContent(contentResponse);

            // make sure permanent delete is prevented
            var contentWastebasket = _fixture.ContentRepository.Get<IContent>(_deletedContent.ContentGuid);
            Assert.NotNull(contentWastebasket);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenPermanentDeleteHeaderIsFalse_AndContentIsInRecycleBin_ShouldReturnConflict()
        {
            using var userContext = new UserScope("authenticated", AuthorizedRole);

            // move content to recycle bin
            _fixture.ContentRepository.MoveToWastebasket(_deletedContent.ContentLink);

            // try deleting the content in the recycle bin
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, ContentManagementApiController.RoutePrefix + _deletedContent.ContentGuid);
            requestMessage.Headers.Add(HeaderConstants.PermanentDeleteHeaderName, "false");

            var contentPermanentDeleteResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.Conflict(contentPermanentDeleteResponse);
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentExistsAndUnAuthenticated_ShouldReturnForbidden()
        {
            using (var userContext = new UserScope("anonymous", _anonymousRole))
            {
                var moveToTrash = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + _deletedContent.ContentGuid);
                AssertResponse.Forbidden(moveToTrash);
            }
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentIsNotExposedToApi_ShouldReturnForbidden()
        {
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
            {
                var moveToTrash = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + _deletedContent.ContentGuid);
                AssertResponse.Forbidden(moveToTrash);
            }
        }

        [Fact]
        public async Task Delete_ByContentGuid_WhenContentIsExposedToApi_ShouldReturnNoContent()
        {
            using (var userContext = new UserScope("authenticated", AuthorizedRole))
            using (new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole)))
            {
                SetupSecurityDescriptor(_deletedContent.ContentLink, addContentApiWriteRole: true);
                var moveToTrash = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + _deletedContent.ContentGuid);
                AssertResponse.NoContent(moveToTrash);
            }
        }

        [Fact]
        public async Task Delete_ByContentReference_WhenContentExistsAndAuthenticated_ShouldReturnNoContent()
        {
            using var userContext = new UserScope("authenticated", AuthorizedRole);
            using var scope = new OptionsScope(o => o.SetRequiredRole(_contentApiWriteRole));

            SetupSecurityDescriptor(_deletedContent.ContentLink, addContentApiWriteRole: true);
            var moveToTrash = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + _deletedContent.ContentLink.ID);
            AssertResponse.NoContent(moveToTrash);

            // make sure the created content is move to trash
            var contentWastebasket = _fixture.ContentRepository.Get<IContent>(_deletedContent.ContentGuid);
            Assert.True(contentWastebasket.IsDeleted);
        }

        [Fact]
        public async Task Delete_ByContentReference_WhenPermanentDeleteHeaderIsTrue_ShouldPermanentDeleteWithoutMovingToRecycleBin()
        {
            using var userContext = new UserScope("authenticated", AuthorizedRole);
            var deletedContentGuid = _deletedContent.ContentGuid;

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, ContentManagementApiController.RoutePrefix + _deletedContent.ContentLink.ID);
            requestMessage.Headers.Add(HeaderConstants.PermanentDeleteHeaderName, "true");

            var contentResponse = await _fixture.Client.SendAsync(requestMessage);
            AssertResponse.NoContent(contentResponse);

            // make sure the page is deleted permanently
            var deletedPage = _fixture.ContentRepository.TryGet<IContent>(deletedContentGuid, out var permanentDeletedContent);
            Assert.False(deletedPage);
            Assert.Null(permanentDeletedContent);
        }

        [Fact]
        public async Task Delete_ByContentReference_WhenContentDoesNotExists_ShouldReturnNotFound()
        {
            var response = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + int.MaxValue);
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task Delete_ByContentReference_WhenContentReferenceIncorrect_ShouldReturnBadRequest()
        {
            var contentReference = "blargh";
            var response = await _fixture.Client.DeleteAsync(ContentManagementApiController.RoutePrefix + contentReference);
            AssertResponse.BadRequest(response);
            Assert.Contains($"'{contentReference}' is incorrect format.", await response.Content.ReadAsStringAsync());
        }
    }
}
