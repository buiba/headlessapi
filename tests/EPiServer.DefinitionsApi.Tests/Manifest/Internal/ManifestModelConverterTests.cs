using System;
using System.Globalization;
using System.IO;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    public class ManifestModelConverterTests
    {
        [Fact]
        public void ReadJson_ShouldConvertValueTypes()
        {
            var json = @"{
                             ""testInteger"": 99,
                             ""testFloat"": 99.99,
                             ""testString"": ""test"",
                             ""testBoolean"": true,
                             ""testDate"": ""2020-10-20 23:30"",
                             ""testGuid"": ""128a8e28-d54d-42c6-a525-c5f804d0a41e"",
                             ""testUri"": ""http://www.example.com"",
                             ""testTimeSpan"": ""00:10:00"",
                             ""TestEnumType"": ""Value1""
                         }";

            var reader = new JsonTextReader(new StringReader(json));

            var sut = new ManifestModelConverter(Mock.Of<ManifestSectionImporterResolver>());

            var result = sut.ReadJson(reader, typeof(TestManifestModel), null, JsonSerializer.CreateDefault()) as TestManifestModel;

            Assert.NotNull(result);
            Assert.Equal(99, result.TestInteger);
            Assert.Equal(float.Parse("99.99", CultureInfo.InvariantCulture), result.TestFloat);
            Assert.Equal("test", result.TestString);
            Assert.True(result.TestBoolean);
            Assert.Equal(DateTime.Parse("2020-10-20 23:30"), result.TestDate);
            Assert.Equal(Guid.Parse("128a8e28-d54d-42c6-a525-c5f804d0a41e"), result.TestGuid);
            Assert.Equal(new Uri("http://www.example.com"), result.TestUri);
            Assert.Equal(TimeSpan.FromMinutes(10), result.TestTimeSpan);
            Assert.Equal(EnumType.Value1, result.TestEnumType);
        }

        [Fact]
        public void ReadJson_WhenInvalidEnumValue_ShouldUseDefaultEnumValue()
        {
            var json = @"{
                             ""TestEnumType"": ""xxx""
                         }";

            var reader = new JsonTextReader(new StringReader(json));

            var sut = new ManifestModelConverter(Mock.Of<ManifestSectionImporterResolver>());

            var result = sut.ReadJson(reader, typeof(TestManifestModel), null, JsonSerializer.CreateDefault()) as TestManifestModel;

            Assert.NotNull(result);
            Assert.Equal(EnumType.Default, result.TestEnumType);
        }

        [Fact]
        public void ReadJson_ShouldConvertReferenceTypes()
        {
            var json = @"{
                             ""testArray"": [ ""1"", ""2"" ],
                             ""testComplexType"": {
                                 ""property1"": ""Value1"",
                                 ""property2"": ""Value2""
                             },
                         }";

            var reader = new JsonTextReader(new StringReader(json));

            var sut = new ManifestModelConverter(Mock.Of<ManifestSectionImporterResolver>());

            var result = sut.ReadJson(reader, typeof(TestManifestModel), null, JsonSerializer.CreateDefault()) as TestManifestModel;

            Assert.NotNull(result);

            Assert.Collection(
                result.TestArray,
                x => Assert.Equal("1", x),
                x => Assert.Equal("2", x));

            Assert.NotNull(result.TestComplexType);
            Assert.Equal("Value1", result.TestComplexType.Property1);
            Assert.Equal("Value2", result.TestComplexType.Property2);
        }

        [Fact]
        public void ReadJson_WhenSectionsExist_ShouldConvertObjectsToSections()
        {
            var json = @"{
                             ""sectionA"": {
                                 ""property1"": ""Value1"",
                                 ""property2"": ""Value2""
                             },
                             ""sectionB"": { },
                             ""sectionC"": { }
                         }";

            var resolverMock = new Mock<ManifestSectionImporterResolver>();

            resolverMock
                .Setup(x => x.ResolveManifestSectionType(It.Is<string>(x => x.Equals("sectionA", StringComparison.OrdinalIgnoreCase))))
                .Returns(typeof(SectionA));

            resolverMock
                .Setup(x => x.ResolveManifestSectionType(It.Is<string>(x => x.Equals("sectionB", StringComparison.OrdinalIgnoreCase))))
                .Returns(typeof(SectionB));

            var reader = new JsonTextReader(new StringReader(json));

            var sut = new ManifestModelConverter(resolverMock.Object);

            var result = sut.ReadJson(reader, typeof(TestManifestModel), null, JsonSerializer.CreateDefault()) as TestManifestModel;

            Assert.NotNull(result);
            Assert.Collection(
                result.Sections.Values,
                x => Assert.True(x is SectionA),
                x => Assert.True(x is SectionB));
        }

        [Fact]
        public void ReadJson_WhenMissingMemberAndMissingMemberHandlingIsIgnore_ShouldNotThrow()
        {
            var json = @"{""missingMember"": ""test""}";

            var reader = new JsonTextReader(new StringReader(json));

            var sut = new ManifestModelConverter(Mock.Of<ManifestSectionImporterResolver>());

            var result = sut.ReadJson(reader, typeof(TestManifestModel), null, JsonSerializer.CreateDefault()) as TestManifestModel;

            Assert.NotNull(result);
        }

        [Fact]
        public void ReadJson_WhenMissingMemberAndMissingMemberHandlingIsError_ShouldThrowMissingMemberException()
        {
            var json = @"{""missingMember"": ""test""}";

            var reader = new JsonTextReader(new StringReader(json));

            var sut = new ManifestModelConverter(Mock.Of<ManifestSectionImporterResolver>());

            var serializer = JsonSerializer.CreateDefault();
            serializer.MissingMemberHandling = MissingMemberHandling.Error;

            Assert.Throws<MissingMemberException>(() => sut.ReadJson(reader, typeof(TestManifestModel), null, serializer));
        }

        private class TestManifestModel : ManifestModel
        {
            public string[] TestArray { get; set; }

            public int TestInteger { get; set; }

            public float TestFloat { get; set; }

            public string TestString { get; set; }

            public bool TestBoolean { get; set; }

            public DateTime TestDate { get; set; }

            public Guid TestGuid { get; set; }

            public Uri TestUri { get; set; }

            public TimeSpan TestTimeSpan { get; set; }

            public ComplexType TestComplexType { get; set; }

            public EnumType TestEnumType { get; set; }
        }

        private class ComplexType
        {
            public string Property1 { get; set; }

            public string Property2 { get; set; }
        }

        private enum EnumType
        {
            Default,
            Value1,
            Value2,
            Value3
        }

        private class SectionA : IManifestSection
        {
            public string Property1 { get; set; }

            public string Property2 { get; set; }
        }

        private class SectionB : IManifestSection
        { }

        private class SectionC : IManifestSection
        { }
    }
}
