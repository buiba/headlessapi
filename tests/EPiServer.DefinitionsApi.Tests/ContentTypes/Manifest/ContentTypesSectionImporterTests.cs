using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.Manifest;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes.Manifest
{
    public class ContentTypesSectionImporterTests
    {
        [Fact]
        public void Import_ShouldCallSave()
        {
            var repositoryMock = new Mock<ExternalContentTypeRepository>();

            var sut = new ContentTypesSectionImporter(repositoryMock.Object);

            var section = new ContentTypesSection
            {
                AllowedDowngrades = VersionComponent.Major,
                AllowedUpgrades = VersionComponent.Major,
                Items = new ValidatableExternalContentTypes
                {
                    new ExternalContentType()
                }
            };

            sut.Import(section, new ImportContext());

            var result = Enumerable.Empty<Guid>();

            repositoryMock.Verify(x =>
                x.Save(
                    It.IsAny<IEnumerable<ExternalContentType>>(),
                    out result,
                    It.IsAny<VersionComponent>(),
                    It.IsAny<VersionComponent>()),
                Times.Once);
        }

        [Fact]
        public void Import_ShouldLogMessage()
        {
            var repositoryMock = new Mock<ExternalContentTypeRepository>();
            var context = new ImportContext();

            var sut = new ContentTypesSectionImporter(repositoryMock.Object);

            sut.Import(new ContentTypesSection(), context);

            var result = Enumerable.Empty<Guid>();

            Assert.Collection(context.Log, x => Assert.Equal(ImportLogMessageSeverity.Success, x.Severity));
        }

        [Fact]
        public void Import_WhenGuidMissing_ShouldGenerateOne()
        {
            var repositoryMock = new Mock<ExternalContentTypeRepository>();

            var section = new ContentTypesSection
            {
                Items = new ValidatableExternalContentTypes
                {
                    new ExternalContentType
                    {
                        BaseType = ContentTypeBase.Page.ToString(),
                        Name = "ContentType1"
                    }
                }
            };

            var sut = new ContentTypesSectionImporter(repositoryMock.Object);

            sut.Import(section, new ImportContext());

            Assert.NotEqual(Guid.Empty, section.Items.First().Id);
        }

        [Fact]
        public void Import_WhenContentTypeExistAndGuidMissing_ShouldUseExistingGuid()
        {
            var expectedName = "ContentType1";
            var expectedId = Guid.NewGuid();

            var repositoryMock = new Mock<ExternalContentTypeRepository>();

            repositoryMock
                .Setup(x => x.Get(It.Is<string>(x => x == expectedName)))
                .Returns(new ExternalContentType { Id = expectedId });

            var section = new ContentTypesSection
            {
                Items = new ValidatableExternalContentTypes
                {
                    new ExternalContentType
                    {
                        BaseType = ContentTypeBase.Page.ToString(),
                        Name = expectedName
                    }
                }
            };

            var sut = new ContentTypesSectionImporter(repositoryMock.Object);

            sut.Import(section, new ImportContext());

            Assert.Equal(expectedId, section.Items.First().Id);
        }
    }
}
