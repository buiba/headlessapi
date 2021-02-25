using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    public class ManifestServiceTests
    {
        [Fact]
        public void ImportManifestSections_WhenNoImporterAndContinueOnError_ShouldContinue()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = true
            };

            var resolverMock = new Mock<ManifestSectionImporterResolver>();

            var sut = new ManifestService(resolverMock.Object);

            sut.ImportManifestSections(new IManifestSection[] { new SectionA(), new SectionB(), new SectionC() }, importContext);
        }

        [Fact]
        public void ImportManifestSections_WhenNoImporterAndContinueOnError_ShouldLogError()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = true
            };

            var resolverMock = new Mock<ManifestSectionImporterResolver>();

            var sut = new ManifestService(resolverMock.Object);

            sut.ImportManifestSections(new IManifestSection[] { new SectionA() }, importContext);

            Assert.Collection(importContext.Log, x => Assert.Equal(ImportLogMessageSeverity.Error, x.Severity));
        }

        [Fact]
        public void ImportManifestSections_WhenNoImporterAndNotContinueOnError_ShouldThrowArgumentException()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = false
            };

            var resolverMock = new Mock<ManifestSectionImporterResolver>();

            var sut = new ManifestService(resolverMock.Object);

            Assert.Throws<ArgumentException>(() => sut.ImportManifestSections(new IManifestSection[] { new SectionA() }, importContext));
        }

        [Fact]
        public void ImportManifestSections_WhenImporterExist_ShouldCallImportOnAll()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = true
            };

            var importerMock = new Mock<IManifestSectionImporter>();
            importerMock.Setup(x => x.Import(It.IsAny<IManifestSection>(), It.IsAny<ImportContext>()));

            var resolverMock = new Mock<ManifestSectionImporterResolver>();
            resolverMock.Setup(x => x.Resolve(It.IsAny<IManifestSection>())).Returns(importerMock.Object);

            var sut = new ManifestService(resolverMock.Object);

            sut.ImportManifestSections(new IManifestSection[] { new SectionA(), new SectionB(), new SectionC() }, importContext);

            importerMock.Verify(mock => mock.Import(It.IsAny<IManifestSection>(), It.IsAny<ImportContext>()), Times.Exactly(3));
        }

        [Fact]
        public void ImportManifestSections_WhenExceptionAndContinueOnError_ShouldContinue()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = true
            };

            var importerMock = new Mock<IManifestSectionImporter>();

            importerMock
                .Setup(x => x.Import(It.IsAny<SectionA>(), It.IsAny<ImportContext>()))
                .Throws<Exception>();

            var resolverMock = new Mock<ManifestSectionImporterResolver>();

            resolverMock
                .Setup(x => x.Resolve(It.IsAny<IManifestSection>()))
                .Returns(importerMock.Object);

            var sut = new ManifestService(resolverMock.Object);

            sut.ImportManifestSections(new IManifestSection[] { new SectionA(), new SectionB(), new SectionC() }, importContext);
            importerMock.Verify(mock => mock.Import(It.IsAny<IManifestSection>(), It.IsAny<ImportContext>()), Times.Exactly(3));
        }

        [Fact]
        public void ImportManifestSections_WhenException_ShouldLogError()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = true
            };

            var importerMock = new Mock<IManifestSectionImporter>();

            importerMock
                .Setup(x => x.Import(It.IsAny<IManifestSection>(), It.IsAny<ImportContext>()))
                .Throws<Exception>();

            var resolverMock = new Mock<ManifestSectionImporterResolver>();

            resolverMock
                .Setup(x => x.Resolve(It.IsAny<IManifestSection>()))
                .Returns(importerMock.Object);

            var sut = new ManifestService(resolverMock.Object);

            sut.ImportManifestSections(new IManifestSection[] { new SectionA() }, importContext);
            
            Assert.Collection(importContext.Log, x => Assert.Equal(ImportLogMessageSeverity.Error, x.Severity));
        }

        [Fact]
        public void ImportManifestSections_WhenExceptionAndNotContinueOnError_ShouldThrow()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = false
            };

            var importerMock = new Mock<IManifestSectionImporter>();
            importerMock.Setup(x => x.Import(It.IsAny<SectionC>(), It.IsAny<ImportContext>())).Throws<Exception>();

            var resolverMock = new Mock<ManifestSectionImporterResolver>();
            resolverMock.Setup(x => x.Resolve(It.IsAny<IManifestSection>())).Returns(importerMock.Object);

            var sut = new ManifestService(resolverMock.Object);

            Assert.Throws<Exception>(() => sut.ImportManifestSections(new IManifestSection[] { new SectionA(), new SectionB(), new SectionC() }, importContext));
        }

        [Fact]
        public void ImportManifestSections_ShouldImportInOrder()
        {
            var importContext = new ImportContext
            {
                ContinueOnError = true
            };

            var expectedOrder = new List<IManifestSection>() { new SectionA(), new SectionB(), new SectionC() };
            var importedOrder = new List<IManifestSection>();

            var resolverMock = new Mock<ManifestSectionImporterResolver>();

            resolverMock
                .Setup(x => x.Resolve(It.Is<IManifestSection>(x => x is SectionA)))
                .Returns(new ImporterA(importedOrder));

            resolverMock
                .Setup(x => x.Resolve(It.Is<IManifestSection>(x => x is SectionB)))
                .Returns(new ImporterB(importedOrder));

            resolverMock
                .Setup(x => x.Resolve(It.Is<IManifestSection>(x => x is SectionC)))
                .Returns(new ImporterC(importedOrder));

            var sut = new ManifestService(resolverMock.Object);

            sut.ImportManifestSections(new IManifestSection[] { new SectionC(), new SectionB(), new SectionA() }, importContext);

            Assert.True(importedOrder.SequenceEqual(expectedOrder, new TestSectionComparer()));
        }

        private class SectionA : IManifestSection
        { }

        private class SectionB : IManifestSection
        { }

        private class SectionC : IManifestSection
        { }

        private class TestSectionComparer : IEqualityComparer<IManifestSection>
        {
            public bool Equals(IManifestSection x, IManifestSection y)
            {
                if (x is null || y is null)
                {
                    return false;
                }

                return x.GetType() == y.GetType();
            }

            public int GetHashCode(IManifestSection obj)
            {
                return obj.GetHashCode();
            }
        }

        private class ImporterA : ImporterBase<SectionA>
        {
            public ImporterA(List<IManifestSection> list)
                : base(list)
            { }

            public override string SectionName => "SectionA";

            public override int Order => 1;
        }

        private class ImporterB : ImporterBase<SectionB>
        {
            public ImporterB(List<IManifestSection> list)
                : base(list)
            { }

            public override string SectionName => "SectionB";

            public override int Order => 2;
        }

        private class ImporterC : ImporterBase<SectionC>
        {
            public ImporterC(List<IManifestSection> list)
                : base(list)
            { }

            public override string SectionName => "SectionC";

            public override int Order => 3;
        }

        private abstract class ImporterBase<TSet> : IManifestSectionImporter
            where TSet : IManifestSection
        {
            private readonly List<IManifestSection> _list;

            public ImporterBase(List<IManifestSection> list)
            {
                _list = list;
            }

            public virtual Type SectionType => typeof(TSet);

            public virtual string SectionName { get; }

            public virtual int Order { get; }

            public void Import(IManifestSection section, ImportContext importContext)
            {
                _list.Add(section);
            }
        }
    }
}
