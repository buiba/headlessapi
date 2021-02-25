using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.Validation;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json.Linq;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class PropertyValidationContentTypeServiceExtension : IExternalContentTypeServiceExtension
    {
        private readonly IPropertyValidationSettingsRepository _settingsRepository;
        private readonly IPropertyValidationDefinitionProvider _definitionProvider;

        public PropertyValidationContentTypeServiceExtension(
            IPropertyValidationSettingsRepository propertyValidationSettingsRepository,
            IPropertyValidationDefinitionProvider propertyValidationDefinitionProvider)
        {
            _settingsRepository = propertyValidationSettingsRepository ?? throw new ArgumentNullException(nameof(propertyValidationSettingsRepository));
            _definitionProvider = propertyValidationDefinitionProvider ?? throw new ArgumentNullException(nameof(propertyValidationDefinitionProvider));
        }

        public void Save(IEnumerable<ExternalContentType> externalContentTypes, IEnumerable<ContentType> internalContentTypes, ContentTypeSaveOptions contentTypeSaveOptions)
        {
            foreach (var contentType in externalContentTypes.Zip(internalContentTypes, (e, i) => (External: e, Internal: i)))
            {
                foreach (var property in contentType.External.Properties)
                {
                    var propertyDefinition = contentType.Internal.PropertyDefinitions.First(x => x.Name == property.Name);

                    _settingsRepository.Assign(propertyDefinition, ConvertValidationSettings(propertyDefinition, property.Validation));
                }
            }
        }

        internal IEnumerable<IPropertyValidationSettings> ConvertValidationSettings(PropertyDefinition propertyDefinition, IList<ExternalPropertyValidationSettings> externalValidations)
        {
            if (externalValidations is null || externalValidations.Count == 0)
            {
                yield break;
            }

            foreach (var validation in externalValidations)
            {
                var definition = _definitionProvider.Get(validation.Name, propertyDefinition.Type.DefinitionType);

                if (definition is null)
                {
                    continue;
                }

                var settings = (IPropertyValidationSettings)Activator.CreateInstance(definition.SettingsType);

                settings.Severity = validation.Severity;
                settings.ErrorMessage = validation.ErrorMessage;

                AssignSettingsProperties(definition.SettingsType, settings, validation.Settings);

                yield return settings;
            }
        }

        private void AssignSettingsProperties(Type settingsType, IPropertyValidationSettings instance, IDictionary<string, JToken> properties)
        {
            foreach (var propertyHelper in PropertyHelper.GetVisibleProperties(settingsType))
            {
                if (properties.TryGetValue(propertyHelper.Name, out var value))
                {
                    propertyHelper.SetValue(instance, value.ToObject(propertyHelper.Property.PropertyType));
                }
            }
        }

        public bool TryDelete(Guid id)
        {
            return false;
        }
    }
}
