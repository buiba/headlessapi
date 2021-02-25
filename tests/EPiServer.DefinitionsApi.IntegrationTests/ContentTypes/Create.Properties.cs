using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.IntegrationTests;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.PlugIn;
using EPiServer.SpecializedProperties;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    public sealed partial class Create
    {
        //<type, itemType, expectedProperty>
        public static TheoryData ExpectedProperties => new TheoryData<string, Type>
        {
            { nameof(PropertyBoolean), typeof(PropertyBoolean)},
            { nameof(PropertyNumber), typeof(PropertyNumber) },
            { nameof(PropertyFloatNumber), typeof(PropertyFloatNumber) },
            { nameof(PropertyDate), typeof(PropertyDate) },
            { nameof(PropertyContentReference), typeof(PropertyContentReference) },
            { nameof(PropertyLongString), typeof(PropertyLongString) },
            { nameof(PropertyXhtmlString), typeof(PropertyXhtmlString) },
            { nameof(PropertyContentReferenceList), typeof(PropertyContentReferenceList) },
        };

        private const string MetadataTabName = "Metadata";
        private readonly string _editSettingsWithValue = $"{{\"visibility\":\"default\",\"displayName\":\"MetaTitle\",\"groupName\":\"{MetadataTabName}\",\"order\":100}}";
        private readonly string _editSettings = "{\"visibility\":\"default\",\"groupName\":\"Information\",\"order\":0}";
        private readonly string _editSettingsWithHintUrl = $"{{\"visibility\":\"default\",\"displayName\":\"MetaTitle\",\"groupName\":\"{MetadataTabName}\",\"order\":100,\"hint\":\"Url\"}}";

        [Theory]
        [MemberData(nameof(ExpectedProperties))]
        public async Task CreateAsync_WhenKnownDataType_ShouldCreateProperty(string dataType, Type expectedPropertyType)
        {
            var propertyName = "SomeProperty";
            var branchSpecificValue = true;
            var editSettingsValue = new ExternalPropertyEditSettings(VisibilityStatus.Default, "MetaTitle", MetadataTabName, 100, null, null);
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new [] { new { name = propertyName, dataType, branchSpecific = branchSpecificValue, editSettings = editSettingsValue } }
            };
            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await _fixture.WithTab(new TabDefinition(-1, MetadataTabName), async () =>
                {
                    await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
                    var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                    var content = await response.Content.ReadAsStringAsync();
                    var expectedEditSettings = dataType == "Url" ? _editSettingsWithHintUrl : _editSettingsWithValue;
                    content.Should().BeValidJson()
                        .Which.Should().BeEquivalentTo(
                        $"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[{{'name': '{propertyName}','dataType': '{dataType}','branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {expectedEditSettings} }}] }}");

                    var cmsContentType = _contentTypeRepository.Load(contentType.id);
                    Assert.Equal(expectedPropertyType, cmsContentType.PropertyDefinitions.Single().Type.DefinitionType);
                });
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyIsForExistingBlockType_ShouldCreateBlockProperty()
        {
            var blockType = new BlockType { GUID = Guid.NewGuid(), Name = "SomeBlockType", Base = ContentTypeBase.Block, };
            _contentTypeRepository.Save(blockType);
            var editSettingsValue = new ExternalPropertyEditSettings(VisibilityStatus.Default, "MetaTitle", MetadataTabName, 100, null, null);
            var propertyName = "BlockProperty";
            var branchSpecificValue = true;
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new [] { new { name = propertyName, dataType = nameof(PropertyBlock), itemType = blockType.Name, branchSpecific = branchSpecificValue, editSettings = editSettingsValue } }
            };
            using (_fixture.WithContentTypeIds(contentType.id, blockType.GUID))
            {
                await _fixture.WithTab(new TabDefinition(-1, MetadataTabName), async () =>
                {
                    await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
                    var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                    var content = await response.Content.ReadAsStringAsync();
                    content.Should().BeValidJson()
                        .Which.Should().BeEquivalentTo(
                        $"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[{{'name': 'BlockProperty','dataType': '{nameof(PropertyBlock)}','itemType': '{blockType.Name}','branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {_editSettingsWithValue} }}] }}");

                    var cmsContentType = _contentTypeRepository.Load(contentType.id);
                    Assert.Equal(typeof(PropertyBlock<BlockData>), cmsContentType.PropertyDefinitions.Single().Type.DefinitionType);
                });
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyIsARegisteredCustomProperty_ShouldCreateCustomProperty()
        {
            var branchSpecificValue = true;
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { new { name = "UsingCustomProperty", dataType = nameof(CustomTestProperty), branchSpecific = branchSpecificValue } }
            };
            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
                var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().BeValidJson()
                    .Which.Should().BeEquivalentTo(
                    $"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[{{'name': 'UsingCustomProperty','dataType': '{nameof(CustomTestProperty)}','branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {_editSettings} }}] }}");

                var cmsContentType = _contentTypeRepository.Load(contentType.id);
                Assert.Equal(typeof(CustomTestProperty), cmsContentType.PropertyDefinitions.Single().Type.DefinitionType);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyMissingName_ShouldReturnValidationError()
        {
            var id = Guid.NewGuid();

            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{id:N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { dataType = nameof(PropertyString) } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal($"Property of 'ContentType_{id:N}' cannot be null",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyNameIsInvalid_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "11_Property", dataType = nameof(PropertyString) } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal("The field Name must match the regular expression '[a-zA-Z][\\w]*'.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyNameIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = new string('S', 101), dataType = nameof(PropertyString) } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal($"The field Name must be a string or array type with a maximum length of '100'.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyMissingDataType_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeProperty" } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal("The Type field is required.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyHasUnknownDataType_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeProperty", dataType = "Rocket" } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal("No registered property could be resolved from property data type with DataType 'Rocket'",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyGroupDoesNotExist_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeName", dataType = nameof(PropertyString), editSettings = new { groupName = "Test" } } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal("The group Test is not valid.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyGroupIsNotValid_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeName", dataType = nameof(PropertyString), editSettings = new { groupName = "123ABC" } } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal("The field GroupName must match the regular expression '[a-zA-Z][\\w]*'.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyGroupNameIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeName", dataType = nameof(PropertyString), editSettings = new { groupName = new string('S', 101) } } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal($"The field GroupName must be a string or array type with a maximum length of '100'.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenEditDisplayNameIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeName", dataType = nameof(PropertyString), editSettings = new { displayName = new string('S', 2001) } } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal($"The field DisplayName must be a string or array type with a maximum length of '255'.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenEditHelpTextIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeName", dataType = nameof(PropertyString), editSettings = new { helpText = new string('S', 2001) } } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal($"The field HelpText must be a string or array type with a maximum length of '2000'.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenEditHintIsTooLong_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeName", dataType = nameof(PropertyString), editSettings = new { hint = new string('S', 256) } } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal($"The field Hint must be a string or array type with a maximum length of '255'.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyOrderIsOutOfRange_ShouldReturnValidationError()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { new { name = "SomeName", dataType = nameof(PropertyString), editSettings = new { order = -1 } } } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var error = await response.Content.ReadAs<ErrorResponse>();

            AssertResponse.ValidationError(response);
            Assert.Equal($"The field Order must be between 0 and {int.MaxValue}.",  error.Error.GetFirstValidationErrorMessage());
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyIsKnownType_ShouldCreateContentTypeWithProperty()
        {
            var branchSpecificValue = true;
            var property = new { name = "SomeProperty", dataType = nameof(PropertyString), branchSpecific = branchSpecificValue };
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object> { { property } }
            };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[{{'name': '{property.name}', 'dataType': '{property.dataType}', 'branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {_editSettings} }}] }}");
        }
    }

    [PropertyDefinitionTypePlugIn]
    public class CustomTestProperty : PropertyString { }
}
