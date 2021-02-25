using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyBlob"/>
    /// </summary>
    public class BlobPropertyModel : PropertyModel<string, PropertyBlob>
    {
        [JsonConstructor]
        internal BlobPropertyModel()
        {
        }

        public BlobPropertyModel(PropertyBlob propertyBlob) : base(propertyBlob)
        {
            Value = propertyBlob.ToString();
        }
    }
}
