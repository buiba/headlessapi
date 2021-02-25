using System;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Serialization.Models
{
    public class PropertyDecimalModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyDecimalModel(null));
        }
    }
}
