using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class VirtualLinkPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VirtualLinkPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyVirtualLink = new PropertyVirtualLink();

            var virtualLinkPropertyModel = new VirtualLinkPropertyModel(propertyVirtualLink);

            Assert.NotNull(virtualLinkPropertyModel);
        }
    }
}
