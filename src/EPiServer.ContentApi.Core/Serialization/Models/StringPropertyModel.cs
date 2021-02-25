using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyString"/>
    /// </summary>
    public class StringPropertyModel : PropertyModel<string, PropertyString>
    {
        [JsonConstructor]
        internal StringPropertyModel() { }

        public StringPropertyModel(PropertyString propertyString) : base(propertyString)
        {
            Value = propertyString.ToString();
        }
    }
}
