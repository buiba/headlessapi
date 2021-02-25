using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.SpecializedProperties;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    public class BlockPropertyConverterTest
    {
        BlockPropertyConverter Subject(IPropertyConverter converter = null, string propertyToIgnore = null, bool noConverter = false)
        {
            var contentTypeRepository = new Mock<IContentTypeRepository>();
            contentTypeRepository.Setup(c => c.Load(It.IsAny<Type>())).Returns(new ContentType());

            var reflectionService = new Mock<ReflectionService>();
            reflectionService.Setup(r => r.GetAttributes(It.IsAny<ContentType>(), It.IsAny<PropertyData>())).Returns(Enumerable.Empty<Attribute>());
            reflectionService.Setup(r => r.GetAttributes(It.IsAny<ContentType>(), It.Is<PropertyData>(p => p.Name.Equals(propertyToIgnore)))).Returns(new Attribute[] { new JsonIgnoreAttribute() });

            if (converter == null && !noConverter)
            {
                var converterMock = new Mock<IPropertyConverter>();
                converterMock.Setup(c => c.Convert(It.IsAny<PropertyData>(), It.IsAny<ConverterContext>())).Returns(Mock.Of<IPropertyModel>());
                converter = converterMock.Object;
            }

            var converterResolver = new Mock<IPropertyConverterResolver>();
            converterResolver.Setup(c => c.Resolve(It.IsAny<PropertyData>())).Returns(converter);

            var blockConverter = new BlockPropertyConverter(contentTypeRepository.Object, reflectionService.Object, converterResolver.Object);
            
            return blockConverter;
        }

        PropertyBlock CreateBlockProperty(PropertyData blockProperty)
        {
            var propertyBlock = new PropertyBlock<BlockData>(new BlockData());
            propertyBlock.Property.Add(blockProperty);
            return propertyBlock;
        }

        [Fact]
        public void ConvertToPropertyModel_IfBlockPropertyHasJsonIgnore_ShouldNotIncludeProperty()
        {
            var ignoredProperty = new PropertyString { Name = "ignored", Value = "something" };
            var propertyBlock = CreateBlockProperty(ignoredProperty);
            var propertyModel = Subject(propertyToIgnore: ignoredProperty.Name).Convert(propertyBlock, new ConverterContext(new ContentApiOptions(), string.Empty, string.Empty, true, null)) as BlockPropertyModel;
            Assert.Empty(propertyModel.Properties);
        }

        [Fact]
        public void ConvertToPropertyModel_IfBlockPropertyHasNoConverter_ShouldNotIncludeProperty()
        {
            var noConverterProperty = new CustomProperty { Name = "custom", Value = "something" };
            var propertyBlock = CreateBlockProperty(noConverterProperty);
            var propertyModel = Subject(noConverter: true).Convert(propertyBlock, new ConverterContext(new ContentApiOptions(), string.Empty, string.Empty, true, null)) as BlockPropertyModel;
            Assert.Empty(propertyModel.Properties);
        }

        [Fact]
        public void ConvertToPropertyModel_IfBlockPropertyHasCustomConverter_ShouldUseConverter()
        {
            var customProperty = new CustomProperty { Name = "custom", Value = "something" };
            var customModel = Mock.Of<IPropertyModel>();

            var customConverter = new Mock<IPropertyConverter>();
            customConverter.Setup(c => c.Convert(customProperty, It.IsAny<ConverterContext>())).Returns(customModel);

            var propertyBlock = CreateBlockProperty(customProperty);
            var propertyModel = Subject(customConverter.Object).Convert(propertyBlock, new ConverterContext(new ContentApiOptions(), string.Empty, string.Empty, true, null)) as BlockPropertyModel;
            Assert.Equal(customModel, propertyModel.Properties[customProperty.Name]);
        }
    }

    public class CustomProperty : PropertyString
    { }
}
