using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyLongString"/>
    /// </summary>
    public class LongStringPropertyModel : PropertyModel<string, PropertyLongString>
    {
        [JsonConstructor]
        internal LongStringPropertyModel() { }

        public LongStringPropertyModel(PropertyLongString propertyLongString) : base(propertyLongString)
        {
            Value = propertyLongString.ToString();
        }
    }
}
