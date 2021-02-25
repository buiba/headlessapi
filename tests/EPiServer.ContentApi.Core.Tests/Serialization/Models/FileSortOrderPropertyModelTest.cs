using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class FileSortOrderPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FileSortOrderPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyFileSortOrder = new PropertyFileSortOrder();

            var fileSortOrderPropertyModel = new FileSortOrderPropertyModel(propertyFileSortOrder);

            Assert.NotNull(fileSortOrderPropertyModel);
        }
    }
}
