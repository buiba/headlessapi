using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyIntegerList"/>
    /// </summary>
    public class IntegerListPropertyModel : PropertyModel<IList<int>, PropertyIntegerList>
    {
        [JsonConstructor]
        internal IntegerListPropertyModel() { }

        public IntegerListPropertyModel(PropertyIntegerList propertyIntegerList) : base(propertyIntegerList)
        {
            Value = propertyIntegerList.List;
        }
    }
}
