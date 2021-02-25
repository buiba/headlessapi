using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Core;
using FluentAssertions;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyContentReferenceListValueConverterTest
    {
        private PropertyContentReferenceListValueConverter Subject() => new PropertyContentReferenceListValueConverter();

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotContentReferenceListPropertyModel_ShouldThrowNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => Subject().Convert(new BlockPropertyModel(), null));
        }


        [Fact]
        public void Convert_IfContentReferenceListPropertyModel_ShouldReturnContentReferenceList()
        {
            var contentModelReferences = new List<ContentModelReference>
            {
                new ContentModelReference
                {
                    Id = 5,
                    WorkId =6,
                    ProviderName = "ContentModelReferenceOne"
                },
                new ContentModelReference
                {
                    Id = 7,
                    WorkId = 8,
                    ProviderName = "ContentModelReferenceTwo"
                }
            };

            var propertyModel = new ContentReferenceListPropertyModel
            {
                Value = contentModelReferences
            };

            var contentReferences = Subject().Convert(propertyModel, null) as List<ContentReference>;

            contentReferences.Should().BeEquivalentTo(contentModelReferences.Select(c => new ContentReference()
            {
                ID = c.Id.Value,
                WorkID = c.WorkId.Value,
                ProviderName = c.ProviderName,
            }));
        }

        [Fact]
        public void Convert_IfValueContainsNullWorkIdContentModelReference_ShouldReturnContentReferenceWithWorkIdIsZero()
        {
            var contentModelReferences = new List<ContentModelReference>
            {
                new ContentModelReference
                {
                    Id = 5,
                    WorkId = null,
                    ProviderName = "ContentModelReferenceOne"
                }
            };

            var propertyModel = new ContentReferenceListPropertyModel
            {
                Value = contentModelReferences
            };

            var contentReferences = Subject().Convert(propertyModel, null) as List<ContentReference>;
            Assert.Equal(0, contentReferences[0].WorkID);            
        }

        [Fact]
        public void Convert_IfValueContainsNullIdContentModelReference_ShouldNotReturnNullIdContentReference()
        {
            var contentModelReferences = new List<ContentModelReference>
            {
                new ContentModelReference
                {
                    Id = null,
                    WorkId = 1,
                    ProviderName = "ContentModelReferenceOne"
                },
                new ContentModelReference
                {
                    Id = 1,
                    WorkId = 2,
                    ProviderName = "ContentModelReferenceTwo"
                }
            };

            var propertyModel = new ContentReferenceListPropertyModel
            {
                Value = contentModelReferences
            };

            var contentReferences = Subject().Convert(propertyModel, null) as List<ContentReference>;
            Assert.Single(contentReferences);
        }

        [Fact]
        public void Convert_IfModelIsNull_ShouldReturnNull()
        {
            var propertyModel = new ContentReferenceListPropertyModel
            {
                Value = null
            };

            var contentReferences = Subject().Convert(propertyModel, null) as List<ContentReference>;
            contentReferences.Should().BeNull();
        }
    }
}
