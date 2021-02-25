using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class ImageUrlPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ImageUrlPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyImageUrl = new PropertyImageUrl();

            var imageUrlPropertyModel = new ImageUrlPropertyModel(propertyImageUrl);

            Assert.NotNull(imageUrlPropertyModel);
        }
    }
}
