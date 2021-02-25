using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Tests.Serialization;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Security;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.ContentResult.Internal
{
    public class NullCleaningContentSerializerTest
    {
        private ContentApiModel _capturedModel;
        private NullCleaningContentSerializer Subject()
        {
            var capturingSerializer = new Mock<IContentApiSerializer>();
            capturingSerializer.Setup(c => c.Serialize(It.IsAny<object>()))
                .Callback<object>(o => { _capturedModel = o as ContentApiModel; });
            
            return new NullCleaningContentSerializer(capturingSerializer.Object);
        }

        [Fact]
        public void BuildContent_WhenItemInDictionaryIsNull_ShouldRemoveEntry()
        {
            var key = "akey";
            var model = new ContentApiModel();
            model.Properties[key] = null;
            Subject().Serialize(model);
            Assert.False(_capturedModel.Properties.ContainsKey(key));
        }

        [Fact]
        public void BuildContent_WhenItemInNestedDictionaryIsNull_ShouldRemoveEntry()
        {
            var keyWithValue = "akey";
            var keyWithNullValue = "anotherkey";
            var contentAreaProp = "anArea";
            var blockModel = new ContentApiModel();
            blockModel.Properties[keyWithValue] = "something";
            blockModel.Properties[keyWithNullValue] = null;
            var expandedContentReference = new ContentReferencePropertyModel(new EPiServer.Core.PropertyContentReference(), new TestConverterContext(false), Mock.Of<ContentLoaderService>(), Mock.Of<ContentConvertingService>(), Mock.Of<IContentAccessEvaluator>(), Mock.Of<ISecurityPrincipal>(), Mock.Of<UrlResolverService>())
            {
                ExpandedValue = blockModel
            };
            var contentModel = new ContentApiModel();
            contentModel.Properties.Add(contentAreaProp, new List<ContentReferencePropertyModel>(new[] { expandedContentReference }));
            Subject().Serialize(contentModel);
            Assert.False((_capturedModel.Properties[contentAreaProp] as IEnumerable<ContentReferencePropertyModel>).Single().ExpandedValue.Properties.ContainsKey(keyWithNullValue));
            Assert.True((_capturedModel.Properties[contentAreaProp] as IEnumerable<ContentReferencePropertyModel>).Single().ExpandedValue.Properties.ContainsKey(keyWithValue));
        }

        [Fact]
        public void BuildContent_WhenItemIndDictionaryIsStringEmpty_ShouldRemoveEntry()
        {
            var key = "akey";
            var model = new ContentApiModel();
            model.Properties[key] = string.Empty;
            Subject().Serialize(model);
            Assert.False(_capturedModel.Properties.ContainsKey(key));
        }

        [Fact]
        public void BuildContent_WhenDictionaryValueIsANestedDictionary_ShouldRemoveNullEntryOfNestedDictionary()
        {
            var key = "akey";
            var nestedKey = "nestedKey";
            var nestedKeyWithNullValue = "nestedKeyWithNullValue";
            var model = new ContentApiModel();
            model.Properties[key] = new Dictionary<string, object>() { 
                { nestedKey, "value" },
                { nestedKeyWithNullValue, null },
            };

            Subject().Serialize(model);
            Assert.True((_capturedModel.Properties[key] as Dictionary<string, object>).ContainsKey(nestedKey));
            Assert.False((_capturedModel.Properties[key] as Dictionary<string, object>).ContainsKey(nestedKeyWithNullValue));
        }
    }
}
