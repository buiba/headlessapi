using EPiServer.ContentApi.Core.Serialization.Models;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class XHtmlPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new XHtmlPropertyModel(null, new TestConverterContext(true)));
        }

    }
}
