using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CustomMappers
{
    [ServiceConfiguration(typeof(IPropertyModelConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CustomPropertyModelConverter : IPropertyModelConverter
    {
        public const string HandledPropertyName = "CustomPropertyMapped";
        public const string ConverterAddedprefix = "CustomHandled";

        public int SortOrder => 1000;

        public IEnumerable<TypeModel> ModelTypes => throw new NotImplementedException();

        public IPropertyModel ConvertToPropertyModel(EPiServer.Core.PropertyData propertyData, CultureInfo language, bool excludePersonalizedContent, bool expand)
        {
            var stringProperty = propertyData.CreateWritableClone() as PropertyString;
            stringProperty.Value = ConverterAddedprefix + stringProperty.ToString();
            return new StringPropertyModel(stringProperty);
        }

        public bool HasPropertyModelAssociatedWith(EPiServer.Core.PropertyData propertyData) => HandledPropertyName.Equals(propertyData.Name, StringComparison.OrdinalIgnoreCase);
    }
}
