using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    [ServiceConfiguration(typeof(IPropertyConverterResolver), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class DefaultPropertyConverterResolver : IPropertyConverterResolver
    {
        private readonly List<IPropertyConverterProvider> _propertyConverterProviders;
#pragma warning disable CS0618 // Type or member is obsolete
        public DefaultPropertyConverterResolver(IEnumerable<IPropertyModelConverter> legacyConverters, IEnumerable<IPropertyConverterProvider> propertyConverterProviders)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            var nonSystemLegacyConverters = legacyConverters.Where(lm => !lm.GetType().Assembly.GetName().Name.StartsWith("EPiServer.ContentApi"));
            _propertyConverterProviders = (nonSystemLegacyConverters != null ? 
                    ((IEnumerable<IPropertyConverterProvider>)nonSystemLegacyConverters.Select(m => new PropertyModelConverterWrapper(m))).Concat(propertyConverterProviders) :
                    propertyConverterProviders)
                    .OrderByDescending(p => p.SortOrder)
                    .ToList();
        }


        public IPropertyConverter Resolve(PropertyData propertyData)
        {
            foreach (var provider in _propertyConverterProviders)
            {
                var converter = provider.Resolve(propertyData);
                if (converter != null)
                {
                    return converter;
                }
            }

            return null;
        }
    }
}
