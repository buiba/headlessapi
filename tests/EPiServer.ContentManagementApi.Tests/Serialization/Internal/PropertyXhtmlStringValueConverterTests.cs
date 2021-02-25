using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Core;
using FluentAssertions;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyXhtmlStringValueConverterTests
    {
        private PropertyXhtmlStringValueConverter Subject() => new PropertyXhtmlStringValueConverter();

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotXHtmlStringPropertyModel_ShouldThrowNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => Subject().Convert(new BlockPropertyModel(), null));
        }

        [Fact]
        public void Convert_IfXHtmlStringPropertyModel_ShouldReturnXhtmlString()
        {
            var propertyModel = new XHtmlPropertyModel()
            {
                Value = "<p>Test</p>"
            };

            var xHtmlString = Subject().Convert(propertyModel, null) as XhtmlString;
            xHtmlString.ToEditString().Should().BeEquivalentTo(propertyModel.Value);
        }
    }
}
