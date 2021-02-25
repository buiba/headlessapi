using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class SelectorPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SelectorPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertySelector = new PropertySelector();

            var selectorPropertyModel = new SelectorPropertyModel(propertySelector);

            Assert.NotNull(selectorPropertyModel);
        }
    }
}
