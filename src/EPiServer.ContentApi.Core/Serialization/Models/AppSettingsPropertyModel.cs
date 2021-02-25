using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyAppSettings"/>
    /// </summary>
    public class AppSettingsPropertyModel : PropertyModel<string, PropertyAppSettings>
    {
        [JsonConstructor]
        internal AppSettingsPropertyModel()
        {
        }

        public AppSettingsPropertyModel(PropertyAppSettings propertyAppSettings) : base(propertyAppSettings)
        {
            Value = propertyAppSettings.ToString();
        }
    }
}
