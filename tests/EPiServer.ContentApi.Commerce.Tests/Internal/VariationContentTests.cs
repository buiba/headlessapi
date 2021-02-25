using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using System.Globalization;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Internal
{
    public class VariationContentTests : TestBase
    {
        [Fact]
        public void It_should_return_product_variant_content_api_model()
        {
            var content = new VariationContent()
            {
                CommerceMediaCollection = _commerceMediaCollection,
                ParentLink = new PageReference(16, 4),
                ContentLink = new PageReference(11, 5),
                Status = VersionStatus.Published,
                Language = new CultureInfo("en-us"),
            };

            var model = _mapper.Convert(content, new TestConverterContext()) as VariationContentApiModel;
            Assert.NotNull(model);
            Assert.NotNull(model.Assets);
            Assert.NotNull(model.ContentLink);
            Assert.Equal(16, model.ParentLink.Id.Value);
            Assert.Equal(4, model.ParentLink.WorkId);
        }
    }
}
