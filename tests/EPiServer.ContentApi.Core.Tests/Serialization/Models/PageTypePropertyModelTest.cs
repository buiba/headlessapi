using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class PageTypePropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PageTypePropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyPageType = new PropertyPageType();

            var pageTypePropertyModel = new PageTypePropertyModel(propertyPageType);

            Assert.NotNull(pageTypePropertyModel);
        }
    }
}
