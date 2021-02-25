using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyDoubleList"/>
    /// </summary>
    public class DoubleListPropertyModel : PropertyModel<IList<double>, PropertyDoubleList>
    {
        [JsonConstructor]
        internal DoubleListPropertyModel() { }

        public DoubleListPropertyModel(PropertyDoubleList propertyDoubleList) : base(propertyDoubleList)
        {
            Value = propertyDoubleList.List;
        }
    }
}
