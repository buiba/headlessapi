using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    public sealed partial class CreateOrUpdate //.Properties
    {
        [Theory]
        [InlineData(VersionComponent.Major)]
        [InlineData(VersionComponent.Minor)]
        [InlineData(null)]
        public async Task CreateOrUpdateAsync_WhenPropertiesIsChangedFromJson_ShouldUpdatePropertiesOnContentTypeAccordingToAllowedUpgrades(VersionComponent? allowedUpgrades)
        {
            var branchSpecificValue = true;

            // Since CmsCore orders propery without FieldOrder at the end, we should add order = 0 here to maintain the expected order of properties
            var property1 = new { name = "SomeProperty", dataType = nameof(PropertyString), branchSpecific = branchSpecificValue, editSettings = new { displayName = "Property", order = 0 } };
            var property2 = new { name = "OtherProperty", dataType = nameof(PropertyNumber), branchSpecific = branchSpecificValue, editSettings = new { displayName = "Property", order = 0 } };

            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new List<object>(new[] { property1, property2 })
            };

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
            var content = await response.Content.ReadAsStringAsync();

            var editSettingsString = "{\"visibility\":\"default\",\"displayName\":\"Property\",\"groupName\":\"Information\",\"order\":0}";
            var updatedEditSettingsString = "{\"visibility\":\"default\",\"displayName\":\"Title\",\"groupName\":\"Information\",\"order\":100}";

            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[{{'name': '{property1.name}', 'dataType': '{property1.dataType}','branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {editSettingsString} }}, {{'name': '{property2.name}', 'dataType': '{property2.dataType}','branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {editSettingsString} }}] }}");

            var property3 = new { name = "AddedProperty", dataType = nameof(PropertyBoolean), branchSpecific = branchSpecificValue, editSettings = new ExternalPropertyEditSettings(VisibilityStatus.Default, "Title", null, 100, null, null) };
            contentType.properties.Remove(property2);
            contentType.properties.Add(property3);
            var uri = allowedUpgrades.HasValue ? $"{ContentTypesController.RoutePrefix}{contentType.id}?allowedUpgrades={allowedUpgrades.Value}" : $"{ContentTypesController.RoutePrefix}{contentType.id}";
            response = await _fixture.Client.PutAsync(uri, new JsonContent(contentType));

            if (allowedUpgrades.GetValueOrDefault() == VersionComponent.Major)
            {
                response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                content = await response.Content.ReadAsStringAsync();
                content.Should().BeValidJson()
                    .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{ContentTypeBase.Page}', properties:[{{'name': '{property1.name}', 'dataType': '{property1.dataType}','branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {editSettingsString} }}, {{'name': '{property3.name}', 'dataType': '{property3.dataType}','branchSpecific': {branchSpecificValue.ToString().ToLower()},'editSettings': {updatedEditSettingsString} }}] }}");
            }
            else
            {
                AssertResponse.Conflict(response);
            }
        }

        [Theory]
        [InlineData(VersionComponent.Major)]
        [InlineData(VersionComponent.Minor)]
        [InlineData(null)]
        public async Task CreateOrUpdateAsync_WhenPropertyTypeIsChanged_ShouldUpdatePropertyAccordingToAllowedUpgrades(VersionComponent? allowedUpgrades)
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new [] { new { name = "SomeProperty", dataType = nameof(PropertyString) } }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
                response.EnsureSuccessStatusCode();

                // Change property to new type
                contentType.properties[0] = new { name = "SomeProperty", dataType = nameof(PropertyNumber) };
                var uri = allowedUpgrades.HasValue ? $"{ContentTypesController.RoutePrefix}{contentType.id}?allowedUpgrades={allowedUpgrades.Value}" : $"{ContentTypesController.RoutePrefix}{contentType.id}";
                response = await _fixture.Client.PutAsync(uri, new JsonContent(contentType));

                if (allowedUpgrades.GetValueOrDefault() == VersionComponent.Major)
                {
                    AssertResponse.OK(response);

                    var cmsContentType = _contentTypeRepository.Load(contentType.id);

                    Assert.Equal(typeof(PropertyNumber), Assert.Single(cmsContentType.PropertyDefinitions).Type.DefinitionType);
                }
                else
                {
                    AssertResponse.Conflict(response);
                }
            }
        }

        [Theory]
        [InlineData(VersionComponent.Major)]
        [InlineData(VersionComponent.Minor)]
        [InlineData(null)]
        public async Task CreateOrUpdateAsync_WhenPropertyTypeIsChangedWhenContentExist_ShouldUpdatePropertyAccordingToAllowedUpgrades(VersionComponent? allowedUpgrades)
        {
            var contentLink = ContentReference.EmptyReference;
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new [] { new { name = "SomeProperty", dataType = nameof(PropertyString) } }
            };

            var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
            response.EnsureSuccessStatusCode();

            try
            {
                // Create content that uses content type
                var cmsContentType = _contentTypeRepository.Load(contentType.id);
                var page = contentRepository.GetDefault<PageData>(ContentReference.RootPage, cmsContentType.ID, CultureInfo.GetCultureInfo("en"));
                page.Name = "TestContent " + Guid.NewGuid();
                page.Property["SomeProperty"].Value = "Property Value";
                contentLink = contentRepository.Publish(page, AccessLevel.NoAccess);

                // Change property to new type
                contentType.properties[0] = new { name = "SomeProperty", dataType = nameof(PropertyNumber) };
                var uri = allowedUpgrades.HasValue ? $"{ContentTypesController.RoutePrefix}{contentType.id}?allowedUpgrades={allowedUpgrades.Value}" : $"{ContentTypesController.RoutePrefix}{contentType.id}";
                response = await _fixture.Client.PutAsync(uri, new JsonContent(contentType));

                if (allowedUpgrades.GetValueOrDefault() == VersionComponent.Major)
                {
                    AssertResponse.OK(response);

                    cmsContentType = _contentTypeRepository.Load(contentType.id);

                    Assert.Equal(typeof(PropertyNumber), Assert.Single(cmsContentType.PropertyDefinitions).Type.DefinitionType);
                }
                else
                {
                    AssertResponse.Conflict(response);
                }
            }
            finally
            {
                // Cleanup
                if (!ContentReference.IsNullOrEmpty(contentLink))
                {
                    contentRepository.Delete(contentLink, true, AccessLevel.NoAccess);
                }
                _contentTypeRepository.Delete(_contentTypeRepository.Load(contentType.id).ID);
            }
        }

        [Theory]
        [MemberData(nameof(ExpectedPropertySerializations))]
        public async Task CreateAndGetProperties_ShouldFormatPropertiesFlattened(string dataType, string itemType, string expectedJsonFormat)
        {
            var blockType = new
            {
                id = Guid.NewGuid(),
                name = $"ABlockType",
                baseType = ContentTypeBase.Block.ToString()
            };
            var branchSpecificValue = true;
            var property = string.IsNullOrEmpty(itemType) ? (object)new { name = "AProperty", dataType, branchSpecific = branchSpecificValue } : new { name = "AProperty", dataType, itemType, branchSpecific = branchSpecificValue };
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new[] { property }
            };

            var scope = _fixture.WithContentTypeIds(contentType.id, blockType.id);

            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(blockType));
            await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
            
            Assert.Contains(expectedJsonFormat, await response.Content.ReadAsStringAsync());

            scope.Dispose();
        }

        public static TheoryData ExpectedPropertySerializations => new TheoryData<string, string, string>
        {
            { "PropertyString", null, "{\"name\":\"AProperty\",\"dataType\":\"PropertyString\",\"branchSpecific\":true,\"editSettings\":{\"visibility\":\"default\",\"groupName\":\"Information\",\"order\":0}" },
            { "PropertyBlock", "ABlockType", "{\"name\":\"AProperty\",\"dataType\":\"PropertyBlock\",\"itemType\":\"ABlockType\",\"branchSpecific\":true,\"editSettings\":{\"visibility\":\"default\",\"groupName\":\"Information\",\"order\":0}" },
        };
    }
}
