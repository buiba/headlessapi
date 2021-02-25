using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyNumber"/>
    /// </summary>
    public class NumberPropertyModel : PropertyModel<int?, PropertyNumber>
    {
        [JsonConstructor]
        internal NumberPropertyModel() { }

        public NumberPropertyModel(PropertyNumber propertyNumber) : base(propertyNumber)
        {
            Value = propertyNumber.Number;
        }

    }
}
