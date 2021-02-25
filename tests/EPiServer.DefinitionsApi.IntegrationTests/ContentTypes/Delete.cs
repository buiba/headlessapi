using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.IntegrationTests;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class Delete
    {
        private readonly ServiceFixture _fixture;

        public Delete(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DeleteAsync_WhenContentTypeExists_ShouldReturnNoContent()
        {
            var id = Guid.NewGuid();
            var contentType = new { id, name = $"ContentType_{id:N}", baseType = ContentTypeBase.Page.ToString() };
            await CreateContentType(contentType);

            var response = await _fixture.Client.DeleteAsync(ContentTypesController.RoutePrefix + id);

            AssertResponse.NoContent(response);
        }

        [Fact]
        public async Task DeleteAsync_WhenContentTypeDoesNotExists_ShouldReturnNotFound()
        {
            var response = await _fixture.Client.DeleteAsync(ContentTypesController.RoutePrefix + Guid.NewGuid());

            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task DeleteAsync_WithSystemContentType_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.DeleteAsync(ContentTypesController.RoutePrefix + SystemContentTypes.RecycleBin);

            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task DeleteAsync_WhenContentTypeIsBeingUsed_ShouldReturnConflict()
        {
            var id = Guid.NewGuid();

            var contentType = new ContentType
            {
                GUID = id,
                Base = ContentTypeBase.Page,
                Name = $"ContentType_{id:N}"
            };

            await _fixture.WithContentType(contentType, async () =>
            {
                var page = _fixture.ContentRepository.GetDefault<PageData>(ContentReference.StartPage, contentType.ID);
                page.Name = $"ContentType_{id:N}";

                await _fixture.WithContent(page, async () =>
                {
                    var response = await _fixture.Client.DeleteAsync(ContentTypesController.RoutePrefix + id);
                    var error = await response.Content.ReadAs<ErrorResponse>();

                    Assert.Equal(HttpStatusCode.Conflict, error.StatusCode);
                    Assert.Equal(ProblemCode.InUse, error.Error.Code);
                });
            });
        }

        private async Task CreateContentType(object contentType)
        {
            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            response.EnsureSuccessStatusCode();
        }
    }
}
