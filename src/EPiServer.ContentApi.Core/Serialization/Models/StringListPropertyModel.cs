using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyStringList"/>
    /// </summary>
    public class StringListPropertyModel : PropertyModel<IList<string>, PropertyStringList>
    {
        [JsonConstructor]
        internal StringListPropertyModel() { }

        public StringListPropertyModel(PropertyStringList propertyStringList) : base(propertyStringList)
        {
            Value = propertyStringList.List;
        }
    }
}
