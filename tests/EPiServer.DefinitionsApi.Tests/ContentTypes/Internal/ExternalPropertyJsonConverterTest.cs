using EPiServer.Core;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using EPiServer.Validation;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    public class ExternalPropertyJsonConverterTest
    {
        public static readonly TheoryData Serializations = new TheoryData<ExternalProperty, string>
        {
            {
                new ExternalProperty { Name = "One" },
                "{\"name\":\"One\",\"dataType\":null,\"branchSpecific\":false}"
            },
            {
                new ExternalProperty { Name = "One", DataType = new ExternalPropertyDataType(nameof(PropertyString)), BranchSpecific = true },
                $"{{\"name\":\"One\",\"dataType\":\"{nameof(PropertyString)}\",\"branchSpecific\":true}}"
            },
            {
                new ExternalProperty { Name = "AProperty", DataType = new ExternalPropertyDataType("PropertySomething"), BranchSpecific = true, EditSettings = new ExternalPropertyEditSettings(VisibilityStatus.Default, "MetaTitle", "Metadata", 100, null, null ) },
                "{\"name\":\"AProperty\",\"dataType\":\"PropertySomething\",\"branchSpecific\":true,\"editSettings\":{\"visibility\":\"default\",\"displayName\":\"MetaTitle\",\"groupName\":\"Metadata\",\"order\":100}}"
            },
            {
                new ExternalProperty { Name = "AProperty", DataType = ExternalPropertyDataType.Block("ABlockType"), BranchSpecific = false,EditSettings = new ExternalPropertyEditSettings(VisibilityStatus.Default, "PageImage", null, 100, null, "image" )},
                "{\"name\":\"AProperty\",\"dataType\":\"PropertyBlock\",\"itemType\":\"ABlockType\",\"branchSpecific\":false,\"editSettings\":{\"visibility\":\"default\",\"displayName\":\"PageImage\",\"order\":100,\"hint\":\"image\"}}"
            },
            {
                new ExternalProperty { Name = "AProperty", DataType = new ExternalPropertyDataType("PropertySomething"), EditSettings = new ExternalPropertyEditSettings(VisibilityStatus.Default, "TeaserText", null, 200, "helpTextHere", "textarea" ) },
                "{\"name\":\"AProperty\",\"dataType\":\"PropertySomething\",\"branchSpecific\":false,\"editSettings\":{\"visibility\":\"default\",\"displayName\":\"TeaserText\",\"order\":200,\"helpText\":\"helpTextHere\",\"hint\":\"textarea\"}}"
            },
            {
                new ExternalProperty { Name = "AProperty", DataType = new ExternalPropertyDataType("Whatever"), BranchSpecific = true },
                 "{\"name\":\"AProperty\",\"dataType\":\"Whatever\",\"branchSpecific\":true}"
            },
            {
                new ExternalProperty
                {
                    Name = "AProperty",
                    DataType = new ExternalPropertyDataType("PropertyString"),
                    Validation = {
                        new ExternalPropertyValidationSettings {
                            Name = "Any",
                            Severity = ValidationErrorSeverity.Warning,
                            ErrorMessage = "Bad!",
                            Settings = {
                                { "low", 5 },
                                { "high", 50 }
                            },
                        }
                    }
                },
                 "{\"name\":\"AProperty\",\"dataType\":\"PropertyString\",\"branchSpecific\":false,\"validation\":[{\"name\":\"Any\",\"severity\":\"warning\",\"errorMessage\":\"Bad!\",\"low\":5,\"high\":50}]}"
            },
        };

        [Theory]
        [MemberData(nameof(Serializations))]
        public void Serialize_WithProperty_ShouldReturnCorrectJson(ExternalProperty source, string expectedJson)
        {
            var json = JsonConvert.SerializeObject(source);

            Assert.Equal(expectedJson, json);
        }

        [Theory]
        [MemberData(nameof(Serializations))]
        public void Deserialize_WithProperty_ShouldReturnCorrectJson(ExternalProperty expected, string sourceJson)
        {
            var result = JsonConvert.DeserializeObject<ExternalProperty>(sourceJson);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
