using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class DropDownListPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DropDownListPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyDropdownList = new PropertyDropDownList();

            var dropDownListPropertyModel = new DropDownListPropertyModel(propertyDropdownList);

            Assert.NotNull(dropDownListPropertyModel);
        }
    }
}
