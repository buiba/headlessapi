using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyDateList"/>
    /// </summary>
    public class DateListPropertyModel : PropertyModel<IList<DateTime>, PropertyDateList>
    {
        [JsonConstructor]
        internal DateListPropertyModel() { }

        public DateListPropertyModel(PropertyDateList propertyDateList) : base(propertyDateList)
        {
            Value = propertyDateList.List;
        }
    }
}
