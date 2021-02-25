using System;
using System.Collections.Generic;
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
    public sealed partial class Create
    {
        private readonly ServiceFixture _fixture;
        private readonly IContentTypeRepository _contentTypeRepository;

        public Create(ServiceFixture fixture)
        {
            _fixture = fixture;
            _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
        }

        [Fact]
        public async Task CreateAsync_WhenContentTypeWithTheSameIdAlreadyExists_ShouldReturnConflict()
        {
            var contentType = new { id = SystemContentTypes.RootPage, name = "MyPage", baseType = ContentTypeBase.Page.ToString() };
            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task CreateAsync_WithNewContentType_ShouldReturnCreatedWithLocation()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };
            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            var id = AssertResponse.Created(ContentTypesController.RoutePrefix, response);

            Assert.Equal(contentType.id, id);
        }

        [Fact]
        public async Task CreateAsync_WithNewContentType_ShouldCreateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task CreateAsync_WithNewContentTypeID_WhenAnotherContentTypeWithTheSameNameAlreadyExists_ShouldReturnConflictError()
        {
            var existing = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(existing));
            response.EnsureSuccessStatusCode();

            var contentType = new { id = Guid.NewGuid(), existing.name, existing.baseType };
            response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task CreateAsync_WithoutContentTypeID_WhenAnotherContentTypeWithTheSameNameAlreadyExists_ShouldReturnConflictError()
        {
            var existing = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(existing));
            response.EnsureSuccessStatusCode();

            var contentType = new { existing.name, existing.baseType };
            response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.Conflict(response);
        }


        [Fact]
        public async Task CreateAsync_WhenContentTypeIsMissingName_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), baseType = ContentTypeBase.Page.ToString() };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateAsync_WhenMissingBase_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}" };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateAsync_WhenUnknownBase_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = "Rocket" };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateAsync_WhenContentTypeNameContainsInvalidCharacters_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), name = "invalid-#@" };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateAsync_WhenContentTypeNameIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new { id = Guid.NewGuid(), name = new string('S', 51) };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateAsync_WithNewContentTypeWithEditSettings_ShouldCreateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString(), editSettings = new { displayName = "somename" } };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task CreateAsync_WhenEditSettingsDisplayNameIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                editSettings = new { displayName = new string('S', 51) }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateAsync_WhenEditSettingsDescriptionIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                editSettings = new { description = new string('S', 256) }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task CreateAsync_ShouldTrimStringValues()
        {
            var editSettings = new { displayName = "  name  ", description = "  description  " };
            var propertyEditSettings = new { displayName = "  name  ", helpText = "  help  ", groupName = "  Information  ", hint = "  hint  " };
            var property = new { name = "name", dataType = "PropertyLongString", editSettings = propertyEditSettings };
            var contentType = new { id = Guid.NewGuid(), baseType = "page", name = $"ContentType_{Guid.NewGuid():N}", editSettings, properties = new List<object>(new[] { property }) };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ 'id': '{contentType.id}', 'name': '{contentType.name}', 'baseType': 'page', 'editSettings': {{ 'displayName': 'name', 'description': 'description', 'available': false, 'order': 0 }}, 'properties': [ {{ 'name': 'name', 'dataType': 'PropertyLongString', 'branchSpecific': false, 'editSettings': {{ 'visibility': 'default', 'displayName': 'name', 'groupName': 'Information', 'order': 0, 'helpText': 'help', 'hint': 'hint' }} }} ] }}");
        }

        [Fact]
        public async Task CreateAsync_WhenEditSettingsIsIncluded_ShouldCreateContentTypeWithEditSettings()
        {
            var editSettings = new { displayName = "somename", description = "somedescription", available = true, order = 200 };
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                editSettings
            };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', editSettings: {{ displayName : '{editSettings.displayName}', order: {editSettings.order}, description: '{editSettings.description}', available: {editSettings.available.ToString().ToLower()} }}, properties:[] }}");
        }
    }
}
