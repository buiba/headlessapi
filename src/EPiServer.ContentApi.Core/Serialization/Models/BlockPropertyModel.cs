using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyBlock"/>
    /// </summary>
    public class BlockPropertyModel : IPropertyModel<PropertyBlock>, IPersonalizableProperty, IFlattenableProperty
    {
        public bool ExcludePersonalizedContent { get; set; }
        [JsonIgnore]
        public PropertyBlock PropertyDataProperty { get; set; }
        public string Name { get; set; }

        public string PropertyDataType => nameof(PropertyBlock);

        [JsonExtensionData]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public object Flatten() => Properties.ToDictionary(p => p.Key, p => p.Value is IFlattenableProperty flattenableProperty ? flattenableProperty.Flatten() : p.Value);
    }
}
