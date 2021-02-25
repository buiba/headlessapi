using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    /// <summary>
    /// Resolve the <see cref="TypeModel"/> from given property data type
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    [ServiceConfiguration(typeof(TypeModelResolver), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TypeModelResolver
    {
        private IEnumerable<TypeModel> _typeModels;
        
        public virtual TypeModel Resolve(Type propertyDataType)
        {
            if (_typeModels is null)
            {
                _typeModels = ResolveModelTypes();
            }

            return _typeModels.FirstOrDefault(td => td.PropertyType.Equals(propertyDataType));
        }

        internal static IEnumerable<TypeModel> ResolveModelTypes()
        {
            var modelTypes = new List<TypeModel>();

            // get all concrete classes of IPropertyModel
            var propertyModels = ReflectionHelper.GetConcreteDerivedTypes(typeof(IPropertyModel));

            foreach (var propModel in propertyModels)
            {
                // ignore property model if by intention, it should not be included in default model type registration
                if (propModel.GetInterfaces().Contains(typeof(IExcludeFromModelTypeRegistration)))
                {
                    continue;
                }

                // get all parent generic types e.g: IPropertyModel<>  PropertyModel<,>  PersonalizablePropertyModel<,> etc..
                var baseGenericTypes = ReflectionHelper.GetParentGenericTypes(propModel);

                // only get base generic type  IPropertyModel<>
                var iPropertyModel = baseGenericTypes?.SingleOrDefault(t => string.Equals(t.Name, typeof(IPropertyModel<>).Name, StringComparison.OrdinalIgnoreCase));
                if (iPropertyModel == null)
                {
                    continue;
                }

                // get generic argument, because IPropertyModel<> has only one argument type which is inherited from EPiServer.Core.PropertyData
                var propertyDataProperty = iPropertyModel.GetGenericArguments()[0];

                modelTypes.Add(new TypeModel
                {
                    ModelType = propModel,
                    ModelTypeString = propModel.FullName,
                    PropertyType = propertyDataProperty
                });
            }

            return modelTypes;
        }
    }    
}
