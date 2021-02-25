using System.Collections.Generic;
using System.Globalization;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentApi.Commerce.Internal
{
    //plan is to obsolete this partial class as [Obsolete("Replaced by IContentConverter")]

    /// <summary>
    /// This class is used for handling the mapping between EPiServer property SeoInformation to SeoInformationPropertyModel.
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyModelConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class SeoInformationPropertyModelConverter : IPropertyConverter, IPropertyModelConverter
    {
        /// <inheritdoc />  
        public IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext) => new SeoInformationBlockPropertyModel((PropertyBlock)propertyData);

        /// <inheritdoc />      
        public bool HasPropertyModelAssociatedWith(PropertyData propertyData)
        {
            var blockPropertyData = propertyData as PropertyBlock;
            return blockPropertyData?.Value is SeoInformation;
        }

        /// <inheritdoc />      
        public IPropertyModel ConvertToPropertyModel(PropertyData propertyData, CultureInfo language, bool excludePersonalizedContent, bool expand)
        {
            return new SeoInformationBlockPropertyModel((PropertyBlock)propertyData);
        }

        /// <inheritdoc />      
        public int SortOrder => 200;

        /// <inheritdoc />      
        public IEnumerable<TypeModel> ModelTypes => new List<TypeModel>();
    }
}
