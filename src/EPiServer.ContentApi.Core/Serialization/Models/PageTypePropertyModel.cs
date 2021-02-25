using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyPageType"/>
    /// </summary>
    public class PageTypePropertyModel : PropertyModel<string, PropertyPageType>
    {
        [JsonConstructor]
        internal PageTypePropertyModel() { }

        public PageTypePropertyModel(PropertyPageType propertyPageType) : base(propertyPageType)
        {
            Value = propertyPageType.PageTypeName;
        }
    }
}
