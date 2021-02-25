using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;
using EPiServer.SpecializedProperties;
using EPiServer.Validation;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json.Linq;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class ContentTypeMapper
    {
        private static readonly HashSet<string> KnownValidationSettingsProperties = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(IPropertyValidationSettings.Id),
            nameof(IPropertyValidationSettings.Severity),
            nameof(IPropertyValidationSettings.ErrorMessage)
        };

        private readonly PropertyDataTypeResolver _propertyDataTypeResolver;
        private readonly IPropertyValidationSettingsRepository _validationSettingsRepository;
        private readonly ITabDefinitionRepository _tabDefinitionRepository;

        protected ContentTypeMapper() { }

        public ContentTypeMapper(PropertyDataTypeResolver propertyDataTypeResolver, IPropertyValidationSettingsRepository validationSettingsRepository, ITabDefinitionRepository tabDefinitionRepository)
        {
            _propertyDataTypeResolver = propertyDataTypeResolver;
            _validationSettingsRepository = validationSettingsRepository;
            _tabDefinitionRepository = tabDefinitionRepository;
        }

        public virtual void MapToExternal(ContentType source, ExternalContentType target)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            target.Id = source.GUID;
            target.Name = source.Name;
            target.BaseType = source.Base.ToString();
            target.Version = source.Version?.ToString();
            target.EditSettings = ResolveExternalContentTypeEditSettings(source);
            target.Properties = source.PropertyDefinitions.Select(FromPropertyDefinition).ToArray();
        }

        private static ExternalContentTypeEditSettings ResolveExternalContentTypeEditSettings(ContentType contentType)
        {
            return contentType.DisplayName == default &&
                contentType.Description == default &&
                contentType.IsAvailable == default &&
                contentType.SortOrder == default
                ? null
                : new ExternalContentTypeEditSettings(contentType.DisplayName?.Trim(), contentType.Description?.Trim(), contentType.IsAvailable, contentType.SortOrder);
        }

        private ExternalProperty FromPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            var dataType = _propertyDataTypeResolver.ToExternalPropertyDataType(propertyDefinition.Type);
            var externalProperty = new ExternalProperty
            {
                Name = propertyDefinition.Name,
                DataType = dataType,
                BranchSpecific = propertyDefinition.LanguageSpecific,
                EditSettings = ResolveExternalPropertyEditSettings(propertyDefinition)
            };

            AssignExternalPropertyValidationSettings(propertyDefinition, externalProperty);

            return externalProperty;
        }

        private static ExternalPropertyEditSettings ResolveExternalPropertyEditSettings(PropertyDefinition property)
        {
            var visibilityStatus = property.DisplayEditUI ? VisibilityStatus.Default : VisibilityStatus.Hidden;
            var editorHint = property.Type.DefinitionType == typeof(PropertyUrl) ? nameof(Url) : property.EditorHint;

            return new ExternalPropertyEditSettings(visibilityStatus, property.EditCaption, property.Tab.Name, property.FieldOrder, property.HelpText, editorHint);
        }

        private void AssignExternalPropertyValidationSettings(PropertyDefinition propertyDefinition, ExternalProperty externalProperty)
        {
            foreach (var settings in _validationSettingsRepository.List(propertyDefinition))
            {
                var externalSettings = new ExternalPropertyValidationSettings
                {
                    Name = GetValidationSettingName(settings, propertyDefinition),
                    Severity = settings.Severity,
                    ErrorMessage = settings.ErrorMessage,
                };

                foreach (var propertyHelper in PropertyHelper.GetVisibleProperties(settings.GetType()))
                {
                    if (KnownValidationSettingsProperties.Contains(propertyHelper.Name))
                    {
                        continue;
                    }

                    var value = propertyHelper.GetValue(settings);
                    if (value is object)
                    {
                        externalSettings.Settings.Add(propertyHelper.Name, JToken.FromObject(value));
                    }
                }

                externalProperty.Validation.Add(externalSettings);
            }
        }

        private static string GetValidationSettingName(IPropertyValidationSettings settings, PropertyDefinition propertyDefinition)
        {
            var settingsType = settings.GetType();
            var attribute = settingsType.GetCustomAttributes<PropertyValidationSettingsAttribute>(inherit: false)
                               .FirstOrDefault(x => x.DataType.IsAssignableFrom(propertyDefinition.Type.DefinitionType));

            return attribute?.Name ?? settingsType.Name;
        }


        public virtual void MapToInternal(ExternalContentType source, ContentType target)
        {
            if (target is object)
            {
                target.Name = source.Name;
                target.Base = (ContentTypeBase)source.BaseType;
                target.Version = source.Version is object ? new Version(source.Version) : target.Version;

                if (source.EditSettings is ExternalContentTypeEditSettings externalContentTypeEditSettings)
                {
                    target.DisplayName = externalContentTypeEditSettings.DisplayName?.Trim();
                    target.Description = externalContentTypeEditSettings.Description?.Trim();
                    target.IsAvailable = externalContentTypeEditSettings.Available;
                    target.SortOrder = externalContentTypeEditSettings.Order;
                }
                else
                {
                    target.DisplayName = default;
                    target.Description = default;
                    target.IsAvailable = default;
                    target.SortOrder = default;
                }

                // remove internal properties that does not exist in the external content type
                var removedProperties = target.PropertyDefinitions.Where(p => !source.Properties.Any(e => string.Equals(e.Name, p.Name, StringComparison.OrdinalIgnoreCase))).ToList();
                foreach (var property in removedProperties)
                {
                    target.PropertyDefinitions.Remove(property);
                }

                AssignProperties(target, source);
            }
        }

        private void AssignProperties(ContentType internalContentType, ExternalContentType contentType)
        {
            foreach (var property in contentType.Properties)
            {
                var internalProperty = internalContentType.PropertyDefinitions.FirstOrDefault(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)) ??
                    new PropertyDefinition { Name = property.Name, ContentTypeID = internalContentType.ID };

                if (internalProperty.ID == 0)
                {
                    internalContentType.PropertyDefinitions.Add(internalProperty);
                }

                internalProperty.Name = property.Name;
                internalProperty.Type = _propertyDataTypeResolver.Resolve(property.DataType);
                internalProperty.LanguageSpecific = property.BranchSpecific;
                if (property.EditSettings is object)
                {
                    internalProperty.EditorHint = property.EditSettings.Hint;
                    internalProperty.DisplayEditUI = property.EditSettings.Visibility == VisibilityStatus.Default ? true : false;
                    internalProperty.EditCaption = property.EditSettings.DisplayName;
                    internalProperty.FieldOrder = property.EditSettings.Order;
                    internalProperty.HelpText = property.EditSettings.HelpText;
                    if (!string.IsNullOrEmpty(property.EditSettings.GroupName))
                    {
                        internalProperty.Tab = _tabDefinitionRepository.Load(property.EditSettings.GroupName);
                    }
                }
            }
        }

    }

}
