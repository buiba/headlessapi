using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using EPiServer.SpecializedProperties;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class Analyze
    {
        private readonly ServiceFixture _fixture;

        public Analyze(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(AnalyzeData))]
        public async Task Analyze_ContentType(Guid contentTypeId, ExternalContentType originalContentType, ExternalContentType contentType, IEnumerable<ContentTypeDifference> expectedResults)
        {
            contentType.Id = contentTypeId;
            originalContentType.Id = contentTypeId;
            using (_fixture.WithContentTypeIds(contentTypeId))
            {
                await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(originalContentType));
                var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentTypeId);
                AssertResponse.OK(response);

                response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix + "analyze", new JsonContent(contentType));
                AssertResponse.OK(response);
                var content = await response.Content.ReadAsStringAsync();

                var constraint = content.Should().BeValidJson()
                    .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                    .Which.Should().NotBeEmpty();

                foreach (var expectedResult in expectedResults)
                {
                    var tokenString = expectedResult.IsValid
                        ? $"{{ versionComponent: {(int)expectedResult.VersionComponent}, reason: \"{expectedResult.Reason}\" }}"
                        : $"{{ isValid: false, reason: \"{expectedResult.Reason}\" }}";

                    constraint = constraint.And.ContainEquivalentOf(JToken.Parse(tokenString));
                }
            }
        }

        [Fact]
        public async Task Analyze_WhenContentTypeMatchesOriginal_ShouldReturnEmptyArray()
        {
            var contentType = new ExternalContentType
            {
                Id = Guid.NewGuid(),
                Name = "ContentType_Test1",
                BaseType = ContentTypeBase.Page.ToString(),
                Properties = new[] { new ExternalProperty { Name = "prop", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) } }
            };

            using (_fixture.WithContentTypeIds(contentType.Id))
            {
                var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));
                AssertResponse.Created(response);

                response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix + "analyze", new JsonContent(contentType));
                AssertResponse.OK(response);

                var content = await response.Content.ReadAsStringAsync();

                var constraint = content.Should().BeValidJson()
                    .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                    .Which.Should().BeEmpty();
            }
        }

        [Theory]
        [MemberData(nameof(AnalyzeReasonData))]
        public async Task Analyze_ContentType_ShouldReturnCorrectReason(Guid contentTypeId, ExternalContentType originalContentType, ExternalContentType contentType, string[] expectedReasons)
        {
            contentType.Id = contentTypeId;
            originalContentType.Id = contentTypeId;

            var tabDefinitions = new List<TabDefinition>
            {
                new TabDefinition { Name = "GroupTest1"},
                new TabDefinition { Name = "GroupTest2"}
            };

            await _fixture.WithTabs(tabDefinitions, async () =>
            {
                using (_fixture.WithContentTypeIds(contentTypeId))
                {
                    await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(originalContentType));
                    var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentTypeId);
                    AssertResponse.OK(response);

                    response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix + "analyze", new JsonContent(contentType));
                    AssertResponse.OK(response);

                    var jObject = JToken.Parse(await response.Content.ReadAsStringAsync());
                    var actualReasons = jObject.Select(m => m["reason"].ToString()).ToArray();

                    foreach (var expectedReason in expectedReasons)
                    {
                        Assert.Contains(expectedReason, actualReasons);
                    }
                }
            });
        }

        public static TheoryData AnalyzeReasonData => new TheoryData<Guid, ExternalContentType, ExternalContentType, string[]>
        {
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    EditSettings = new ExternalContentTypeEditSettings(null, null, false, 0),
                    Properties = new [] { new ExternalProperty { Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyNumber)) } }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    EditSettings = new ExternalContentTypeEditSettings(null, null, true, 1),
                    Properties = new [] {
                        new ExternalProperty {
                            Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyNumber)),
                            BranchSpecific = true
                        }
                    }
                },
                new string[] {
                    PropertyFieldChangedReason("prop1", $"'{nameof(ExternalProperty.BranchSpecific)}'", $"{false}", $"{true}"),
                    $"Content type '{nameof(ExternalContentTypeEditSettings.Available)}' changed from '{false}' to '{true}'",
                    $"Content type '{nameof(ExternalContentTypeEditSettings.Order)}' has changed from '0' to '1'"
                }
            },
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] {
                        new ExternalProperty {
                            Name = "prop1",
                            DataType = new ExternalPropertyDataType(nameof(PropertyNumber)),
                            EditSettings = new ExternalPropertyEditSettings(VisibilityStatus.Default, "DisplayName1", "GroupTest1", 0, string.Empty, "Hint1")
                        }
                    }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] {
                        new ExternalProperty {
                            Name = "prop1",
                            DataType = new ExternalPropertyDataType(nameof(PropertyNumber)),
                            EditSettings = new ExternalPropertyEditSettings(VisibilityStatus.Hidden, "DisplayName2", "GroupTest2", 1, string.Empty, "Hint2")
                        }
                    }
                },
                new string[] {
                    PropertyFieldChangedReason("prop1", $"'{nameof(ExternalPropertyEditSettings.DisplayName)}'" , "DisplayName1", "DisplayName2"),
                    PropertyFieldChangedReason("prop1", $" '{nameof(ExternalPropertyEditSettings.Order)}'" , "0", "1"),
                    PropertyFieldChangedReason("prop1", $"'{nameof(ExternalPropertyEditSettings.Visibility)}'" , "Default", "Hidden"),
                    PropertyFieldChangedReason("prop1", $"'{nameof(ExternalPropertyEditSettings.Hint)}'" , "Hint1", "Hint2"),
                    PropertyFieldChangedReason("prop1", $"'{nameof(ExternalPropertyEditSettings.GroupName)}'" , "GroupTest1", "GroupTest2")
                }
            }
        };

        public static TheoryData AnalyzeData => new TheoryData<Guid, ExternalContentType, ExternalContentType, IEnumerable<ContentTypeDifference>>
        {
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean))} }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test2",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean))} }
                },
                new []
                {
                    NameChangedDifference("ContentType_Test1", "ContentType_Test2"),
                }
            },
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean))} }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test2",
                    BaseType = ContentTypeBase.Block.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) } }
                },
                new []
                {
                    BaseChangedDifference(ContentTypeBase.Page, ContentTypeBase.Block),
                    NameChangedDifference("ContentType_Test1", "ContentType_Test2"),
                }
            },
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) } }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test2",
                    BaseType = ContentTypeBase.Block.ToString(),
                    Properties = new []
                    {
                        new ExternalProperty { Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) },
                        new ExternalProperty { Name = "prop2", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) }
                    }
                },
                new []
                {
                    BaseChangedDifference(ContentTypeBase.Page, ContentTypeBase.Block),
                    NameChangedDifference("ContentType_Test1", "ContentType_Test2"),
                    PropertyAddedDifference("prop2"),
                }
            },
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new []
                    {
                        new ExternalProperty { Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) },
                        new ExternalProperty() { Name = "prop2", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) },
                    }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test2",
                    BaseType = ContentTypeBase.Block.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean))} }
                },
                new []
                {
                    BaseChangedDifference(ContentTypeBase.Page, ContentTypeBase.Block),
                    NameChangedDifference("ContentType_Test1", "ContentType_Test2"),
                    PropertyRemovedDifference("prop2"),
                }
            },
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty { Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) } }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test2",
                    BaseType = ContentTypeBase.Block.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop2", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) } }
                },
                new []
                {
                    BaseChangedDifference(ContentTypeBase.Page, ContentTypeBase.Block),
                    NameChangedDifference("ContentType_Test1", "ContentType_Test2"),
                    PropertyRemovedDifference("prop1"),
                    PropertyAddedDifference("prop2"),
                }
            },
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty { Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyBoolean)) } }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test2",
                    BaseType = ContentTypeBase.Block.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyDate)) } }
                },
                new []
                {
                    BaseChangedDifference(ContentTypeBase.Page, ContentTypeBase.Block),
                    NameChangedDifference("ContentType_Test1", "ContentType_Test2"),
                    PropertyTypeChangedDifference("prop1", "Boolean", "DateTime"),
                }
            },
            {
                Guid.NewGuid(),
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty { Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyXhtmlString)) } }
                },
                new ExternalContentType
                {
                    Name = "ContentType_Test1",
                    BaseType = ContentTypeBase.Page.ToString(),
                    Properties = new [] { new ExternalProperty {  Name = "prop1", DataType = new ExternalPropertyDataType(nameof(PropertyNumber)) } }
                },
                new []
                {
                    PropertyTypeChangedDifference("prop1", "XhtmlString", "Integer"),
                }
            },
        };

        private static ContentTypeDifference NameChangedDifference(string original, string updated) => new ContentTypeDifference(VersionComponent.Major, $"Content type 'Name' has changed from '{original}' to '{updated}'");
        private static ContentTypeDifference BaseChangedDifference(ContentTypeBase original, ContentTypeBase updated) => ContentTypeDifference.Invalid($"Content type 'Base' has changed from '{original}' to '{updated}'");
        private static ContentTypeDifference PropertyRemovedDifference(string propertyName) => new ContentTypeDifference(VersionComponent.Major, $"'1' properties has been removed, '{propertyName}'");
        private static ContentTypeDifference PropertyAddedDifference(string propertyName) => new ContentTypeDifference(VersionComponent.Minor, $"'1' properties has been added, '{propertyName}'");
        private static ContentTypeDifference PropertyTypeChangedDifference(string propertyName, string previousPropertyType, string newPropertyType) => new ContentTypeDifference(VersionComponent.Major, $"Property: '{propertyName}': 'Type' has changed from '{previousPropertyType}' to '{newPropertyType}'");
        private static string PropertyFieldChangedReason(string propertyName, string propertyFieldName, string previousPropertyType, string newPropertyType) => $"The property '{propertyName}' has changed: {propertyFieldName} has changed from '{previousPropertyType}' to '{newPropertyType}'";
    }
}
