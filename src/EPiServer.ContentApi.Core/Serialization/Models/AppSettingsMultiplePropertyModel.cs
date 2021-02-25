using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyAppSettingsMultiple"/>
    /// </summary>
    public class AppSettingsMultiplePropertyModel : PropertyModel<IEnumerable<string>, PropertyAppSettingsMultiple>
    {
        [JsonConstructor]
        internal AppSettingsMultiplePropertyModel()
        {
        }

        public AppSettingsMultiplePropertyModel(PropertyAppSettingsMultiple propertyAppSettingsMultiple) : base(propertyAppSettingsMultiple)
        {
            Value = string.IsNullOrEmpty(propertyAppSettingsMultiple.ToString()) ? new List<string>() :
                propertyAppSettingsMultiple.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
