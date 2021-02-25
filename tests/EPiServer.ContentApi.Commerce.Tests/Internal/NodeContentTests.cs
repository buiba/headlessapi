using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using EPiServer.ContentApi.Commerce.Internal.Models.Content;
using Xunit;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Commerce.Tests.Internal
{
    public class NodeContentTests : TestBase
    {
        private List<ProductContent> _productContentList;
        private NodeContent _nodeContent;
        public NodeContentTests()
        {
            _productContentList = new List<ProductContent>
                    {
                        new ProductContent
                        {
                            ContentLink = new PageReference(12, 3),
                            ParentLink = new PageReference(16, 4),
                            CommerceMediaCollection = _commerceMediaCollection
                        },
                        new ProductContent
                        {
                            ContentLink = new PageReference(11, 3),
                            ParentLink = new PageReference(16, 4),
                            CommerceMediaCollection = _commerceMediaCollection
                        }
                    };

            _nodeContent = new NodeContent()
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
            var model = _mapper.Convert(_nodeContent, new TestConverterContext()) as NodeContentApiModel;
                Assert.NotNull(model);
                Assert.NotNull(model.ContentLink);
                Assert.Equal(16, model.ParentLink.Id.Value);
                Assert.Equal(4, model.ParentLink.WorkId);
                Assert.NotEmpty(model.Assets);
            
        }
    }
}
