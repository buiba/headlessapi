using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.SpecializedProperties;
using FluentAssertions;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyLinkCollectionValueConverterTests
    {
        private PropertyLinkCollectionValueConverter Subject()
        {
            return new PropertyLinkCollectionValueConverter();
        }

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            var subject = Subject();

            Assert.Throws<ArgumentNullException>(() => subject.Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotLinkCollectionPropertyModel_ShouldThrowNotSupportedException()
        {
            var subject = Subject();

            Assert.Throws<NotSupportedException>(() => subject.Convert(new BlockPropertyModel(), null));
        }

        [Fact]
        public void Convert_IfLinkCollectionPropertyModel_ShouldReturnLinkItemCollection()
        {
            var subject = Subject();
            var linkItemNodes = new List<LinkItemNode>
            {
                new LinkItemNode
                {
                    Href = "http://href1.com",
                    Text = "link 1",
                    Target = "blank",
                    Title = "link 1"
                },
                new LinkItemNode
                {
                    Href = "http://href2.com",
                    Text = "link 2",
                    Target = "blank",
                    Title = "link 2"
                },
            };

            var propertyModel = new LinkCollectionPropertyModel
            {
                Value = linkItemNodes
            };

            var linkItemCollection = subject.Convert(propertyModel, null) as LinkItemCollection;
            
            linkItemCollection.Should().BeEquivalentTo(linkItemNodes.Select(x => new LinkItem
            {
                Href = x.Href,
                Title = x.Title,
                Target = x.Target,
                Text = x.Text
            }));
        }

        [Fact]
        public void Convert_IfModelNotHasValue_ShouldReturnNull()
        {
            var subject = Subject();

            var propertyModel = new LinkCollectionPropertyModel
            {
                Value = null
            };

            var linkItemCollection = subject.Convert(propertyModel, null) as LinkItemCollection;

            linkItemCollection.Should().BeNull();
        }
    }
}
