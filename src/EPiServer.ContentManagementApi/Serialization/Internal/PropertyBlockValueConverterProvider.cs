using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    /// <summary>
    /// Provider for <see cref="PropertyBlockValueConverter"/>. This provider will be register as singleton service in initialize module.
    /// </summary>
    internal class PropertyBlockValueConverterProvider : IPropertyDataValueConverterProvider
    {
        private IPropertyDataValueConverterResolver _converterResolver;
        private PropertyBlockValueConverter _propertyBlockValueConverter;

        public void Initialize(IPropertyDataValueConverterResolver converterResolver) => _converterResolver = converterResolver;

        public IPropertyDataValueConverter Resolve(IPropertyModel propertyModel)
        {
            if (propertyModel is BlockPropertyModel && _converterResolver is object)
            {                
                if (_propertyBlockValueConverter is null)
                {
                    _propertyBlockValueConverter = new PropertyBlockValueConverter(_converterResolver);
                }
                return _propertyBlockValueConverter;
            }

            return null;
        }
    }
}
