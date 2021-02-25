using System;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Serialization.Models
{
    public class DictionaryMultiplePropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DictionaryMultiplePropertyModel(null));
        }
    }
}
