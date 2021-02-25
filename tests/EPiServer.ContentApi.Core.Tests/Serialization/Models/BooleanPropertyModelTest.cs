using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class BooleanPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BooleanPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var booleanProperty = new PropertyBoolean();

            var booleanPropertyModel = new BooleanPropertyModel(booleanProperty);

            Assert.NotNull(booleanPropertyModel);
        }
    }
}
