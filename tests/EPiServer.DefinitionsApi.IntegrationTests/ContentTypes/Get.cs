using System;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class Get
    {
        private readonly ServiceFixture _fixture;

        public Get(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAsync_WhenContentTypeDoesNotExists_ShouldReturnNotFound()
        {
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + Guid.NewGuid());

            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task GetAsync_WhenContentTypeExists_ShouldReturnContentType()
        {
            var id = SystemContentTypes.RootPage;

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + id);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{id}', name: 'SysRoot', baseType: '{ContentTypeBase.Page}', 'editSettings': {{'description': 'Used as root/welcome page', 'available': false, 'order': 10000 }}, properties:[] }}");
        }

        [Fact]
        public async Task GetAsync_WhenContentTypeWithEditSettingsExists_ShouldReturnContentTypeWithEditSettings()
        {
            var editSettings = new { displayName = "somename", description = "somedescription", available = true, order = 200 };
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString(), editSettings };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', 'editSettings': {{'displayName': '{editSettings.displayName}', 'description': '{editSettings.description}', 'available': {editSettings.available.ToString().ToLower()}, 'order': {editSettings.order} }}, properties:[] }}");
        }

        [Fact]
        public async Task GetAsync_WhenContentTypeWithoutEditSettingsExists_ShouldReturnContentTypeWithoutEditSettings()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[] }}");
        }
    }
}
