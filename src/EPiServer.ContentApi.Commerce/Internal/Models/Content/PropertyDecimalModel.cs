using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyDecimal"/>
    /// </summary>
    internal class PropertyDecimalModel : PropertyModel<decimal?, PropertyDecimal>
    {
        public PropertyDecimalModel(PropertyDecimal type) : base(type)
        {
            Value = type.Decimal;
        }
    }
}
