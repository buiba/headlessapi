using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.ServiceLocation;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed partial class CreateOrUpdate
    {
        private readonly ServiceFixture _fixture;
        private readonly IContentTypeRepository _contentTypeRepository;

        public CreateOrUpdate(ServiceFixture fixture)
        {
            _fixture = fixture;
            _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeAlreadyExists_ShouldReturnOk()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            await CreateContentType(contentType);

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeIsSystemType_ShouldReturnBadRequest()
        {
            var contentType = new { id = SystemContentTypes.ContentFolder, name = "LetsChangeTheContentFolder", baseType = ContentTypeBase.Folder.ToString() };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeAlreadyExists_ShouldUpdateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString(), name = $"ContentType_{Guid.NewGuid():N}" };

            await CreateContentType(contentType);

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.OK(response);

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeAlreadyExists_ShouldAddEditSettings()
        {
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString(), name = $"ContentType_{Guid.NewGuid():N}" };
            var updateEditSettings = new { displayName = "somename", description = "somedescription", available = true, order = 200 };
            var updateContentType = new { contentType.id, contentType.baseType, contentType.name, editSettings = updateEditSettings };

            await CreateContentType(contentType);

            var response = await CallCreateOrUpdateAsync(contentType.id, updateContentType);

            AssertResponse.OK(response);

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', 'editSettings': {{'displayName': '{updateEditSettings.displayName}', 'description': '{updateEditSettings.description}', 'available': {updateEditSettings.available.ToString().ToLower()}, 'order': {updateEditSettings.order} }}, properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeWithEditSettingsAlreadyExists_ShouldUpdateEditSettings()
        {
            var editSettings = new { displayName = "somename", description = "somedescription", available = true, order = 200 };
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString(), name = $"ContentType_{Guid.NewGuid():N}", editSettings };
            var updateEditSettings = new { displayName = "someothername", description = "someotherdescription", available = false, order = 300 };
            var updateContentType = new { contentType.id, contentType.baseType, contentType.name, editSettings = updateEditSettings };

            await CreateContentType(contentType);

            var response = await CallCreateOrUpdateAsync(contentType.id, updateContentType);

            AssertResponse.OK(response);

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', 'editSettings': {{'displayName': '{updateEditSettings.displayName}', 'description': '{updateEditSettings.description}', 'available': {updateEditSettings.available.ToString().ToLower()}, 'order': {updateEditSettings.order} }}, properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeWithEditSettingsIsUpdatedWithContentTypeWithoutEditSettings_ShouldRemoveEditSettings()
        {
            var editSettings = new { displayName = "somename", description = "somedescription", available = true, order = 200 };
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString(), name = $"ContentType_{Guid.NewGuid():N}", editSettings };
            var updateContentType = new { contentType.id, contentType.baseType, contentType.name };

            await CreateContentType(contentType);

            var response = await CallCreateOrUpdateAsync(contentType.id, updateContentType);

            AssertResponse.OK(response);

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WithNewContentTypeWithEditSettings_ShouldCreateContentTypeWithEditSettings()
        {
            var editSettings = new { displayName = "somename", description = "somedescription", available = true, order = 200 };
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString(), name = $"ContentType_{Guid.NewGuid():N}", editSettings };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.Created(response);

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', 'editSettings': {{'displayName': '{editSettings.displayName}', 'description': '{editSettings.description}', 'available': {editSettings.available.ToString().ToLower()}, 'order': {editSettings.order} }}, properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_ShouldTrimStringValues()
        {
            var editSettings = new { displayName = "  name  ", description = "  description  " };
            var propertyEditSettings = new { displayName = "  name  ", helpText = "  help  ", groupName = "  Information  ", hint = "  hint  " };
            var property = new { name = "name", dataType = "PropertyLongString", editSettings = propertyEditSettings };
            var contentType = new { id = Guid.NewGuid(), baseType = "page", name = $"ContentType_{Guid.NewGuid():N}", editSettings, properties = new List<object>(new[] { property }) };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.Created(response);

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ 'id': '{contentType.id}', 'name': '{contentType.name}', 'baseType': 'page', 'editSettings': {{ 'displayName': 'name', 'description': 'description', 'available': false, 'order': 0 }}, 'properties': [ {{ 'name': 'name', 'dataType': 'PropertyLongString', 'branchSpecific': false, 'editSettings': {{ 'visibility': 'default', 'displayName': 'name', 'groupName': 'Information', 'order': 0, 'helpText': 'help', 'hint': 'hint' }} }} ] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenAnotherContentTypeWithTheSameNameAlreadyExists_ShouldReturnConflict()
        {
            var existing = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            await CreateContentType(existing);

            var contentType = new { id = Guid.NewGuid(), existing.name, existing.baseType };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenChangingBaseOfExistingContentType_ShouldReturnConflict()
        {
            var existing = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString(), name = $"ContentType_{Guid.NewGuid():N}" };

            await CreateContentType(existing);

            var contentType = new { existing.id, baseType = ContentTypeBase.Video.ToString(), existing.name };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WithNewContentType_ShouldReturnCreatedWithLocation()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            Assert.Equal(contentType.id, AssertResponse.Created(ContentTypesController.RoutePrefix, response));
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WithNewContentType_ShouldCreateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString(), name = $"ContentType_{Guid.NewGuid():N}" };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            response.EnsureSuccessStatusCode();

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WithNewMediaType_ShouldCreateMediaContentType()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Media.ToString() };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            response.EnsureSuccessStatusCode();

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{contentType.baseType}', properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeIsMissingName_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Block.ToString() };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeIdDoesNotMatchLocation_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Block.ToString() };

            var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentType);

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeNameContainsInvalidCharacters_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), name = "invalid-#@", baseType = ContentTypeBase.Block.ToString() };

            var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentType);

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenContentTypeNameIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), name = new string('S', 51), baseType = ContentTypeBase.Block.ToString() };

            var response = await CallCreateOrUpdateAsync(Guid.NewGuid(), contentType);

            AssertResponse.ValidationError(response);
        }

        private Task<HttpResponseMessage> CallCreateOrUpdateAsync(Guid id, object contentType) => _fixture.Client.PutAsync(ContentTypesController.RoutePrefix + id, new JsonContent(contentType));

        private async Task CreateContentType(object contentType)
        {
            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            response.EnsureSuccessStatusCode();
        }

        private async Task<HttpResponseMessage> GetContentType(Guid id)
        {
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + id);
            response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
