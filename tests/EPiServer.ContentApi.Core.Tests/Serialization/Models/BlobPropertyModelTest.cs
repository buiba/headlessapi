using EPiServer.ContentApi.Core.Serialization.Models;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class BlobPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BlobPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            //var blobProperty = new PropertyBlob();

            //var blobPropertyModel = new BlobPropertyModel(blobProperty);

            //Assert.NotNull(blobPropertyModel);
        }
    }
}
