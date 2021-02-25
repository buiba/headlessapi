using EPiServer.ContentApi.Core.ContentResult;
using Microsoft.Extensions.Internal;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EPiServer.ContentApi.Core.ContentResult.Internal
{
    /// <summary>
    /// Currently NullValueHandling only effects properties, not values in arrays or dictionaries. It seem this is the expected behavior of Newtonsoft.Json 
    /// and will not be fixed (according to this https://github.com/JamesNK/Newtonsoft.Json/issues/1491).
    /// So we need this class to remove the null entry of Dictionary when serialize object
    /// </summary>
    internal class NullCleaningContentSerializer : IContentApiSerializer
    {        
        private ConcurrentDictionary<Type, Func<object, IEnumerable<object>>> _propertyAccessorDictionary = new ConcurrentDictionary<Type, Func<object, IEnumerable<object>>>();
        private readonly IContentApiSerializer _defaultSerializer;

        public string MediaType => _defaultSerializer.MediaType;

        public Encoding Encoding => _defaultSerializer.Encoding;

        public NullCleaningContentSerializer(IContentApiSerializer defaultSerializer)
        {
            _defaultSerializer = defaultSerializer;
        }

       
        public string Serialize(object value)
        {
            RemoveNullValues(value);
            return _defaultSerializer.Serialize(value);
        }

        private void RemoveNullValues(object value)
        {
            if (!(value is object) || !IsKnownType(value.GetType())) return;

            if (value is IDictionary<string, object> dictionary)
            {
                RemoveNullValuesInDictionary(dictionary);
                return;
            }

            foreach (var propertyValue in (value as IEnumerable) ?? _propertyAccessorDictionary.GetOrAdd(value.GetType(), ResolvePropertyValues)(value))
            {
                if (propertyValue is IDictionary<string, object> propertyValueDictionary)
                {
                    RemoveNullValuesInDictionary(propertyValueDictionary);
                }
                else
                {
                    RemoveNullValues(propertyValue);
                }
            }
        }

        private void RemoveNullValuesInDictionary(IDictionary<string, object> entries)
        {
            var keysToRemove = new List<string>();
            foreach (var entry in entries)
            {
                if (!(entry.Value is object) || (entry.Value is string stringValue && stringValue == string.Empty))
                {
                    keysToRemove.Add(entry.Key);
                }
                else
                {
                    RemoveNullValues(entry.Value);                                        
                }
            }

            foreach (var key in keysToRemove)
            {
                entries.Remove(key);
            }
        }

        private Func<object, IEnumerable<object>> ResolvePropertyValues(Type type)
        {
            var propertyAccessors = type.GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0 && p.PropertyType != type)
                .Select(p => ObjectMethodExecutor.Create(p.GetMethod, p.ReflectedType.GetTypeInfo()));

            Func<object, IEnumerable<object>> PropertyAccessors = (value) =>
            {
                return propertyAccessors.Any() ?
                    propertyAccessors.Select(p => p.Execute(value, new object[0])) :
                    Enumerable.Empty<object>();
            };

            return PropertyAccessors;
        }

        private bool IsKnownType(Type type) => typeof(IEnumerable).IsAssignableFrom(type) || type.Assembly.GetName().Name.StartsWith("EPiServer.ContentApi");
    }
}
