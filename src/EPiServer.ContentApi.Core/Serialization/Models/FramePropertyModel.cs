using EPiServer.Framework;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyFrame"/>
    /// </summary>
    public class FramePropertyModel : PropertyModel<string, PropertyFrame>
    {
        [JsonConstructor]
        internal FramePropertyModel() { }

        public FramePropertyModel(PropertyFrame propertyFrame) : base(propertyFrame)
        {
            Value = propertyFrame.ToString();
        }
    }
}
