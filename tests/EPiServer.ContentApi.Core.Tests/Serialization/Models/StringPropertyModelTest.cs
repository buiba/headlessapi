using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class StringPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new StringPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyString = new PropertyString();

            var stringPropertyModel = new StringPropertyModel(propertyString);

            Assert.NotNull(stringPropertyModel);
        }
    }
}
