using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class NumberPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new NumberPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyNumber = new PropertyNumber();

            var numberPropertyModel = new NumberPropertyModel(propertyNumber);

            Assert.NotNull(numberPropertyModel);
        }
    }
}
