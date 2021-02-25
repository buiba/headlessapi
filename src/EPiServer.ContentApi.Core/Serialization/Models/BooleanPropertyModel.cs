using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyBoolean"/>
    /// </summary>
    public class BooleanPropertyModel : PropertyModel<bool?, PropertyBoolean>
    {
        [JsonConstructor]
        internal BooleanPropertyModel()
        {
        }

        public BooleanPropertyModel(PropertyBoolean propertyBoolean) : base(propertyBoolean)
        {
            Value = propertyBoolean.Boolean;
        }

    }
}
