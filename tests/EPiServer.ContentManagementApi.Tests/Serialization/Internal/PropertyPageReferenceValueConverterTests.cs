using System;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Core;
using FluentAssertions;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyPageReferenceValueConverterTests
    {
        private PropertyPageReferenceValueConverter Subject() => new PropertyPageReferenceValueConverter();

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotPageReferencePropertyModel_ShouldThrowNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => Subject().Convert(new BlockPropertyModel(), null));
        }

        [Fact]
        public void Convert_IfModelIsPageReferencePropertyModelWithValueIsNull_ShouldReturnNull()
        {
            var propertyModel = new PageReferencePropertyModel
            {
                Value = null
            };

            var pageReference = Subject().Convert(propertyModel, null) as PageReference;
            Assert.Null(pageReference);
        }

        [Fact]
        public void Convert_IfModelIsPageReferencePropertyModelWithIdIsNull_ShouldReturnNull()
        {
            var contentModelReference = new ContentModelReference
            {
                Id = null,
                WorkId = 6,
                ProviderName = "PageReference",
            };

            var propertyModel = new PageReferencePropertyModel
            {
                Value = contentModelReference
            };

            var pageReference = Subject().Convert(propertyModel, null) as PageReference;
            Assert.Null(pageReference);
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

            var propertyModel = new PageReferencePropertyModel
            {
                Value = contentModelReference
            };

            var pageReference = Subject().Convert(propertyModel, null) as PageReference;
            Assert.Equal(0, pageReference.WorkID);
        }

        [Fact]
        public void Convert_IfPageReferencePropertyModelIsValid_ShouldReturnPageReference()
        {
            var contentModelReference = new ContentModelReference
            {
                Id = 5,
                WorkId = 6,
                ProviderName = "PageReference",
            };

            var propertyModel = new PageReferencePropertyModel
            {
                Value = contentModelReference
            };

            var pageReference = Subject().Convert(propertyModel, null) as PageReference;
            pageReference.Should().BeEquivalentTo(new PageReference(contentModelReference.Id.Value, contentModelReference.WorkId.Value, contentModelReference.ProviderName));
        }
    }
}
