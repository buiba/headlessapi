using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Moq;
using System.Globalization;
using System.Linq;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Internal
{
    public class ProductContentTests : TestBase
    {
        protected ProductContent _productContent;

        public ProductContentTests()
        {
            _productContent = new ProductContent()
            {
                CommerceMediaCollection = _commerceMediaCollection,
                ParentLink = new PageReference(16, 4),
                ContentLink = new PageReference(11, 5),
                Status = VersionStatus.Published,
                Language = new CultureInfo("en-us"),
            };
        }

        [Fact]
        public void TransformContent_ShouldReturnProductModel()
        {
            var model = _mapper.Convert(_productContent, new TestConverterContext()) as ProductContentApiModel;

            Assert.NotNull(model);
            Assert.NotNull(model.Assets);
            Assert.NotNull(model.ContentLink);
            Assert.Equal(16, model.ParentLink.Id.Value);
            Assert.Equal(4, model.ParentLink.WorkId);
        }
    }
}
