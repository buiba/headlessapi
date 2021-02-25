using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Tests.Serialization.Internal
{
    public class ContentApiModelMinifierTest
    {
        [Fact]
        public void Minify_WhenSelectParameterIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentApiModelMinifier().Minify(new ContentApiModel(), null));
        }

        [Fact]
        public void Minify_WhenContentModelParameterIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentApiModelMinifier().Minify(null, new[] { "something" }));
        }

        [Fact]
        public void Minify_WhenCalled_ShouldAddNameFromContentApiModel()
        {
            var contentApiModel = new ContentApiModel { Name = "someName" };
            var minifiedModel = new ContentApiModelMinifier().Minify(contentApiModel, new[] { "something" });
            Assert.Equal(contentApiModel.Name, minifiedModel.Name);
        }

        [Fact]
        public void Minify_WhenCalled_ShouldAddContentLinkFromContentApiModel()
        {
            var contentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { GuidValue = Guid.NewGuid() } };
            var minifiedModel = new ContentApiModelMinifier().Minify(contentApiModel, new[] { "something" });
            Assert.Equal(contentApiModel.ContentLink.GuidValue.Value, minifiedModel.ContentLink.GuidValue.Value);
        }

        [Fact]
        public void Minify_WhenSelectedPropertyDoesNotMatchInCasing_ShouldAddProperty()
        {
            var key = "aKey";
            var value = new object();
            var contentApiModel = new ContentApiModel { Properties = new Dictionary<string, object> { { key, value } } };
            var minifiedModel = new ContentApiModelMinifier().Minify(contentApiModel, new[] { "AkEY" });
            Assert.Same(value, minifiedModel.Properties[key]);
        }

        [Fact]
        public void Minify_WhenExistingPropertyDoesNotMatchSelect_ShouldNotAddProperty()
        {
            var key = "aKey";
            var value = new object();
            var contentApiModel = new ContentApiModel { Properties = new Dictionary<string, object> { { key, value } } };
            var minifiedModel = new ContentApiModelMinifier().Minify(contentApiModel, new[] { "anotherKey" });
            Assert.False(minifiedModel.Properties.ContainsKey(key));
        }

        [Fact]
        public void Minify_WhenSeveralExistingPropertyMatchesSelect_ShouldAddProperties()
        {
            var key1 = "aKey";
            var value1 = new object();
            var key2 = "anotherKey";
            var value2 = new object();
            var contentApiModel = new ContentApiModel { Properties = new Dictionary<string, object> { { key1, value1 }, { key2, value2 } } };
            var minifiedModel = new ContentApiModelMinifier().Minify(contentApiModel, new[]{key1,key2 });
            Assert.Same(value1, minifiedModel.Properties[key1]);
            Assert.Same(value2, minifiedModel.Properties[key2]);
        }
    }
}
