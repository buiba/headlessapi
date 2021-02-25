using EPiServer.Framework;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    public class DropDownListPropertyModel : PropertyModel<string, PropertyDropDownList>
    {
        [JsonConstructor]
        internal DropDownListPropertyModel() { }

        public DropDownListPropertyModel(PropertyDropDownList propertyDropDownList) : base(propertyDropDownList)
        {
            Value = propertyDropDownList.ToString();
        }
    }
}
