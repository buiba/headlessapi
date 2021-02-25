using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using System.Globalization;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Internal
{
    public class BundleContentTests : TestBase
    {
        protected BundleContent _content;
        public BundleContentTests()
        {
            _content = new BundleContent()
            {
                CommerceMediaCollection = _commerceMediaCollection,
                ParentLink = new PageReference(16, 4),
                ContentLink = new PageReference(11, 5),
                Status = VersionStatus.Published,
                Language = new CultureInfo("en-us"),
        };
        }

        [Fact]
        public void TransformContent_ShouldReturnBundleModel()
        {
            var model = _mapper.Convert(_content, new TestConverterContext()) as BundleContentApiModel;
            Assert.NotNull(model);
            Assert.NotNull(model.Assets);
            Assert.NotNull(model.ContentLink);
            Assert.Equal(16, model.ParentLink.Id.Value);
            Assert.Equal(4, model.ParentLink.WorkId);
        }
    }
}
