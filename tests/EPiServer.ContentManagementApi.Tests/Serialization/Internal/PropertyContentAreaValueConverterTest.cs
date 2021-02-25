using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyContentAreaValueConverterTest
    {
        private PropertyContentAreaValueConverter Subject() => new PropertyContentAreaValueConverter(new Mock<DisplayOptions>().Object);

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotContentReferencePropertyModel_ShouldThrowNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => Subject().Convert(new BlockPropertyModel(), null));
        }

        [Fact]
        public void Convert_IfModelValueIsNull_ShouldReturnNull()
        {
            var propertyModel = new ContentAreaPropertyModel
            {
                Value = null
            };

            Assert.Null(Subject().Convert(propertyModel, null));
        }

        [Fact]
        public void Convert_IfModelValueIsContainDisplayOptionDoesNotExist_ShouldThrowException()
        {
            var subject = Subject();

            var contentAreItemModels = new List<ContentAreaItemModel>
            {
                new ContentAreaItemModel
                {
                    DisplayOption = "test",
                    ContentLink = new ContentModelReference { Id = 100 }
                }
            };

            var propertyModel = new ContentAreaPropertyModel
            {
                Value = contentAreItemModels
            };

            Assert.Throws<ErrorException>(() => Subject().Convert(propertyModel, null));
        }
    }
}
