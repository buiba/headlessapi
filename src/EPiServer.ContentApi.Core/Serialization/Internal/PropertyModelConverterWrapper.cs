using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
#pragma warning disable CS0618 //Wraps obsolete
    internal class PropertyModelConverterWrapper : IPropertyConverterProvider, IPropertyConverter
    {
        private readonly IPropertyModelConverter _propertyModelConverter;

        public PropertyModelConverterWrapper(IPropertyModelConverter propertyModelConverter)
        {
            _propertyModelConverter = propertyModelConverter;
        }

        public int SortOrder => _propertyModelConverter.SortOrder;

        public IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext) 
            => _propertyModelConverter.ConvertToPropertyModel(propertyData, contentMappingContext.Language, contentMappingContext.ExcludePersonalizedContent, contentMappingContext.ShouldExpand(propertyData.Name));

        public IPropertyConverter Resolve(PropertyData propertyData) => _propertyModelConverter.HasPropertyModelAssociatedWith(propertyData) ? this : null;
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
