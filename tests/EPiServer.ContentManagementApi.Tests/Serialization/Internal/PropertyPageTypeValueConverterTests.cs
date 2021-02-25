using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.DataAbstraction;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyPageTypeValueConverterTests
    {
        private PropertyPageTypeValueConverter Subject()
        {
            var contentTypeRepository = new Mock<IContentTypeRepository>();
            contentTypeRepository.Setup(c => c.Load(It.Is<string>(s => s.Equals("TestPage")))).Returns(new ContentType() { ID = 1 });
            var converter = new PropertyPageTypeValueConverter(contentTypeRepository.Object);
            return converter;
        }        

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotPageTypePropertyModel_ShouldThrowNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => Subject().Convert(new BlockPropertyModel(), null));
        }

        [Fact]
        public void Convert_IfModelValueIsNotAValidPageTypeName_ShouldReturnNull()
        {
            var value = Subject().Convert(new PageTypePropertyModel() { Value = "InvalidPage" }, null);
            Assert.Null(value);
        }

        [Fact]
        public void Convert_IfModelValueIsValidPageTypeName_ShouldReturnNull()
        {
            var value = Subject().Convert(new PageTypePropertyModel() { Value = "TestPage" }, null);
            Assert.Equal(1, value);
        }
    }
}
