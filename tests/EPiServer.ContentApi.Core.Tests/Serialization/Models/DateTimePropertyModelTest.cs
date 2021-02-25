using System;
using Xunit;
using EPiServer.Core;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class DateTimePropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DateTimePropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyDate = new PropertyDate();

            var dateTimePropertyModel = new DateTimePropertyModel(propertyDate);

            Assert.NotNull(dateTimePropertyModel);
        }
    }
}
