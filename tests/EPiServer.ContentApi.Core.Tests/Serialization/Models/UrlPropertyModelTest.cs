using EPiServer.ContentApi.Core.Serialization.Models;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class UrlPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UrlPropertyModel(null));
        }

        //[Fact]
        //public void Constructor()
        //{
        //    var propertyUrl = new PropertyUrl();

        //    var urlPropertyModel = new UrlPropertyModel(propertyUrl);

        //    Assert.NotNull(urlPropertyModel);
        //}
    }
}
