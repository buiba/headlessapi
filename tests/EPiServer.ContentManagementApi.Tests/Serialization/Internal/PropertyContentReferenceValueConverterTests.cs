using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Core;
using Xunit;
using FluentAssertions;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyContentReferenceValueConverterTests
    {
        private PropertyContentReferenceValueConverter Subject() => new PropertyContentReferenceValueConverter();

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().Convert(null));
        }

        [Fact]
        public void Convert_IfModelIsNotContentReferencePropertyModel_ShouldThrowNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => Subject().Convert(new BlockPropertyModel()));
        }

        [Fact]
        public void Convert_IfContentReferencePropertyModel_ShouldReturnContentReference()
        {
            var contentModelReference = new ContentModelReference
            {
                Id = 5,
                WorkId = 6,
                ProviderName = "ContentReference",
            };

            var propertyModel = new ContentReferencePropertyModel
            {
                Value = contentModelReference
            };

            var contentReference = Subject().Convert(propertyModel) as ContentReference;

            Assert.Equal(contentModelReference.Id, contentReference.ID);
            Assert.Equal(contentModelReference.WorkId, contentReference.WorkID);
            Assert.Equal(contentModelReference.ProviderName, contentReference.ProviderName);
        }

        [Fact]
        public void Convert_IfModelIsContentReferencePropertyModelWithIdIsNull_ShouldReturnNull()
        {
            var contentModelReference = new ContentModelReference
            {
                Id = null,
                WorkId = 6,
                ProviderName = "ContentReference",
            };

            var propertyModel = new ContentReferencePropertyModel
            {
                Value = contentModelReference
            };

            var contentReference = Subject().Convert(propertyModel) as ContentReference;
            Assert.Null(contentReference);
        }

        [Fact]
        public void Convert_IfModelIsContentReferencePropertyModelWithWorkIdIsNull_ShouldReturnContentReferenceWithWorkIdIsZero()
        {
            var contentModelReference = new ContentModelReference
            {
                Id = 5,
                WorkId = null,
                ProviderName = "ProviderName",
            };

            var propertyModel = new ContentReferencePropertyModel
            {
                Value = contentModelReference
            };

            var contentReference = Subject().Convert(propertyModel) as ContentReference;
            Assert.Equal(0, contentReference.WorkID);
        }

        [Fact]
        public void Convert_IfModelNotHasValue_ShouldReturnNull()
        {
            var propertyModel = new ContentReferencePropertyModel
            {
                Value = null
            };

            var contentReference = Subject().Convert(propertyModel) as ContentReference;
            contentReference.Should().BeNull();
        }
    }
}
