using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    public class ContentConverterTest
    {
        [Fact]
        public void Convert_WhenCustomContentFilter_ShouldBeAbleToRemoveProperty()
        {
            string somePropertyName = "SomeProperty";
            var contentFilter = new TestContentFilter(somePropertyName);
            var content = new TestContent();
            content.Property.Add(new PropertyString { Name = somePropertyName, Value = "This should not be on serialized model" });
            var subject = Subject(contentFilter: contentFilter);
            var model = subject.Convert(content, new ConverterContext(new ContentApiOptions(), "", "", true, CultureInfo.InvariantCulture));
            Assert.False(model.Properties.TryGetValue(somePropertyName, out var _));
        }

        [Fact]
        public void Convert_WhenCustomModelFilter_ShouldBeAbleToAlterModel()
        {
            string somePropertyName = "SomeProperty";
            var contentModelFilter = new TestContentApiModelFilter(somePropertyName);
            var content = new TestContent();
            content.Property.Add(new PropertyString { Name = somePropertyName, Value = "This should not be on serialized model" });
            var subject = Subject(contentApiModelFilter: contentModelFilter);
            var model = subject.Convert(content, new ConverterContext(new ContentApiOptions(), "", "", true, CultureInfo.InvariantCulture));
            Assert.False(model.Properties.TryGetValue(somePropertyName, out var _));
        }
     
        private ContentConvertingService Subject(IContentApiModelFilter contentApiModelFilter = null, IContentFilter contentFilter = null, TestContentApiModel contentApiModel = null)
        {
            TestContentApiModel testContentApiModel = null;
            var contentConverter = new Mock<IContentConverter>();
            contentConverter.Setup(c => c.Convert(It.IsAny<IContent>(), It.IsAny<ConverterContext>()))
                .Callback<IContent, ConverterContext>((content, ctx) =>
                {
                    testContentApiModel = contentApiModel ?? new TestContentApiModel();
                    foreach (var property in content.Property)
                    {
                        testContentApiModel.Properties[property.Name] = property.Value;
                    }
                })
                .Returns(() => testContentApiModel);
            var contentConverterResolver = new Mock<IContentConverterResolver>();
            contentConverterResolver.Setup(c => c.Resolve(It.IsAny<IContent>())).Returns(contentConverter.Object);
            return new ContentConvertingService(contentConverterResolver.Object, contentFilter != null ? new[] { contentFilter } : Enumerable.Empty<IContentFilter>(), contentApiModelFilter != null ? new[] { contentApiModelFilter } : Enumerable.Empty<IContentApiModelFilter>());
        }
    }

    public class TestableContentItem : IContentItem
    {
        public ContentModelReference ContentLink { get; set ; }
    }

    public class TestContent : BasicContent
    {}

    public class TestContentApiModel : ContentApiModel
    { }

    public class TestContentFilter : ContentFilter<TestContent>
    {
        private readonly string _propertytoRemove;

        public TestContentFilter(string propertyToRemove = null)
        {
            _propertytoRemove = propertyToRemove;
        }
        public override void Filter(TestContent content, ConverterContext converterContext)
        {
            if (_propertytoRemove != null)
            {
                content.Property.Remove(_propertytoRemove);
            }
        }
    }

    public class TestContentApiModelFilter : ContentApiModelFilter<TestContentApiModel>
    {
        private readonly string _propertytoRemove;
        public TestContentApiModelFilter(string propertyToRemove = null)
        {
            _propertytoRemove = propertyToRemove;
        }
        public override void Filter(TestContentApiModel contentApiModel, ConverterContext converterContext)
        {
            if (_propertytoRemove != null)
            {
                contentApiModel.Properties.Remove(_propertytoRemove);
            }
        }
    }
}
