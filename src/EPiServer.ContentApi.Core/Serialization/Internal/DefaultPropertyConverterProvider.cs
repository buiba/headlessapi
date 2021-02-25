using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    [ServiceConfiguration(typeof(IPropertyConverterProvider), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class DefaultPropertyConverterProvider : IPropertyConverterProvider
    {
        private readonly PropertyModelFactory _propertyModelFactory;
        private readonly IEnumerable<TypeModel> _typeModels;
        private readonly ConcurrentDictionary<Type, IPropertyConverter> _propertyConverters = new ConcurrentDictionary<Type, IPropertyConverter>();

        public DefaultPropertyConverterProvider(PropertyModelFactory propertyModelFactory)
        {
            _propertyModelFactory = propertyModelFactory;
            _typeModels = TypeModelResolver.ResolveModelTypes();
        }

        public int SortOrder => 0;

        public IPropertyConverter Resolve(PropertyData propertyData)
        {
            if (propertyData == null)
            {
                return null;
            }
            return _propertyConverters.GetOrAdd(propertyData.GetType(), t =>
            {
                var modelType = _typeModels.FirstOrDefault(x => x.PropertyType == t);
                if (modelType != null)
                {
                    return new DefaultPropertyConverter(modelType, _propertyModelFactory);
                }
                return null;
            });
        }
    }
}
