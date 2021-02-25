using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class ExternalPropertyJsonConverter : JsonConverter<ExternalProperty>
    {
        private const string Name = "name";
        private const string DataType = "dataType";
        private const string ItemType = "itemType";
        private const string BranchSpecific = "branchSpecific";
        private const string EditSettings = "editSettings";
        private const string Validation = "validation";

        public override ExternalProperty ReadJson(JsonReader reader, Type objectType, ExternalProperty existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            if (jsonObject.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var nameProperty))
            {
                var dataTypeProperty = jsonObject.GetValue(DataType, StringComparison.OrdinalIgnoreCase);

                string itemType = null;
                if (jsonObject.TryGetValue(ItemType, StringComparison.OrdinalIgnoreCase, out var itemTypeProperty))
                {
                    itemType = (string)itemTypeProperty;
                }

                var branchSpecific = false;
                if (jsonObject.TryGetValue(BranchSpecific, StringComparison.OrdinalIgnoreCase, out var branchSpecificProperty))
                {
                    branchSpecific = (bool)branchSpecificProperty;
                }

                var editSettings = default(ExternalPropertyEditSettings);
                if (jsonObject.TryGetValue(EditSettings, StringComparison.OrdinalIgnoreCase, out var editSettingsProperty))
                {
                    editSettings = editSettingsProperty.ToObject<ExternalPropertyEditSettings>(serializer);
                    if (!Enum.IsDefined(typeof(VisibilityStatus), editSettings.Visibility))
                    {
                        editSettings.Visibility = VisibilityStatus.Default;
                    }

                    editSettings.DisplayName = editSettings.DisplayName?.Trim();
                    editSettings.GroupName = editSettings.GroupName?.Trim();
                    editSettings.HelpText = editSettings.HelpText?.Trim();
                    editSettings.Hint = editSettings.Hint?.Trim();
                }

                var externalProperty = new ExternalProperty
                {
                    Name = (string)nameProperty,
                    DataType = new ExternalPropertyDataType((string)dataTypeProperty, itemType),
                    BranchSpecific = branchSpecific,
                    EditSettings = editSettings,
                };

                if (jsonObject.TryGetValue(Validation, StringComparison.OrdinalIgnoreCase, out var validationProperty) && validationProperty is JArray validationArray)
                {
                    foreach (var validationSettings in validationArray)
                    {
                        externalProperty.Validation.Add(validationSettings.ToObject<ExternalPropertyValidationSettings>(serializer));
                    }
                }

                return externalProperty;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, ExternalProperty value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(Name, false);
            writer.WriteValue(value.Name);
            writer.WritePropertyName(DataType);
            writer.WriteValue(value.DataType.Type);

            if (!string.IsNullOrWhiteSpace(value.DataType.ItemType))
            {
                writer.WritePropertyName(ItemType);
                writer.WriteValue(value.DataType.ItemType);
            }

            writer.WritePropertyName(BranchSpecific);
            writer.WriteValue(value.BranchSpecific);

            if (value.EditSettings is object)
            {
                writer.WritePropertyName(EditSettings);
                JToken.FromObject(value.EditSettings, serializer).WriteTo(writer);
            }

            if (value.Validation.Count > 0)
            {
                writer.WritePropertyName(Validation);
                writer.WriteStartArray();
                foreach (var validationSettings in value.Validation)
                {
                    JToken.FromObject(validationSettings, serializer).WriteTo(writer);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
