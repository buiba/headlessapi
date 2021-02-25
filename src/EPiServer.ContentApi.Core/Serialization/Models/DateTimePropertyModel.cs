using System;
using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyDate"/>
    /// </summary>
    public class DateTimePropertyModel : PropertyModel<DateTime?, PropertyDate>
    {
        [JsonConstructor]
        internal DateTimePropertyModel() { }

        public DateTimePropertyModel(PropertyDate propertyDate) : base(propertyDate)
        {
            Value = propertyDate.Date?.ToUniversalTime();
        }
    }
}
