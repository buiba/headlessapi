using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyImageUrl"/>
    /// </summary>
    public class ImageUrlPropertyModel : PropertyModel<string, PropertyImageUrl>
    {
        [JsonConstructor]
        internal ImageUrlPropertyModel() { }

        public ImageUrlPropertyModel(PropertyImageUrl propertyImageUrl) : base(propertyImageUrl)
        {
            Value = propertyImageUrl.ToString();
        }
    }
}
