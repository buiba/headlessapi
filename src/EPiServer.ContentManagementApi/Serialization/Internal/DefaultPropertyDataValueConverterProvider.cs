using System;
using System.Collections.Generic;
using System.Reflection;
using EPiServer.ContentApi.Core.Serialization;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    internal class DefaultPropertyDataValueConverterProvider : IPropertyDataValueConverterProvider
    {        
        private readonly IEnumerable<IPropertyDataValueConverter> _propertyDataValueConverters;
        private readonly Dictionary<Type, IPropertyDataValueConverter> _mappedConverters = new Dictionary<Type, IPropertyDataValueConverter>();

        public DefaultPropertyDataValueConverterProvider(IEnumerable<IPropertyDataValueConverter> propertyDataValueConverters)
        {         
            _propertyDataValueConverters = propertyDataValueConverters;
        }       

        public IPropertyDataValueConverter Resolve(IPropertyModel propertyModel)
        {
            if (propertyModel == null)
            {
                return null;
            }

            _mappedConverters.TryGetValue(propertyModel.GetType(), out var converter);

            return converter;
        }

        internal void RegisterConverters()
        {
            foreach (var converter in _propertyDataValueConverters)
            {
                var attributes = converter.GetType().GetCustomAttribute<PropertyDataValueConverterAttribute>();
                foreach (var registerModel in attributes?.PropertyModelTypes)
                {                   
                    _mappedConverters[registerModel] = converter;
                }
            }
        }        
    }
}
