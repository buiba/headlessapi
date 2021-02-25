using System;
using Xunit;
using EPiServer.Core;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class LongStringPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new LongStringPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyLongString = new PropertyLongString();

            var longStringPropertyModel = new LongStringPropertyModel(propertyLongString);

            Assert.NotNull(longStringPropertyModel);
        }
    }
}
