using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyBlockValueConverterTests
    {
        private readonly Mock<IPropertyDataValueConverterResolver> _mockConverterResolver = new Mock<IPropertyDataValueConverterResolver>();
        private PropertyBlockValueConverter Subject()
        {
            var converter = new PropertyBlockValueConverter(_mockConverterResolver.Object);
            return converter;
        }

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            var subject = Subject();

            Assert.Throws<ArgumentNullException>(() => subject.Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotBlockPropertyModel_ShouldThrowNotSupportedException()
        {
            var subject = Subject();

            Assert.Throws<NotSupportedException>(() => subject.Convert(new ContentReferencePropertyModel(), null));
        }

        [Fact]
        public void Convert_IfPropertyDataIsNotIPropertyBlock_ShouldThrowNotSupportedException()
        {
            var subject = Subject();

            Assert.Throws<NotSupportedException>(() => subject.Convert(new BlockPropertyModel(), new NotPropertyBlock()));
        }

        [Fact]
        public void Convert_IfModelNotContainsProperties_ShouldReturnBlockWithoutData()
        {
            var propertyData = new CustomBlockProperty(new OuterBlock());
            propertyData.Property.Add("innerblock", new CustomBlockProperty(new InnerBlock()));

            var blockPropertyModel = new BlockPropertyModel();

            var subject = Subject();
            var block = subject.Convert(blockPropertyModel, propertyData);
            Assert.Null(((block as OuterBlock).Property["innerblock"].Value as InnerBlock).Title);
        }

        [Fact]
        public void Convert_IfPropertiesNotContainsPropertyModel_ShouldReturnBlockWithoutData()
        {
            var propertyData = new CustomBlockProperty(new OuterBlock());
            propertyData.Property.Add("innerblock", new CustomBlockProperty(new InnerBlock()));

            var blockPropertyModel = new BlockPropertyModel();
            blockPropertyModel.Properties.Add("hi", "there");

            var subject = Subject();
            var block = subject.Convert(blockPropertyModel, propertyData);
            Assert.Null(((block as OuterBlock).Property["innerblock"].Value as InnerBlock).Title);
        }

        [Fact]
        public void Convert_IfCannotResolveConverter_ShouldReturnBlockWithoutData()
        {
            var propertyData = new CustomBlockProperty(new OuterBlock());
            propertyData.Property.Add("innerblock", new CustomBlockProperty(new InnerBlock()));

            var blockPropertyModel = new BlockPropertyModel();
            blockPropertyModel.Properties.Add("Heading", new StringPropertyModel(new Core.PropertyString("additional info")));

            var subject = Subject();
            _mockConverterResolver.Setup(x => x.Resolve(It.IsAny<IPropertyModel>())).Returns<IPropertyDataValueConverter>(null);

            var block = subject.Convert(blockPropertyModel, propertyData);
            Assert.Null(((block as OuterBlock).Property["innerblock"].Value as InnerBlock).Title);
        }

        [Fact]
        public void Convert_IfPropertyNameNotMatch_ShouldNotUpdateTheValue()
        {
            var propertyData = new CustomBlockProperty(new OuterBlock());
            propertyData.Property.Add("title", new Core.PropertyString("additional info"));

            var blockPropertyModel = new BlockPropertyModel();
            blockPropertyModel.Properties.Add("wrongtitle", new StringPropertyModel(new Core.PropertyString("updated info")));

            var dataValueConverter = new SimplePropertyDataValueConverter();
            var subject = Subject();
            _mockConverterResolver.Setup(x => x.Resolve(It.Is<IPropertyModel>(x => x is StringPropertyModel))).Returns(dataValueConverter);

            var block = subject.Convert(blockPropertyModel, propertyData);
            Assert.Equal(propertyData.Property["title"].Value, (block as OuterBlock).Property["title"].Value);
        }

        [Fact]
        public void Convert_IfPropertyMatch_ShouldUpdateProperty()
        {
            var propertyData = new CustomBlockProperty(new OuterBlock());
            propertyData.Property.Add("title", new Core.PropertyString("additional info"));

            var blockPropertyModel = new BlockPropertyModel();
            blockPropertyModel.Properties.Add("title", new StringPropertyModel(new Core.PropertyString("updated info")));

            var dataValueConverter = new SimplePropertyDataValueConverter();
            var subject = Subject();
            _mockConverterResolver.Setup(x => x.Resolve(It.Is<IPropertyModel>(x => x is StringPropertyModel))).Returns(dataValueConverter);

            var block = subject.Convert(blockPropertyModel, propertyData);
            Assert.Equal("updated info", (block as OuterBlock).Property["title"].Value);
        }

        [Fact]
        public void Convert_IfNestedProperty_ShouldReturnRecursiveProperty()
        {
            var innerBlock = new InnerBlock();
            innerBlock.Property.Add("Title", new Core.PropertyString("original inner block title"));

            var propertyData = new CustomBlockProperty(new OuterBlock());
            propertyData.Property.Add("NestedProperty", new CustomBlockProperty(innerBlock));

            var blockPropertyModel = new BlockPropertyModel()
            {
                Name = "SomeBlock",
                Properties = new Dictionary<string, object>()
                {
                    {
                        "NestedProperty", new BlockPropertyModel()
                        {
                            Name = "SomeNestedBlock",
                            Properties = new Dictionary<string, object>()
                            {
                                { "Title", new StringPropertyModel() { Value = "Some Nested Block Title" } }
                            }
                        }
                    }
                }
            };

            var subject = Subject();
            _mockConverterResolver.Setup(x => x.Resolve(It.Is<IPropertyModel>(x => x is StringPropertyModel))).Returns(new SimplePropertyDataValueConverter());
            _mockConverterResolver.Setup(x => x.Resolve(It.Is<IPropertyModel>(x => x is BlockPropertyModel))).Returns(subject);

            var block = subject.Convert(blockPropertyModel, propertyData);
            Assert.Equal("Some Nested Block Title", ((block as OuterBlock).Property["NestedProperty"].Value as InnerBlock).Property["Title"].Value);
        }
    }
}
