using System;
using System.Collections.Generic;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    public class ContentTypeValidatorTest
    {
        private ContentTypeValidator Subject(
            PropertyDataTypeResolver propertyTypeResolver = null,
            ITabDefinitionRepository tabDefinitionRepository = null)
        {
            return new ContentTypeValidator(
                propertyTypeResolver ?? Mock.Of<PropertyDataTypeResolver>(),
                tabDefinitionRepository ?? Mock.Of<ITabDefinitionRepository>());
        }

        [Fact]
        public void IsValid_WhenIsBlockWithoutItemType_ShouldNotBeValid()
        {
            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = ExternalPropertyDataType.Block(null) }
                }
            };
            Assert.Single(Subject().Validate(new[] { contentType }));
        }

        [Fact]
        public void IsValid_WhenNoPropertyDefinitionCanBeResolved_ShouldNotBeValid()
        {
            var repository = new Mock<PropertyDataTypeResolver>();
            repository.Setup(e => e.Resolve(It.IsAny<ExternalPropertyDataType>())).Returns((PropertyDefinitionType)null);

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = new ExternalPropertyDataType("UnknownType") }
                }
            };
            Assert.Single(Subject(repository.Object).Validate(new[] { contentType }));
        }

        [Fact]
        public void IsValid_WhenNoPropertyDefinitionResolverThrowsMultipleDefinitionMatchedException_ShouldNotBeValid()
        {
            var repository = new Mock<PropertyDataTypeResolver>();
            repository.Setup(e => e.Resolve(It.IsAny<ExternalPropertyDataType>()))
                .Callback(() => throw new MultipleDefinitionMatchedException())
                .Returns((PropertyDefinitionType)null);

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = new ExternalPropertyDataType("DuplicateName") }
                }
            };
            Assert.Single(Subject(repository.Object).Validate(new[] { contentType }));
        }

        [Fact]
        public void IsValid_WhenPropertyDefinitionCanBeResolved_ShouldBeValid()
        {
            var repository = new Mock<PropertyDataTypeResolver>();
            repository.Setup(e => e.Resolve(It.IsAny<ExternalPropertyDataType>()))
                .Returns(() => new PropertyDefinitionType());

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = new ExternalPropertyDataType("KnownType") }
                }
            };
            Assert.Empty(Subject(repository.Object).Validate(new[] { contentType }));
        }

        [Fact]
        public void Validate_WhenStaticBlockReferencesUnknownType_ShouldNotBeValid()
        {
            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = ExternalPropertyDataType.Block("UnknownType") }
                }
            };
            Assert.Single(Subject().Validate(new[] { contentType }));
        }

        [Fact]
        public void Validate_WhenContentTypeHasNullProperty_ShouldNotBeValid()
        {
            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new List<ExternalProperty> { null }
            };
            Assert.Single(Subject().Validate(new[] { contentType }));
        }

        [Fact]
        public void Validate_WhenStaticBlockReferencesExistingType_ShouldBeValid()
        {
            var repository = new Mock<PropertyDataTypeResolver>();
            repository.Setup(e => e.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(ExternalPropertyDataType.Block("ExistingType")))))
                .Returns(() => new PropertyDefinitionType());

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = ExternalPropertyDataType.Block("ExistingType") }
                }
            };
            Assert.Empty(Subject(repository.Object).Validate(new[] { contentType }));
        }

        [Fact]
        public void Validate_WhenStaticBlockReferencesBlockInSameBatch_ShouldBeValid()
        {
            var blockType = new ExternalContentType { BaseType = ContentTypeBase.Block.ToString(), Id = Guid.NewGuid(), Name = "BlockTypeInBatch" };

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = ExternalPropertyDataType.Block("BlockTypeInBatch") }
                }
            };
            Assert.Empty(Subject().Validate(new[] { blockType, contentType }));
        }

        [Fact]
        public void Validate_WhenAContentTypeHasMultiplePropertiesWithSameName_ShouldNotBeValid()
        {
            var repository = new Mock<PropertyDataTypeResolver>();
            repository.Setup(e => e.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType("PropertyNumber")))))
                .Returns(() => new PropertyDefinitionType());

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty { Name = "prop", DataType = new ExternalPropertyDataType("PropertyNumber") },
                    new ExternalProperty { Name = "Prop", DataType = new ExternalPropertyDataType("PropertyNumber") }
                }
            };
            Assert.Single(Subject(repository.Object).Validate(new[] { contentType }));
        }



        [Fact]
        public void Validate_WhenContentTypesHasDuplicateNamesWithNoIds_ShouldNotBeValid()
        {
            var contentTypes = new[]
            {
                new ExternalContentType { Name = "Name" },
                new ExternalContentType { Name = "name" },
            };

            Assert.Single(Subject(Mock.Of<PropertyDataTypeResolver>()).Validate(contentTypes));
        }


        [Fact]
        public void Validate_WhenContentTypesHasDuplicateNames_ShouldNotBeValid()
        {
            var id = Guid.NewGuid();
            var contentTypes = new[]
            {
                new ExternalContentType { Name="Name", Id = Guid.NewGuid() },
                new ExternalContentType { Name="name", Id = id },
            };

            Assert.Single(Subject(Mock.Of<PropertyDataTypeResolver>()).Validate(contentTypes));
        }

        [Fact]
        public void Validate_WhenContentTypesHasDuplicateIds_ShouldNotBeValid()
        {
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var contentTypes = new[]
            {
                new ExternalContentType { Name = "Name1", Id = id },
                new ExternalContentType { Name = "Name2", Id = id },
                new ExternalContentType { Name = "Name4", Id = id2 },
                new ExternalContentType { Name = "Name3", Id = id2 },
            };

            Assert.Single(Subject(Mock.Of<PropertyDataTypeResolver>()).Validate(contentTypes));
        }

        [Fact]
        public void Validate_WhenPropertyGroupCannotBeResolved_ShouldNotBeValid()
        {
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            var tabDefinitionRepository = new Mock<ITabDefinitionRepository>();
            propertyTypeResolver.Setup(e => e.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType("PropertyNumber")))))
                .Returns(() => new PropertyDefinitionType());
            tabDefinitionRepository.Setup(e => e.Load(It.IsAny<string>()))
                .Returns(() => null);

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty
                    {
                        Name = "prop",
                        DataType = new ExternalPropertyDataType("PropertyNumber"),
                        EditSettings = new ExternalPropertyEditSettings(VisibilityStatus.Default, null, "GroupName", 0, null, null)
                    }
                }
            };
            Assert.Single(Subject(propertyTypeResolver.Object, tabDefinitionRepository.Object).Validate(new[] { contentType }));
        }

        [Fact]
        public void Validate_WhenPropertyGroupCanBeResolved_ShouldBeValid()
        {
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            var tabDefinitionRepository = new Mock<ITabDefinitionRepository>();
            propertyTypeResolver.Setup(e => e.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType("PropertyNumber")))))
                .Returns(() => new PropertyDefinitionType());
            tabDefinitionRepository.Setup(e => e.Load(It.IsAny<string>()))
                .Returns(() => new TabDefinition());

            var contentType = new ExternalContentType
            {
                Name = "Name",
                Properties = new[]
                {
                    new ExternalProperty
                    {
                        Name = "prop",
                        DataType = new ExternalPropertyDataType("PropertyNumber"),
                        EditSettings = new ExternalPropertyEditSettings(VisibilityStatus.Default, null, "GroupName", 0, null, null)
                    }
                }
            };
            Assert.Empty(Subject(propertyTypeResolver.Object, tabDefinitionRepository.Object).Validate(new[] { contentType }));
        }
    }
}
