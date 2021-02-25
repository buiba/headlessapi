using System;
using Xunit;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    public class ManifestSectionImporterResolverTests
    {
        [Fact]
        public void Resolve_WhenMatchingImporterExist_ShouldReturnImporter()
        {
            var sut = new ManifestSectionImporterResolver(new IManifestSectionImporter[]
            {
                new ImporterA(),
                new ImporterB(),
                new ImporterC()
            });

            var result = sut.Resolve(new SectionB());

            Assert.IsType<ImporterB>(result);
        }

        [Fact]
        public void Resolve_WhenNoMatchingImporterExist_ShouldReturnNull()
        {
            var sut = new ManifestSectionImporterResolver(new IManifestSectionImporter[]
            {
                new ImporterA(),
                new ImporterB()
            });

            var result = sut.Resolve(new SectionC());

            Assert.Null(result);
        }

        [Fact]
        public void ResolveManifestSectionType_WhenMatchingImporterExist_ShouldReturnType()
        {
            var sut = new ManifestSectionImporterResolver(new IManifestSectionImporter[]
            {
                new ImporterA(),
                new ImporterB(),
                new ImporterC()
            });

            var result = sut.ResolveManifestSectionType("SectionB");

            Assert.True(result == typeof(SectionB));
        }

        [Fact]
        public void ResolveManifestSectionType_WhenNoMatchingImporterExist_ShouldReturnNull()
        {
            var sut = new ManifestSectionImporterResolver(new IManifestSectionImporter[]
            {
                new ImporterA(),
                new ImporterB()
            });

            var result = sut.ResolveManifestSectionType("SectionC");

            Assert.Null(result);
        }

        private class SectionA : IManifestSection
        { }

        private class SectionB : IManifestSection
        { }

        private class SectionC : IManifestSection
        { }

        private class ImporterA : IManifestSectionImporter
        {
            public Type SectionType => typeof(SectionA);

            public string SectionName => nameof(SectionA);

            public int Order => 1;

            public void Import(IManifestSection section, ImportContext importContext)
            {
                return;
            }
        }

        private class ImporterB : IManifestSectionImporter
        {
            public Type SectionType => typeof(SectionB);

            public string SectionName => nameof(SectionB);

            public int Order => 2;

            public void Import(IManifestSection section, ImportContext importContext)
            {
                return;
            }
        }

        private class ImporterC : IManifestSectionImporter
        {
            public Type SectionType => typeof(SectionC);

            public string SectionName => nameof(SectionC);

            public int Order => 3;

            public void Import(IManifestSection section, ImportContext importContext)
            {
                return;
            }
        }
    }
}
