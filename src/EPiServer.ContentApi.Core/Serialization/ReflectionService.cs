using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Service handle object creation and reflection
    /// </summary>
    [ServiceConfiguration(typeof(ReflectionService))]
    public class ReflectionService
    {
        protected readonly ContentTypeModelRepository _contentTypeModelRepository;
        //Safe to cache Type attributes since they cant change without recompilation
        private ConcurrentDictionary<string, IEnumerable<Attribute>> _propertyAttributesCache = new ConcurrentDictionary<string, IEnumerable<Attribute>>();

        /// <internal-api/> //for easier mock in test
        protected ReflectionService() { }

        /// <summary>
        /// Initialize an instance of relection service
        /// </summary>
        /// <param name="contentTypeModelRepository"></param>
        public ReflectionService(ContentTypeModelRepository contentTypeModelRepository)
        {
            _contentTypeModelRepository = contentTypeModelRepository;
        }

        /// <summary>
        /// Create a instance of given type with parameters
        /// </summary>        
        public virtual object CreateInstance(Type type, params object[] args)
        {
            return (type == null) ? null : Activator.CreateInstance(type, args);            
        }

        /// <summary>
        /// Get attributes of an Episerver property from content type
        /// </summary>
        public virtual IEnumerable<Attribute> GetAttributes(ContentType contentType, PropertyData prop)
        {
            return _propertyAttributesCache.GetOrAdd($"{contentType.Name}_{prop.Name}", key =>
            {
                var propertyDefinition = contentType.PropertyDefinitions.SingleOrDefault(pd => string.Equals(pd.Name, prop.Name, StringComparison.Ordinal));

                // In case propertyData is not existed in contentType's propertyDefinitions (i.e property data is defined in base classed of 
                // Episerver.Core package: PageData, ContentData, ...), we try to get Attributes directly from System.Type of given contentType
                if (propertyDefinition == null)
                {
                    return GetAttributesFromType(contentType.ModelType, prop.Name);
                }

                var propertyDefinitionModel = _contentTypeModelRepository.GetPropertyModel(contentType.ID, propertyDefinition);

                if (propertyDefinitionModel == null)
                {
                    return GetAttributesFromType(contentType.ModelType, prop.Name);
                }

                return propertyDefinitionModel.Attributes.GetAllAttributes<Attribute>();
            });           
        }

        /// <summary>
        /// Get Attributes from model type
        /// </summary>
        protected virtual IEnumerable<Attribute> GetAttributesFromType(Type modelType, string propertyName)
        {
            if (modelType == null)
            {
                return null;
            }

            var propertyInfo = modelType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                propertyInfo = GetPropertyInfoFromInterface(modelType, propertyName);
            }

            if (propertyInfo != null)
            {
                return propertyInfo.GetCustomAttributes(true).Cast<Attribute>();
            }

            return null;
        }

        /// <summary>
        /// Get property info from interface
        /// </summary>
        protected virtual PropertyInfo GetPropertyInfoFromInterface(Type modelType, string propertyName)
        {
            var interfaceAndPropertyName = propertyName.Split('_');

            if (interfaceAndPropertyName.Length == 2)
            {
                var interfaceType = modelType.GetInterface(interfaceAndPropertyName[0], true);

                if (interfaceType != null)
                {
                    return modelType.GetProperty(interfaceAndPropertyName[1], BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                }
            }

            return null;
        }
    }
}
