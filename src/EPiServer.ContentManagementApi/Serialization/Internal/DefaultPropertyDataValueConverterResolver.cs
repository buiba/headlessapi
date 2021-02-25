using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    [ServiceConfiguration(typeof(IPropertyDataValueConverterResolver), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class DefaultPropertyDataValueConverterResolver : IPropertyDataValueConverterResolver
    {
        private readonly IEnumerable<IPropertyDataValueConverterProvider> _converterProviders;

        public DefaultPropertyDataValueConverterResolver(IEnumerable<IPropertyDataValueConverterProvider> converterProviders)
        {
            _converterProviders = converterProviders;
        }

        public IPropertyDataValueConverter Resolve(IPropertyModel propertyModel)
        {            
            foreach (var provider in _converterProviders)
            {
                var converter = provider.Resolve(propertyModel);
                if (converter is object)
                {
                    return converter;
                }
            }

            return null;
        }
    }
}
