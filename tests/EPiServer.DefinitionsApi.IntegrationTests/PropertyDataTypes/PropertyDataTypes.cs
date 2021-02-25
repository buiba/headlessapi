using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using EPiServer.SpecializedProperties;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.PropertyDataTypes
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class PropertyDataTypes : IAsyncLifetime
    {
        private readonly ServiceFixture _fixture;
        private string _listResult;

        public PropertyDataTypes(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        public static TheoryData ExpectedPropertyTypes => new TheoryData<string, string>
        {
            { nameof(PropertyAppSettings), null },
            { nameof(PropertyAppSettingsMultiple), null },
            { nameof(PropertyBlob), null },
            { nameof(PropertyBoolean), null },
            { nameof(PropertyCategory), null },
            { nameof(PropertyContentArea), null },
            { nameof(PropertyCheckBoxList), null },
            { nameof(PropertyContentReference), null },
            { nameof(PropertyContentReferenceList), null },
            { nameof(PropertyDate), null },
            { nameof(PropertyDateList), null },
            { nameof(PropertyDocumentUrl), null },
            { nameof(PropertyDoubleList), null },
            { nameof(PropertyDropDownList), null },
            { nameof(PropertyFileSortOrder), null },
            { nameof(PropertyFloatNumber), null },
            { nameof(PropertyFrame), null },
            { nameof(PropertyImageUrl), null },
            { nameof(PropertyIntegerList), null },
            { nameof(PropertyLanguage), null },
            { nameof(PropertyLinkCollection), null },
            { nameof(PropertyLongString), null },
            { nameof(PropertyNumber), null },
            { nameof(PropertyPageReference), null },
            { nameof(PropertyPageType), null },
            { nameof(PropertySortOrder), null },
            { nameof(PropertyString), null },
            { nameof(PropertyStringList), null },
            { nameof(PropertyUrl), null },
            { nameof(PropertyXhtmlString), null },
        };

        public async Task InitializeAsync()
        {
            // As there is only one PropertyDataTypes endpoint (without parameters) we will call it once here.
            var response = await _fixture.Client.GetAsync(PropertyDataTypesController.RoutePrefix);
            AssertResponse.OK(response);

            _listResult = await response.Content.ReadAsStringAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Theory]
        [MemberData(nameof(ExpectedPropertyTypes))]
        public void List_ShouldReturnBuiltInDataTypesInArray(string dataType, string itemType)
        {
            _listResult.Should().BeValidJson()
                .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                .Which.Should().NotBeEmpty()
                .And.Contain(x => (string)x["dataType"] == dataType && (string)x["itemType"] == itemType);
        }
    }
}
