using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;

namespace EPiServer.DefinitionsApi.PropertyDataTypes.Internal
{
    /// <summary>
    /// This class is intended to be used internally by EPiServer. We do not support any backward compatibility on this.
    /// </summary>
    public class PropertyDataTypeResolver
    {
        private readonly IPropertyDefinitionTypeRepository _propertyDefinitionTypeRepository;

        private readonly ConcurrentDictionary<string, ExternalPropertyDataType> _internalToExternal = new ConcurrentDictionary<string, ExternalPropertyDataType>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<ExternalPropertyDataType, PropertyDefinitionType> _externalToInternal = new ConcurrentDictionary<ExternalPropertyDataType, PropertyDefinitionType>();

        //for test
        protected PropertyDataTypeResolver() { }

        public PropertyDataTypeResolver(IPropertyDefinitionTypeRepository propertyDefinitionTypeRepository)
        {
            _propertyDefinitionTypeRepository = propertyDefinitionTypeRepository ?? throw new ArgumentNullException(nameof(propertyDefinitionTypeRepository));
        }

        public void ClearCaches()
        {
            _internalToExternal.Clear();
            _externalToInternal.Clear();
        }

        public virtual IEnumerable<ExternalPropertyDataType> List() => _propertyDefinitionTypeRepository.List().Select(p => ToExternalPropertyDataType(p));

        public virtual PropertyDefinitionType Resolve(ExternalPropertyDataType dataType) => _externalToInternal.GetOrAdd(dataType, _ =>
        {
            if (ExternalPropertyDataType.IsBlock(dataType))
            {
                return _propertyDefinitionTypeRepository.List()
                    .FirstOrDefault(pdt => pdt.DataType == PropertyDataType.Block && pdt.Name.Equals(dataType.ItemType, StringComparison.OrdinalIgnoreCase));
            }

            // Store non-match in lookup as 'null'
            return _propertyDefinitionTypeRepository.List().FirstOrDefault(pdt => pdt.DefinitionType.Name.Equals(dataType.Type, StringComparison.OrdinalIgnoreCase));
        });

        public virtual ExternalPropertyDataType ToExternalPropertyDataType(PropertyDefinitionType propertyDefinitionType) => _internalToExternal.GetOrAdd(propertyDefinitionType.Name, _ =>
        {
            if (propertyDefinitionType.DataType == PropertyDataType.Block)
            {
                return ExternalPropertyDataType.Block(propertyDefinitionType.Name);
            }

            return new ExternalPropertyDataType(propertyDefinitionType.DefinitionType.Name);
        });
    }
}
