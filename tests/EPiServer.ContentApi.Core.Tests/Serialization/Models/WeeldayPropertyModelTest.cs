using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class WeeldayPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WeekdayPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyWeekDay = new PropertyWeekDay();

            var weekdayPropertyModel = new WeekdayPropertyModel(propertyWeekDay);

            Assert.NotNull(weekdayPropertyModel);
        }
    }
}
