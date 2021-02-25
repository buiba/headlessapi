using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertySelector"/>
    /// </summary>
    public class SelectorPropertyModel : PropertyModel<IEnumerable<string>, PropertySelector>
    {
        [JsonConstructor]
        internal SelectorPropertyModel() { }

        public SelectorPropertyModel(PropertySelector propertySelector) : base(propertySelector)
        {
            Value = string.IsNullOrEmpty(propertySelector.ToString()) ? new List<string>() :
                propertySelector.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
