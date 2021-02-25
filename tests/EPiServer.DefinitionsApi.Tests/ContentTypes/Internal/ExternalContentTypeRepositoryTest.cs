using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.DataAbstraction.RuntimeModel.Internal;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;
using EPiServer.Validation;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    public class ExternalContentTypeRepositoryTest
    {
        private static class SystemContentTypes
        {
            public static readonly ContentType RootPage = new ContentType { GUID = new Guid("3fa7d9e7-877b-11d3-827c-00a024cacfcb"), Name = "SysRootPage" };
            public static readonly ContentType RecycleBin = new ContentType { GUID = new Guid("4eea90cd-4210-4115-a399-6d6915554e10"), Name = "SysRecycleBin" };
            public static readonly ContentType ContentFolder = new ContentType { GUID = new Guid("52f8d1e9-6d87-4db6-a465-41890289fb78"), Name = "SysContentFolder" };
            public static readonly ContentType ContentAssetFolder = new ContentType { GUID = new Guid("e9ab78a3-1bbf-48ef-a8d4-1c1f98e80d91"), Name = "SysContentAssetFolder" };

            public static readonly ContentType[] All = new[] { RootPage, RecycleBin, ContentFolder, ContentAssetFolder };
        }

        [Fact]
        public void List_ShouldReturnTypesFromRepository()
        {
            var existing = new[]
            {
                new ContentType { GUID = Guid.NewGuid(), Name = "One" },
                new ContentType { GUID = Guid.NewGuid(), Name = "Two" },
            };
            var subject = Subject(existing);

            var result = subject.List();

            Assert.Equal(existing.Select(x => x.Name), result.ContentTypes.Select(x => x.Name));
        }

        [Fact]
        public void List_WhenRepositoryContainsMoreThanTopParameter_ShouldReturnTopTypesFromRepositoryWithContinuationToken()
        {
            var total = 7;
            var take = 5;
            var existing = Enumerable.Range(0, total)
                .Select(i => new ContentType { GUID = Guid.NewGuid(), Name = "Content-" + i })
                .ToArray();

            var subject = Subject(existing);

            var result = subject.List(take);

            Assert.Equal(existing.Select(x => x.Name).Take(take), result.ContentTypes.Select(x => x.Name));
            Assert.Equal(new ContinuationToken(take, take), result.ContinuationToken);
        }

        [Fact]
        public void List_WhenRepositoryContainsLessThanTopParameter_ShouldReturnTopTypesFromRepositoryWithoutContinuationToken()
        {
            var totalItems = 3;
            var requestedItems = 5;
            var existing = Enumerable.Range(0, totalItems)
                .Select(i => new ContentType { GUID = Guid.NewGuid(), Name = "Content-" + i })
                .ToArray();

            var subject = Subject(existing);

            var result = subject.List(requestedItems);

            Assert.Equal(existing.Select(x => x.Name), result.ContentTypes.Select(x => x.Name));
            Assert.Equal(ContinuationToken.None, result.ContinuationToken);
        }

        [Fact]
        public void List_WithContinuationToken_WhenRepositoryContainsMoreThanTopParameter_ShouldReturnTopTypesFromRepositoryWithContinuationToken()
        {
            var total = 20;
            var take = 5;
            var skip = 10;
            var existing = Enumerable.Range(0, total)
                .Select(i => new ContentType { GUID = Guid.NewGuid(), Name = "Content-" + i })
                .ToArray();

            var subject = Subject(existing);

            var result = subject.List(new ContinuationToken(skip, take));

            Assert.Equal(existing.Select(x => x.Name).Skip(skip).Take(take), result.ContentTypes.Select(x => x.Name));
            Assert.Equal(new ContinuationToken(skip + take, take), result.ContinuationToken);
        }

        [Fact]
        public void List_WithContinuationToken_WhenRepositoryReturnsLastPage_ShouldReturnTopTypesFromRepositoryWithoutContinuationToken()
        {
            var total = 12;
            var take = 5;
            var skip = 10;
            var existing = Enumerable.Range(0, total)
                .Select(i => new ContentType { GUID = Guid.NewGuid(), Name = "Content-" + i })
                .ToArray();

            var subject = Subject(existing);

            var result = subject.List(new ContinuationToken(skip, take));

            Assert.Equal(existing.Select(x => x.Name).Skip(skip).Take(take), result.ContentTypes.Select(x => x.Name));
            Assert.Equal(ContinuationToken.None, result.ContinuationToken);
        }

        [Fact]
        public void List_ShouldFilterOutRootPage()
        {
            var subject = Subject(SystemContentTypes.All);

            var result = subject.List();

            Assert.DoesNotContain(result, x => x.Id == SystemContentTypes.RootPage.GUID);
        }

        [Fact]
        public void List_ShouldFilterOutRecycleBin()
        {
            var subject = Subject(SystemContentTypes.All);

            var result = subject.List();

            Assert.DoesNotContain(result, x => x.Id == SystemContentTypes.RecycleBin.GUID);
        }

        [Fact]
        public void List_ShouldReturnContentAssetFolder()
        {
            var subject = Subject(SystemContentTypes.All);

            var result = subject.List();

            Assert.Contains(result, x => x.Id == SystemContentTypes.ContentAssetFolder.GUID);
        }

        [Fact]
        public void List_ShouldReturnContentFolder()
        {
            var subject = Subject(SystemContentTypes.All);

            var result = subject.List();

            Assert.Contains(result, x => x.Id == SystemContentTypes.ContentFolder.GUID);
        }

        [Fact]
        public void Get_WhenMatchingContentTypeExists_ShouldReturnContentType()
        {
            var existing = new ContentType { GUID = Guid.NewGuid(), Name = "One" };

            var subject = Subject(existing);

            var result = subject.Get(existing.GUID);

            Assert.NotNull(result);
            Assert.Equal(existing.GUID, result.Id);
            Assert.Equal(existing.Name, result.Name);
        }

        [Fact]
        public void Get_WhenNoMatchingContentTypeExists_ShouldReturnNull()
        {
            var subject = Subject(new ContentType { GUID = Guid.NewGuid(), Name = "One" });

            var result = subject.Get(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public void Save_WithNewContentType_ShouldCallSaveOnInnerRepository()
        {
            ContentType saved = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saved = c.Single());

            var subject = Subject(inner.Object);

            var contentType = new ExternalContentType { Id = Guid.NewGuid(), Name = "One", BaseType = ContentTypeBase.Page.ToString() };

            subject.Save(new[] { contentType }, out var createdContentTypes);

            Assert.NotNull(saved);
            Assert.Equal(contentType.Id, saved.GUID);
            Assert.Equal(contentType.Name, saved.Name);
            Assert.Contains(contentType.Id, createdContentTypes);
        }

        [Fact]
        public void Save_WithMultipleNewContentTypes_ShouldReturnAllContentTypes()
        {
            var subject = Subject();

            var contentType1 = new ExternalContentType { Id = Guid.NewGuid(), Name = "PageType1", BaseType = ContentTypeBase.Page.ToString() };
            var contentType2 = new ExternalContentType { Id = Guid.NewGuid(), Name = "PageType2", BaseType = ContentTypeBase.Page.ToString() };

            subject.Save(new[] { contentType1, contentType2 }, out var createdContentTypes);

            Assert.Contains(contentType1.Id, createdContentTypes);
            Assert.Contains(contentType2.Id, createdContentTypes);
        }

        [Fact]
        public void Save_WithNewAndExistingContentType_ShouldReturnNewContentType()
        {
            var existingContentType = new ExternalContentType { Id = Guid.NewGuid(), Name = "PageType1", BaseType = ContentTypeBase.Page.ToString() };
            var newContentType = new ExternalContentType { Id = Guid.NewGuid(), Name = "PageType2", BaseType = ContentTypeBase.Page.ToString() };

            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(It.Is<Guid>(x => x == existingContentType.Id))).Returns(new ContentType());

            var subject = Subject(inner.Object);

            subject.Save(new[] { existingContentType, newContentType }, out var createdContentTypes);

            Assert.DoesNotContain(existingContentType.Id, createdContentTypes);
            Assert.Contains(newContentType.Id, createdContentTypes);
        }

        [Fact]
        public void Save_WhenMatchingContentTypeExists_ShouldUpdateExistingContentType()
        {
            var existing = new ContentType { ID = 22, GUID = Guid.NewGuid(), Name = "One", Description = "Some description" };
            existing.MakeReadOnly();
            ContentType saved = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(existing.GUID)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saved = c.Single());

            var subject = Subject(inner.Object);

            var contentType = new ExternalContentType { Id = existing.GUID, Name = "Two", BaseType = ContentTypeBase.Page.ToString(), EditSettings = new ExternalContentTypeEditSettings(null, "Some description", true, 100) };

            subject.Save(new[] { contentType }, out var createdContentTypes);

            Assert.NotNull(saved);
            Assert.Equal(existing.ID, saved.ID);
            Assert.Equal(contentType.Id, saved.GUID);
            Assert.Equal(contentType.Name, saved.Name);
            Assert.Equal(contentType.EditSettings.DisplayName, saved.DisplayName);
            Assert.Equal(contentType.EditSettings.Description, saved.Description);
            Assert.Equal(contentType.EditSettings.Available, saved.IsAvailable);
            Assert.Equal(contentType.EditSettings.Order, saved.SortOrder);
            Assert.DoesNotContain(contentType.Id, createdContentTypes);
        }

        [Fact]
        public void Save_WhenConflictingContentTypeExists_ShouldThrow()
        {
            var innerException = new ConflictingResourceException("There was a conflict");
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Throws(innerException);

            var subject = Subject(inner.Object);

            var contentType = new ExternalContentType { Id = Guid.NewGuid(), Name = "ConflictingType", BaseType = ContentTypeBase.Page.ToString() };

            var exception = Assert.Throws<ErrorException>(() => subject.Save(new[] { contentType }, out _));
            Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
            Assert.Equal(innerException.Message, exception.ErrorResponse.Error.Message);
        }

        [Fact]
        public void Save_WhenInnerRepositoryThrowsInvalidContentTypeBase_ShouldThrowErrorException()
        {
            var contentType = new ExternalContentType { Id = Guid.NewGuid(), Name = "ConflictingType", BaseType = ContentTypeBase.Page.ToString() };

            var innerException = new InvalidContentTypeBaseException("The content type base is no good.");
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Save(It.Is<IEnumerable<ContentType>>(x => x.Any(c => c.GUID == contentType.Id)), It.IsAny<ContentTypeSaveOptions>())).Throws(innerException);

            var subject = Subject(inner.Object);

            var exception = Assert.Throws<ErrorException>(() => subject.Save(new[] { contentType }, out _));
            Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
            Assert.Equal(innerException.Message, exception.ErrorResponse.Error.Message);
        }

        [Fact]
        public void Save_WhenInnerRepositoryThrowsVersionValidationException_ShouldThrowErrorException()
        {
            var contentType = new ExternalContentType { Id = Guid.NewGuid(), Name = "SomeType", BaseType = ContentTypeBase.Page.ToString() };

            var innerException = new VersionValidationException("You can't update this content type with that kind of version.");
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Save(It.Is<IEnumerable<ContentType>>(x => x.Any(c => c.GUID == contentType.Id)), It.IsAny<ContentTypeSaveOptions>())).Throws(innerException);

            var subject = Subject(inner.Object);

            var exception = Assert.Throws<ErrorException>(() => subject.Save(new[] { contentType }, out _));
            Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
            Assert.Equal(innerException.Message, exception.ErrorResponse.Error.Message);
            Assert.Equal(ProblemCode.Version, exception.ErrorResponse.Error.Code);
        }

        public static readonly TheoryData ReadOnlySystemTypes = new TheoryData<ContentType>
        {
            SystemContentTypes.RootPage,
            SystemContentTypes.RecycleBin,
            SystemContentTypes.ContentAssetFolder,
            SystemContentTypes.ContentFolder
        };

        [Theory]
        [MemberData(nameof(ReadOnlySystemTypes))]
        public void Save_WhenContentTypeIsRestrictedSystemType_ShouldThrow(ContentType contentType)
        {
            var inner = new Mock<ContentTypeRepository>();

            var subject = Subject(inner.Object);

            var external = new ExternalContentType { Id = contentType.GUID, Name = contentType.Name };

            var ex = Assert.Throws<ErrorException>(() => subject.Save(new[] { external }, out _));
            Assert.Equal(ProblemCode.SystemType, ex.ErrorResponse.Error.Code);

            inner.Verify(x => x.Save(It.IsAny<ContentType>()), Times.Never());
        }

        [Fact]
        public void Save_IfNewContentType_ShouldIncludePropertiesOnContentType()
        {
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            ContentType saved = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saved = c.Single());

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                    new ExternalContentType
                    {
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString(),
                        Properties = new[]
                        {
                            externalProperty
                        }
                    }
                },
                out _);

            Assert.Contains(saved.PropertyDefinitions, p => string.Equals(p.Name, externalProperty.Name) && p.Type.DataType == PropertyDataType.String);
        }

        [Fact]
        public void Save_IfDataTypeHasChangedOnExistingProperty_ShouldUpdateTypeOnDefinition()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId };
            existing.PropertyDefinitions.Add(new PropertyDefinition { Name = externalProperty.Name, Type = new PropertyDefinitionType { DataType = PropertyDataType.Number } });
            ContentType saved = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saved = c.Single());

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                    new ExternalContentType
                    {
                        Id = contentTypeId,
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString(),
                        Properties = new[]
                        {
                            externalProperty
                        }
                    }
                },
                out _);

            Assert.Contains(saved.PropertyDefinitions, p => string.Equals(p.Name, externalProperty.Name) && p.Type.DataType == PropertyDataType.String);
        }

        [Fact]
        public void Save_IfExistingPropertyHasBeenRemoved_ShouldRemoveDefinition()
        {
            var contentTypeId = Guid.NewGuid();
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId };
            existing.PropertyDefinitions.Add(new PropertyDefinition { Name = "ExistingProperty", Type = new PropertyDefinitionType { DataType = PropertyDataType.Number } });
            ContentType saved = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saved = c.Single());

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                    new ExternalContentType
                    {
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString(),
                        Id = contentTypeId,
                        Properties = Enumerable.Empty<ExternalProperty>()
                    }
                },
                out _);

            Assert.Empty(saved.PropertyDefinitions);
        }

        [Fact]
        public void Save_ForExistingContentTypeIfPropertyDoesNotExist_ShouldAddPropertyDefinitionToContentType()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId };
            ContentType saved = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saved = c.Single());

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                    new ExternalContentType
                    {
                        Id = contentTypeId,
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString(),
                        Properties = new[]
                        {
                            externalProperty
                        }
                    }
                },
                out _);

            Assert.Contains(saved.PropertyDefinitions, p => string.Equals(p.Name, externalProperty.Name) && p.Type.DataType == PropertyDataType.String);
        }

        [Fact]
        public void Save_WhenAllowedDowngradesIsSet_ShouldSetDowngradeComponentToMajor()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId, Version = new Version(2, 0, 0) };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                        new ExternalContentType
                        {
                            Id = contentTypeId,
                            Name = "ContentType",
                            BaseType = ContentTypeBase.Page.ToString(),
                            Properties = new[]
                            {
                                externalProperty
                            },
                            Version = "1.0.0"
                        }
                },
                out _,
                allowedDowngrades: VersionComponent.Major);

            Assert.Equal(VersionComponent.Major, saveOptions.AllowedDowngrades);
        }

        [Fact]
        public void Save_WhenAllowedDowngradesIsNotSet_ShouldSetDowngradeComponentToNone()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                        new ExternalContentType
                        {
                            Id = contentTypeId,
                            Name = "ContentType",
                            BaseType = ContentTypeBase.Page.ToString(),
                            Properties = new[]
                            {
                                externalProperty
                            }
                        }
                },
                out _);

            Assert.Equal(VersionComponent.None, saveOptions.AllowedDowngrades);
        }

        [Fact]
        public void Save_WhenAllowedUpgradesIsNotSet_ShouldSetUpgradeComponentToMinor()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                        new ExternalContentType
                        {
                            Id = contentTypeId,
                            Name = "ContentType",
                            BaseType = ContentTypeBase.Page.ToString(),
                            Properties = new[]
                            {
                                externalProperty
                            }
                        }
                },
                out _);

            Assert.Equal(VersionComponent.Minor, saveOptions.AllowedUpgrades);
        }

        [Fact]
        public void Save_WhenAllowedUpgradesIsSetToMajor_ShouldSetUpgradeComponentToMajor()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
            new[]
            {
                    new ExternalContentType
                    {
                        Id = contentTypeId,
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString(),
                        Properties = new[]
                        {
                            externalProperty
                        }
                    }
            },
            out _,
            allowedUpgrades: VersionComponent.Major);

            Assert.Equal(VersionComponent.Major, saveOptions.AllowedUpgrades);
        }

        [Fact]
        public void Save_WhenAllowedUpgradesIsNotSetAndVersionChangeIsMajor_ShouldSetUpgradeComponentToMajor()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId, Version = new Version(1, 0, 0) };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                        new ExternalContentType
                        {
                            Id = contentTypeId,
                            Name = "ContentType",
                            BaseType = ContentTypeBase.Page.ToString(),
                            Version = "2.0.0",
                            Properties = new[]
                            {
                                externalProperty
                            }
                        }
                },
                out _);

            Assert.Equal(VersionComponent.Major, saveOptions.AllowedUpgrades);
        }

        [Fact]
        public void Save_WhenAllowedUpgradesIsNotSetAndVersionChangeIsMinor_ShouldSetUpgradeComponentToMinor()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId, Version = new Version(1, 0, 0) };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
                new[]
                {
                        new ExternalContentType
                        {
                            Id = contentTypeId,
                            Name = "ContentType",
                            BaseType = ContentTypeBase.Page.ToString(),
                            Version = "1.1.0",
                            Properties = new[]
                            {
                                externalProperty
                            }
                        }
                },
                out _);

            Assert.Equal(VersionComponent.Minor, saveOptions.AllowedUpgrades);
        }

        [Fact]
        public void Save_WhenAllowedUpgradesIsNotSetAndNoVersionOnPosted_ShouldSetAutoIncrement()
        {
            var contentTypeId = Guid.NewGuid();
            var externalProperty = new ExternalProperty { Name = "SomeProperty", DataType = new ExternalPropertyDataType(nameof(PropertyString)) };
            var propertyTypeResolver = new Mock<PropertyDataTypeResolver>();
            propertyTypeResolver.Setup(p => p.Resolve(It.Is<ExternalPropertyDataType>(p => p.Equals(new ExternalPropertyDataType(nameof(PropertyString)))))).Returns(new PropertyDefinitionType { DataType = PropertyDataType.String });

            var existing = new ContentType { GUID = contentTypeId, Version = new Version(1, 0, 0) };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);

            var subject = Subject(inner.Object, Mapper(propertyTypeResolver.Object));

            subject.Save(
            new[]
            {
                    new ExternalContentType
                    {
                        Id = contentTypeId,
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString(),
                        Properties = new[]
                        {
                            externalProperty
                        }
                    }
            },
            out _);

            Assert.True(saveOptions.AutoIncrementVersion.Value);
        }

        [Fact]
        public void Save_WhenAllowedDowngradesIsSetAndVersionIsNotSet_ShouldNotSetAllowedDowngrades()
        {
            var contentTypeId = Guid.NewGuid();
            var existing = new ContentType { GUID = contentTypeId };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);
            var subject = Subject(inner.Object);

            subject.Save(
            new[]
            {
                    new ExternalContentType
                    {
                        Id = contentTypeId,
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString()
                    }
            },
            out _, VersionComponent.Major);
            Assert.Equal(VersionComponent.None, saveOptions.AllowedDowngrades);
        }

        [Fact]
        public void Save_WhenAllowedDowngradesIsSetAndVersionIsSet_ShouldSetAllowedDowngrades()
        {
            var contentTypeId = Guid.NewGuid();
            var existing = new ContentType { GUID = contentTypeId, Version = new Version(1, 0, 0) };
            ContentTypeSaveOptions saveOptions = null;
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(contentTypeId)).Returns(existing);
            inner.Setup(x => x.Save(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).Callback<IEnumerable<ContentType>, ContentTypeSaveOptions>((c, o) => saveOptions = o);
            var subject = Subject(inner.Object);

            subject.Save(
            new[]
            {
                    new ExternalContentType
                    {
                        Id = contentTypeId,
                        Name = "ContentType",
                        BaseType = ContentTypeBase.Page.ToString(),
                        Version = "1.0.0.0"
                    }
            },
            out _, VersionComponent.Major);
            Assert.Equal(VersionComponent.Major, saveOptions.AllowedDowngrades);
        }

        [Fact]
        public void TryDelete_WhenNoMatchingContentTypeExists_ShouldReturnFalse()
        {
            Assert.False(Subject().TryDelete(Guid.NewGuid()));
        }

        [Fact]
        public void TryDelete_WhenMatchingContentTypeExists_ShouldReturnTrue()
        {
            var existing = new ContentType { GUID = Guid.NewGuid(), Name = "One", Base = ContentTypeBase.Page };

            var subject = Subject(existing);

            Assert.True(subject.TryDelete(existing.GUID));
        }

        [Fact]
        public void TryDelete_WhenMatchingContentTypeExists_ShouldCallDeleteOnInnerRepository()
        {
            var existing = new PageType { GUID = Guid.NewGuid(), Name = "One" };

            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(existing.GUID)).Returns(existing);
            var subject = Subject(inner.Object);

            subject.TryDelete(existing.GUID);

            inner.Verify(x => x.Delete(existing), Times.Once());
        }

        [Fact]
        public void TryDelete_WithRootPage_ShouldThrow()
        {
            var inner = new Mock<ContentTypeRepository>();
            var subject = Subject(inner.Object);

            Assert.Throws<ErrorException>(() => subject.TryDelete(SystemContentTypes.RootPage.GUID));

            inner.Verify(x => x.Delete(It.IsAny<ContentType>()), Times.Never());
        }

        [Fact]
        public void TryDelete_WithRecycleBin_ShouldThrow()
        {
            var inner = new Mock<ContentTypeRepository>();
            var subject = Subject(inner.Object);

            Assert.Throws<ErrorException>(() => subject.TryDelete(SystemContentTypes.RecycleBin.GUID));

            inner.Verify(x => x.Delete(It.IsAny<ContentType>()), Times.Never());
        }

        [Fact]
        public void TryDelete_WithContentFolder_ShouldThrow()
        {
            var inner = new Mock<ContentTypeRepository>();
            var subject = Subject(inner.Object);

            Assert.Throws<ErrorException>(() => subject.TryDelete(SystemContentTypes.ContentFolder.GUID));

            inner.Verify(x => x.Delete(It.IsAny<ContentType>()), Times.Never());
        }

        [Fact]
        public void TryDelete_WithContentAssetFolder_ShouldThrow()
        {
            var inner = new Mock<ContentTypeRepository>();
            var subject = Subject(inner.Object);

            Assert.Throws<ErrorException>(() => subject.TryDelete(SystemContentTypes.ContentAssetFolder.GUID));

            inner.Verify(x => x.Delete(It.IsAny<ContentType>()), Times.Never());
        }

        [Fact]
        public void TryDelete_WhenContentTypeInUse_ShouldThrow()
        {
            var existing = new ContentType { GUID = Guid.NewGuid(), Name = "One", Base = ContentTypeBase.Page };

            var externalContentTypeServiceExtensionMock = new Mock<IExternalContentTypeServiceExtension>();
            externalContentTypeServiceExtensionMock.Setup(x => x.TryDelete(existing.GUID)).Throws<DataAbstractionException>();

            var subject = Subject(externalContentTypeServiceExtension: externalContentTypeServiceExtensionMock.Object);

            var exception = Assert.Throws<ErrorException>(() => subject.TryDelete(existing.GUID));

            Assert.Equal(ProblemCode.InUse, exception.ErrorResponse.Error.Code);
        }

        private static ExternalContentTypeRepository Subject(params ContentType[] contentTypes)
        {
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.List()).Returns(contentTypes);
            inner.Setup(x => x.Load(It.IsAny<Guid>())).Returns<Guid>(id => contentTypes.FirstOrDefault(x => x.GUID == id));

            return Subject(inner.Object);
        }

        private static ExternalContentTypeRepository Subject(
            ContentTypeRepository internalRepository = null,
            ContentTypeMapper mapper = null,
            IExternalContentTypeServiceExtension externalContentTypeServiceExtension = null)
          => new ExternalContentTypeRepository(
            internalRepository ?? Mock.Of<ContentTypeRepository>(),
            mapper ?? Mapper(),
            new List<IExternalContentTypeServiceExtension> {
                externalContentTypeServiceExtension ?? new DefaultExternalContentTypeServiceExtension(
                    internalRepository ?? Mock.Of<ContentTypeRepository>(),
                    new List<IContentTypeBaseProvider> { new DefaultContentTypeBaseProvider() }
                )
            });

        private static ContentTypeMapper Mapper(PropertyDataTypeResolver propertyDataTypeResolver = null)
            => new ContentTypeMapper(
                propertyDataTypeResolver ?? Mock.Of<PropertyDataTypeResolver>(),
                Mock.Of<IPropertyValidationSettingsRepository>(),
                Mock.Of<ITabDefinitionRepository>());
    }
}
