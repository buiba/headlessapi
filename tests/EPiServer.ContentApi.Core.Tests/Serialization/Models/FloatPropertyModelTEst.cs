using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class FloatPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FloatPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyFloatNumber = new PropertyFloatNumber();

            var floatPropertyModel = new FloatPropertyModel(propertyFloatNumber);

            Assert.NotNull(floatPropertyModel);
        }
    }
}
