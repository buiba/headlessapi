using System;
using EPiServer.DefinitionsApi.Manifest;
using EPiServer.DefinitionsApi.PropertyGroups.Internal;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyGroups.Manifest
{
    public class PropertyGroupsSectionImporterTests
    {
        [Fact]
        public void Import_ShouldCallSave()
        {
            var repositoryMock = new Mock<PropertyGroupRepository>();

            var sut = new PropertyGroupsSectionImporter(repositoryMock.Object);

            var section = new PropertyGroupsSection
            {
                Items = new[]
                {
                    new PropertyGroupModel()
                }
            };

            sut.Import(section, new ImportContext());

            repositoryMock.Verify(x => x.Save(It.IsAny<PropertyGroupModel>()), Times.Once);
        }

        [Fact]
        public void Import_ShouldLogMessage()
        {
            var context = new ImportContext();

            var section = new PropertyGroupsSection
            {
                Items = new[]
                {
                    new PropertyGroupModel()
                }
            };

            var sut = new PropertyGroupsSectionImporter(Mock.Of<PropertyGroupRepository>());

            sut.Import(section, context);

            Assert.Collection(context.Log, x => Assert.Equal(ImportLogMessageSeverity.Success, x.Severity));
        }

        [Fact]
        public void Import_WhenNoGroup_ShouldLogMessage()
        {
            var context = new ImportContext();

            var section = new PropertyGroupsSection
            {
                Items = new PropertyGroupModel[0]
            };

            var sut = new PropertyGroupsSectionImporter(Mock.Of<PropertyGroupRepository>());

            sut.Import(section, context);

            Assert.Collection(context.Log, x => Assert.Equal(ImportLogMessageSeverity.Success, x.Severity));
        }

        [Fact]
        public void Import_WhenErrorAndContinueOnError_ShouldLogErrorAndContinue()
        {
            var repositoryMock = new Mock<PropertyGroupRepository>();

            repositoryMock
                .Setup(x => x.Save(It.Is<PropertyGroupModel>(x => x.Name == "Group1")))
                .Throws<Exception>();

            var context = new ImportContext
            {
                ContinueOnError = true
            };

            var section = new PropertyGroupsSection
            {
                Items = new[]
                {
                    new PropertyGroupModel
                    {
                        Name = "Group1"
                    },
                    new PropertyGroupModel
                    {
                        Name = "Group2"
                    }
                }
            };

            var sut = new PropertyGroupsSectionImporter(repositoryMock.Object);

            sut.Import(section, context);

            repositoryMock.Verify(x => x.Save(It.IsAny<PropertyGroupModel>()), Times.Exactly(2));

            Assert.Collection(
               context.Log,
               x => Assert.Equal(ImportLogMessageSeverity.Error, x.Severity),
               x => Assert.Equal(ImportLogMessageSeverity.Success, x.Severity));
        }

        [Fact]
        public void Import_WhenOneGroupAndErrorAndContinueOnError_ShouldLogOneEntry()
        {
            var repositoryMock = new Mock<PropertyGroupRepository>();

            repositoryMock
                .Setup(x => x.Save(It.Is<PropertyGroupModel>(x => x.Name == "Group1")))
                .Throws<Exception>();

            var context = new ImportContext
            {
                ContinueOnError = true
            };

            var section = new PropertyGroupsSection
            {
                Items = new[]
                {
                    new PropertyGroupModel
                    {
                        Name = "Group1"
                    }
                }
            };

            var sut = new PropertyGroupsSectionImporter(repositoryMock.Object);

            sut.Import(section, context);

            repositoryMock.Verify(x => x.Save(It.IsAny<PropertyGroupModel>()), Times.Exactly(1));

            Assert.Collection(
               context.Log,
               x => Assert.Equal(ImportLogMessageSeverity.Error, x.Severity));
        }

        [Fact]
        public void Import_WhenErrorAndNotContinueOnError_ShouldThrow()
        {
            var repositoryMock = new Mock<PropertyGroupRepository>();

            repositoryMock
               .Setup(x => x.Save(It.Is<PropertyGroupModel>(x => x.Name == "Group1")))
               .Throws<Exception>();

            var context = new ImportContext();

            var section = new PropertyGroupsSection
            {
                Items = new[]
                {
                    new PropertyGroupModel
                    {
                        Name = "Group1"
                    },
                    new PropertyGroupModel
                    {
                        Name = "Group2"
                    }
                }
            };

            var sut = new PropertyGroupsSectionImporter(repositoryMock.Object);

            Assert.Throws<Exception>(() => sut.Import(section, context));

            repositoryMock.Verify(x => x.Save(It.IsAny<PropertyGroupModel>()), Times.Exactly(1));

            Assert.Empty(context.Log);
        }  
    }
}
