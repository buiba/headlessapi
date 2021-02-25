using EPiServer.Core;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyWeekDay"/>
    /// </summary>
    public class WeekdayPropertyModel : PropertyModel<string, PropertyWeekDay>
    {
        [JsonConstructor]
        internal WeekdayPropertyModel() { }

        public WeekdayPropertyModel(PropertyWeekDay propertyWeekDay) : base(propertyWeekDay)
        {
            Value = ((Weekday)propertyWeekDay.Value).ToString();
        }
    }
}
