using System;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Serialization.Models
{
    public class SeoInformationBlockPropertyModelTest
    {
        [Fact]
        public void ShouldThrowNullException_WhenPropertyDataIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SeoInformationBlockPropertyModel(null));
        }
    }
}
