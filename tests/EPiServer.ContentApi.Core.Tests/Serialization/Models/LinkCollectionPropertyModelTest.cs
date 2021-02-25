using System;
using System.Globalization;
using System.Linq;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using Moq;
using Xunit;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{   
    public class LinkCollectionPropertyModelTest
    {
		private Mock<ContentLoaderService> _mockContentLoaderService;
        private Mock<ContentConvertingService> _mockContentConvertingService;
        private Mock<IContentAccessEvaluator> _mockContentAccessEvaluator;
        private Mock<ISecurityPrincipal> _mockPrincipalAccessor;

        public LinkCollectionPropertyModelTest()
        {
            _mockContentLoaderService = new Mock<ContentLoaderService>();
            _mockContentConvertingService = new Mock<ContentConvertingService>();
            _mockContentAccessEvaluator = new Mock<IContentAccessEvaluator>();
            _mockPrincipalAccessor = new Mock<ISecurityPrincipal>();
        }

        private LinkCollectionPropertyModel CreateMockPropertyModel(PropertyLinkCollection property, bool excludePersonalizedContent)
        {
            return new LinkCollectionPropertyModel(property, new TestConverterContext(excludePersonalizedContent), _mockContentLoaderService.Object, _mockContentConvertingService.Object, _mockContentAccessEvaluator.Object, _mockPrincipalAccessor.Object);
        }

        public class Constructor : LinkCollectionPropertyModelTest
        {
            public class WhenPropertyIsNull : Constructor
            {
                [Fact]
                public void ItShouldThrowArgumentNulLException()
                {
                    Assert.Throws<ArgumentNullException>(() => CreateMockPropertyModel(null, true));
                }
            }

            public class WhenPropertyIsNotNullAndLinksIsNull : Constructor
            {
                [Fact]
                public void ValueShouldBeEmptyList()
                {
                    var mockProperty = new PropertyLinkCollection();
                    Assert.False(CreateMockPropertyModel(mockProperty, false).Value.Any());
                }
            }

            public class WhenPropertyIsNotNullAndLinksIsEmpty : Constructor
            {
                [Fact]
                public void ValueShouldBeEmptyList()
                {
                    var mockProperty = new PropertyLinkCollection();
                    mockProperty.Links = new LinkItemCollection();
                    Assert.False(CreateMockPropertyModel(mockProperty, false).Value.Any());
                }
            }
        }

        public class Expand : LinkCollectionPropertyModelTest
        {
            public class IfValueIsNull : Expand
            {
                [Fact]
                public void ExpandedValueShouldBeNull()
                {
                    var mockProperty = new PropertyLinkCollection();
                    var contentReferencePropertyModel = CreateMockPropertyModel(mockProperty, false);

                    contentReferencePropertyModel.Expand(new CultureInfo("en"));

                    Assert.Null(contentReferencePropertyModel.ExpandedValue);
                }
            }
        }

    }
}
