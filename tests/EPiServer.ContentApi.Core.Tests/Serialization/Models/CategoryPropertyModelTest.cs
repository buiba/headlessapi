using EPiServer.ContentApi.Core.Serialization.Models;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class CategoryPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CategoryPropertyModel(null));
        }
    }
}
