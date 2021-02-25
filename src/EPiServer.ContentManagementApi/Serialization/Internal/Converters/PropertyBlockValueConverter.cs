using System;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="BlockPropertyModel"/> to <see cref="BlockData"/>
    /// </summary>
    internal class PropertyBlockValueConverter : IPropertyDataValueConverter
    {
        private readonly IPropertyDataValueConverterResolver _converterResolver;        

        public PropertyBlockValueConverter(IPropertyDataValueConverterResolver converterResolver)
        {
            _converterResolver = converterResolver;
        }        

        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel == null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is BlockPropertyModel blockPropertyModel))
            {
                throw new NotSupportedException("PropertyBlockValueConverter supports to convert BlockPropertyModel only");
            }

            var cloneProperty = propertyData.CreateWritableClone();
            if (!(cloneProperty is IPropertyBlock propertyBlock))
            {
                throw new NotSupportedException($"{nameof(propertyData)} must be a IPropertyBlock");
            }

            if (propertyBlock.Property is null)
            {
                return propertyBlock.Block;
            }

            foreach (var item in blockPropertyModel.Properties)
            {
                if (!TryGetConverter(item.Value, out var converter))
                {
                    continue;
                }

                var property = propertyBlock.Property.SingleOrDefault(x => string.Equals(x.Name, item.Key, StringComparison.OrdinalIgnoreCase));
                if (property is object)
                {
                    property.Value = converter.Convert(item.Value as IPropertyModel, property);
                }
            }

            return propertyBlock.Block;
        }

        private bool TryGetConverter(object item, out IPropertyDataValueConverter converter)
        {
            converter = null;
            if (item is IPropertyModel itemValuePropertyModel)
            {
                converter = _converterResolver.Resolve(itemValuePropertyModel);
            }

            return converter is object;
        }
    }
}
