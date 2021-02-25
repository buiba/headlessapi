using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyDictionarySingle"/>
    /// </summary>
    internal class PropertyDictionarySingleModel : PropertyModel<string, PropertyDictionarySingle>
    {
        public PropertyDictionarySingleModel(PropertyDictionarySingle propertyDictionarySingle) : base(propertyDictionarySingle)
        {
            Value = propertyDictionarySingle.ToString();
        }
    }
}
