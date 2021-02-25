using System;
using Xunit;
using EPiServer.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class SortOrderPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SortOrderPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertySortOrder = new PropertySortOrder();

            var sortOrderPropertyModel = new SortOrderPropertyModel(propertySortOrder);

            Assert.NotNull(sortOrderPropertyModel);
        }
    }
}
