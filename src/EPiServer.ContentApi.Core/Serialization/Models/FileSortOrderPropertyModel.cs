using EPiServer.Framework;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyFileSortOrder"/>
    /// </summary>
    public class FileSortOrderPropertyModel : PropertyModel<int?, PropertyFileSortOrder>
    {
        [JsonConstructor]
        internal FileSortOrderPropertyModel() { }

        public FileSortOrderPropertyModel(PropertyFileSortOrder propertyFileSortOrder) : base(propertyFileSortOrder)
        {
            Value = propertyFileSortOrder.Number;
        }
    }
}
