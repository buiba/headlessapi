using EPiServer.Core;
using EPiServer.Framework;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyFloatNumber"/>
    /// </summary>
    public class FloatPropertyModel : PropertyModel<double?, PropertyFloatNumber>
    {
        [JsonConstructor]
        internal FloatPropertyModel() { }

        public FloatPropertyModel(PropertyFloatNumber propertyFloatNumber) : base(propertyFloatNumber)
        {
            Value = propertyFloatNumber.FloatNumber;
        }
    }
}
