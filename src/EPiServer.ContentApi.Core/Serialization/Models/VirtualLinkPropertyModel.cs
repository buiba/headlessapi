using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyVirtualLink"/>
    /// </summary>
    public class VirtualLinkPropertyModel : PropertyModel<string, PropertyVirtualLink>
    {
        [JsonConstructor]
        internal VirtualLinkPropertyModel() { }

        public VirtualLinkPropertyModel(PropertyVirtualLink propertyVirtualLink) : base(propertyVirtualLink)
        {
            Value = propertyVirtualLink.ToString();
        }
    }
}
