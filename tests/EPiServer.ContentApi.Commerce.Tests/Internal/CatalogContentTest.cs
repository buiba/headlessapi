using System.Globalization;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Internal
{
    public class CatalogContentTest : TestBase
    {
        [Fact]
        public void It_should_return__content_api_model()
        {
            var content = new CatalogContent()
            {
                ParentLink = new PageReference(16, 4),
                ContentLink = new PageReference(11, 5),
                Status = VersionStatus.Published,
                Language = new CultureInfo("en-us"),
            };

            var model = _mapper.Convert(content, new TestConverterContext());
            Assert.NotNull(model);
            Assert.NotNull(model.ContentLink);
            Assert.Equal(16, model.ParentLink.Id.Value);
            Assert.Equal(4, model.ParentLink.WorkId);
        }
    }
}
