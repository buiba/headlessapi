using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyLanguage"/>
    /// </summary>
    public class LanguagePropertyModel : PropertyModel<string, PropertyLanguage>
    {
        [JsonConstructor]
        internal LanguagePropertyModel() { }

        public LanguagePropertyModel(PropertyLanguage propertyLanguage) : base(propertyLanguage)
        {
            Value = propertyLanguage.ToString();
        }
    }
}
