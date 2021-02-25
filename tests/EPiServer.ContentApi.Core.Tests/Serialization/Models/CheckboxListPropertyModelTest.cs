using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class CheckboxListPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CheckboxListPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var checkboxListProperty = new PropertyCheckBoxList();

            var checkboxListPropertyModel = new CheckboxListPropertyModel(checkboxListProperty);

            Assert.NotNull(checkboxListPropertyModel);
        }
    }
}
