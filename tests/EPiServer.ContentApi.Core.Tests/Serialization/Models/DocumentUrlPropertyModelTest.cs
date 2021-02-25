using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class DocumentUrlPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DocumentUrlPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyDocumentUrl = new PropertyDocumentUrl();

            var documentUrlPropertyModel = new DocumentUrlPropertyModel(propertyDocumentUrl);

            Assert.NotNull(documentUrlPropertyModel);
        }
    }
}
