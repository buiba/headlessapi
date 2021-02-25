using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyEmailAddress"/>
    /// </summary>
    internal class PropertyEmailAddressModel : PropertyModel<string, PropertyEmailAddress>
    {
        public PropertyEmailAddressModel(PropertyEmailAddress propertyEmailAddress) : base(propertyEmailAddress)
        {
            Value = propertyEmailAddress.ToString();
        }
    }
}
