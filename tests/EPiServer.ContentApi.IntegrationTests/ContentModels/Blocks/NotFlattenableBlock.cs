using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks
{
    public class NotFlattenableBlockPropertyModel : IPropertyModel<PropertyBlock>
    {
        [JsonIgnore]
        public PropertyBlock PropertyDataProperty { get; set; }

        public string Name { get; set; }

        public string PropertyDataType { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public class NotFlattenableBlockPropertyConverter : IPropertyConverter
    {
        public IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext)
        {
            return new NotFlattenableBlockPropertyModel
            {
                Name = propertyData.Name,
                PropertyDataProperty = (PropertyBlock)propertyData,
                PropertyDataType = nameof(PropertyBlock),
                Properties = new Dictionary<string, object>()
                {
                    { "Title", "FakeTitle" },
                    { "Image", new Url("http://localhost") }
                }
            };
        }
    }

    [ServiceConfiguration(typeof(IPropertyConverterProvider))]
    public class NotFlattenableBlockPropertyConverterProvider : IPropertyConverterProvider
    {
        public int SortOrder => 999;

        public IPropertyConverter Resolve(PropertyData propertyData)
        {
            if (propertyData.Value is NotFlattenableBlock)
            {
                return new NotFlattenableBlockPropertyConverter();
            }

            return null;
        }
    }
}
