using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertySortOrder"/>
    /// </summary>
    public class SortOrderPropertyModel : PropertyModel<int?, PropertySortOrder>
    {
        [JsonConstructor]
        internal SortOrderPropertyModel() { }

        public SortOrderPropertyModel(PropertySortOrder propertySortOrder) : base(propertySortOrder)
        {
            Value = propertySortOrder.Number;
        }
    }
}
