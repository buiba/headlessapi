using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class ContentTypeValidator
    {
        private readonly PropertyDataTypeResolver _propertyDataTypeResolver;
        private readonly ITabDefinitionRepository _tabDefinitionRepository;

        public ContentTypeValidator(
            PropertyDataTypeResolver propertyDataTypeResolver,
            ITabDefinitionRepository tabDefinitionRepository)
        {
            _propertyDataTypeResolver = propertyDataTypeResolver ?? throw new ArgumentNullException(nameof(propertyDataTypeResolver));
            _tabDefinitionRepository = tabDefinitionRepository ?? throw new ArgumentNullException(nameof(tabDefinitionRepository));
        }

        internal IEnumerable<ValidationResult> Validate(IEnumerable<ExternalContentType> contentTypes)
        {
            var validateUniqueness = ValidateUniqueness(contentTypes);
            if (validateUniqueness != ValidationResult.Success)
            {
                return new[] { validateUniqueness };
            }

            foreach (var contentType in contentTypes)
            {
                var visitedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var property in contentType.Properties)
                {
                    if (property is null)
                    {
                        return new[] { new ValidationResult($"Property of '{contentType.Name}' cannot be null") };
                    }

                    if (visitedProperties.Contains(property.Name))
                    {
                        return new[] { new ValidationResult($"Multiple properties with name '{property.Name}' found.") };
                    }

                    visitedProperties.Add(property.Name);
                    var dataType = property.DataType;
                    if (ExternalPropertyDataType.IsBlock(dataType.Type) && string.IsNullOrEmpty(dataType.ItemType))
                    {
                        return new[] { new ValidationResult($"{nameof(ExternalPropertyDataType.ItemType)} must be specified when the {nameof(DataType)} is '{dataType.Type}'") };
                    }

                    try
                    {
                        if (!IsBlockPropertyInBatch(dataType, contentTypes) && _propertyDataTypeResolver.Resolve(dataType) is null)
                        {
                            var msg = $"No registered property could be resolved from property data type with {nameof(DataType)} '{dataType.Type}'";
                            if (!string.IsNullOrEmpty(dataType.ItemType))
                            {
                                msg += $" and {nameof(ExternalPropertyDataType.ItemType)} '{dataType.ItemType}'";
                            }

                            return new[] { new ValidationResult(msg) };
                        }
                    }
                    catch (MultipleDefinitionMatchedException)
                    {
                        return new[] { new ValidationResult($"Could not uniquely resolve a list property from property data type with {nameof(DataType)} '{dataType.Type}' and {nameof(ExternalPropertyDataType.ItemType)} '{dataType.ItemType}'. Specify {nameof(ExternalPropertyDataType.ItemType)} as Type.FullName,AssemblyName") };
                    }

                    if (property.EditSettings is object && !IsValidPropertyGroup(property.EditSettings.GroupName))
                    {
                        return new[] { new ValidationResult($"The group {property.EditSettings.GroupName} is not valid.") };
                    }
                }
            }

            return Enumerable.Empty<ValidationResult>();
        }

        private bool IsBlockPropertyInBatch(ExternalPropertyDataType dataType, IEnumerable<ExternalContentType> contentTypes)
        {
            if (ExternalPropertyDataType.IsBlock(dataType))
            {
                return contentTypes.Any(c => nameof(ContentTypeBase.Block).Equals(c.BaseType, StringComparison.OrdinalIgnoreCase) && string.Equals(c.Name, dataType.ItemType, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        private ValidationResult ValidateUniqueness(IEnumerable<ExternalContentType> contentTypes)
        {
            var duplicateIds = contentTypes.Where(c => c.Id != Guid.Empty).Select(c => new { c.Id, c.Name }).GroupBy(c => c.Id).Where(c => c.Count() > 1);
            if (duplicateIds.Any())
            {
                return new ValidationResult($"There are more than one content type with same id '{duplicateIds.First().Key}', '{string.Join(",", duplicateIds.First().Select(e => e.Name))}'");
            }

            var duplicateNames = contentTypes.Select(c => new { c.Id, c.Name }).GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Where(c => c.Count() > 1);
            if (duplicateNames.Any())
            {
                return new ValidationResult($"There are more than one content type with same name '{duplicateNames.First().Key}'");
            }

            return ValidationResult.Success;
        }

        private bool IsValidPropertyGroup(string groupName)
        {
            if (!string.IsNullOrWhiteSpace(groupName))
            {
                return _tabDefinitionRepository.Load(groupName) != null;
            }

            return true;
        }
    }
}
