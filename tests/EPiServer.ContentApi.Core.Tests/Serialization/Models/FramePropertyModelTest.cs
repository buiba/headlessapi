using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class FramePropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FramePropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyFrame = new PropertyFrame();

            var framePropertyModel = new FramePropertyModel(propertyFrame);

            Assert.NotNull(framePropertyModel);
        }
    }
}
