using System;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;
using EPiServer.Validation;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    public class ContentTypeMapperTest
    {
        [Fact]
        public void MapToExternal_WithEditSettings_ShouldMapEditSettings()
        {
            var contentTypeBase = ContentTypeBase.Page;
            var contentTypeGuid = Guid.NewGuid();
            var editSettingsDisplayName = "Name to display";
            var editSettingsDescription = "The description";
            var editSettingsAvailable = true;
            var editSettingsOrder = 300;

            var source = new ContentType
            {
                GUID = contentTypeGuid,
                Base = contentTypeBase,
                DisplayName = editSettingsDisplayName,
                Description = editSettingsDescription,
                IsAvailable = editSettingsAvailable,
                SortOrder = editSettingsOrder
            };

            var target = new ExternalContentType();

            Subject().MapToExternal(source, target);

            Assert.Equal(editSettingsDisplayName, target.EditSettings.DisplayName);
            Assert.Equal(editSettingsDescription, target.EditSettings.Description);
            Assert.Equal(editSettingsAvailable, target.EditSettings.Available);
            Assert.Equal(editSettingsOrder, target.EditSettings.Order);
        }

        [Fact]
        public void MapToExternal_WhenEditSettingsHasDefaultValues_ShouldEditSettingsBeNull()
        {
            var contentTypeBase = ContentTypeBase.Page;
            var contentTypeGuid = Guid.NewGuid();
            string editSettingsDisplayName = default;
            string editSettingsDescription = default;
            bool editSettingsAvailable = default;
            int editSettingsOrder = default;

            var source = new ContentType
            {
                GUID = contentTypeGuid,
                Base = contentTypeBase,
                DisplayName = editSettingsDisplayName,
                Description = editSettingsDescription,
                IsAvailable = editSettingsAvailable,
                SortOrder = editSettingsOrder
            };

            var target = new ExternalContentType();

            Subject().MapToExternal(source, target);

            Assert.Null(target.EditSettings);
        }

        [Fact]
        public void MapToInternal_WithDifferentPropertyDefinitionNameCasing_ShouldMapNewName()
        {
            var expectedPropertyName = "test";
            var contentTypeBase = ContentTypeBase.Page;
            var contentTypeGuid = Guid.NewGuid();
            var contentTypeName = "Test";
            var propertyDefinitionId = 1;

            var target = new ContentType
            {
                GUID = contentTypeGuid,
                Name = contentTypeName,
                Base = contentTypeBase
            };

            target.PropertyDefinitions.Add(new PropertyDefinition
            {
                ID = propertyDefinitionId,
                Name = expectedPropertyName.ToUpperInvariant()
            });

            var source = new ExternalContentType
            {
                Id = contentTypeGuid,
                Name = contentTypeName,
                BaseType = contentTypeBase.ToString(),
                Properties = new[]
                {
                    new ExternalProperty { Name = expectedPropertyName }
                }
            };

            Subject().MapToInternal(source, target);

            Assert.Equal(expectedPropertyName, target.PropertyDefinitions.First().Name);
        }

        [Fact]
        public void MapToInternal_WithEditSettings_ShouldMapEditSettings()
        {
            var contentTypeBase = ContentTypeBase.Page;
            var contentTypeGuid = Guid.NewGuid();
            var editSettingsDisplayName = "Name to display";
            var editSettingsDescription = "The description";
            var editSettingsAvailable = true;
            var editSettingsOrder = 300;

            var source = new ExternalContentType
            {
                Id = contentTypeGuid,
                BaseType = contentTypeBase.ToString(),
                EditSettings = new ExternalContentTypeEditSettings(editSettingsDisplayName, editSettingsDescription, editSettingsAvailable, editSettingsOrder)
            };

            var target = new ContentType
            {
                GUID = contentTypeGuid,
                Base = contentTypeBase,
                DisplayName = editSettingsDisplayName,
                Description = editSettingsDescription,
                IsAvailable = editSettingsAvailable,
                SortOrder = editSettingsOrder
            };

            Subject().MapToInternal(source, target);

            Assert.Equal(editSettingsDisplayName, target.DisplayName);
            Assert.Equal(editSettingsDescription, target.Description);
            Assert.Equal(editSettingsAvailable, target.IsAvailable);
            Assert.Equal(editSettingsOrder, target.SortOrder);
        }

        [Fact]
        public void MapToInternal_WhenEditSettingsHasDefaultValues_ShouldMapEditSettings()
        {
            var contentTypeBase = ContentTypeBase.Page;
            var contentTypeGuid = Guid.NewGuid();
            string editSettingsDisplayName = default;
            string editSettingsDescription = default;
            bool editSettingsAvailable = default;
            int editSettingsOrder = default;

            var source = new ExternalContentType
            {
                Id = contentTypeGuid,
                BaseType = contentTypeBase.ToString(),
                EditSettings = null
            };

            var target = new ContentType
            {
                GUID = contentTypeGuid,
                Base = contentTypeBase,
                DisplayName = editSettingsDisplayName,
                Description = editSettingsDescription,
                IsAvailable = editSettingsAvailable,
                SortOrder = editSettingsOrder
            };

            Subject().MapToInternal(source, target);

            Assert.Equal(default, target.DisplayName);
            Assert.Equal(default, target.Description);
            Assert.Equal(default, target.IsAvailable);
            Assert.Equal(default, target.SortOrder);
        }


        private static ContentTypeMapper Subject(
            PropertyDataTypeResolver propertyDataTypeResolver = null)
          => new ContentTypeMapper(
            propertyDataTypeResolver ?? Mock.Of<PropertyDataTypeResolver>(),
            Mock.Of<IPropertyValidationSettingsRepository>(),
            Mock.Of<ITabDefinitionRepository>());
    }
}
