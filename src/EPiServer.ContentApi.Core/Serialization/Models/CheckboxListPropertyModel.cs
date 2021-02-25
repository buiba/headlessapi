using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyCheckBoxList"/>
    /// </summary>
    public class CheckboxListPropertyModel : PropertyModel<IEnumerable<string>, PropertyCheckBoxList>
    {
        [JsonConstructor]
        internal CheckboxListPropertyModel()
        {
        }

        public CheckboxListPropertyModel(PropertyCheckBoxList propertyCheckBoxList) : base(propertyCheckBoxList)
        {
            Value = string.IsNullOrEmpty(propertyCheckBoxList.ToString()) ? new List<string>() :
                propertyCheckBoxList.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        }
    }
}
