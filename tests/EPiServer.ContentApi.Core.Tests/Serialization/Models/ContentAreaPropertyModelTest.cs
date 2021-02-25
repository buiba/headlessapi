using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using EPiServer.Web;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class ContentAreaPropertyModelTest
    {
		private Mock<ContentLoaderService> _mockContentLoaderService;
        private Mock<ContentConvertingService> _mockContentConvertingService;
        private Mock<IContentAccessEvaluator> _mockContentAccessEvaluator;
        private Mock<ISecurityPrincipal> _mockPrincipalAccessor;
        public ContentAreaPropertyModelTest()
        {
            _mockContentLoaderService = new Mock<ContentLoaderService>();
            _mockContentConvertingService = new Mock<ContentConvertingService>();
            _mockContentAccessEvaluator = new Mock<IContentAccessEvaluator>();
            _mockPrincipalAccessor = new Mock<ISecurityPrincipal>();
        }

        private ContentAreaPropertyModel CreateMockPropertyModel(PropertyContentArea property, bool excludePersonalizedContent, bool isContentManagementRequest = false)
        {
            return new ContentAreaPropertyModel(property,
                new TestConverterContext(excludePersonalizedContent,
                    isContentManagementRequest ? ContextMode.Edit : ContextMode.Default, isContentManagementRequest),
                _mockContentLoaderService.Object, _mockContentConvertingService.Object,
                _mockContentAccessEvaluator.Object, _mockPrincipalAccessor.Object);
        }

        public class Constructor : ContentAreaPropertyModelTest
        {
            public class WhenPropertyIsNull : Constructor
            {
                [Fact]
                public void ItShouldThrowArgumentNulLException()
                {
                    Assert.Throws<ArgumentNullException>(() => CreateMockPropertyModel(null, true));
                }
            }

            public class WhenContentAreaIsNull : Constructor
            {
                [Fact]
                public void ValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentArea();
                    Assert.Null(CreateMockPropertyModel(mockProperty, false).Value);
                }
            }

            public class WhenContentAreaIsNotNull : Constructor
            {
                [Fact]
                public void ValueShouldNotNull()
                {
                    var mockProperty = new PropertyContentArea
                    {
                        Value = new ContentArea()
                    };
                    Assert.NotNull(CreateMockPropertyModel(mockProperty, false).Value);
                }
            }
        }

        public class Expand : ContentAreaPropertyModelTest
        {
            public class WhenValueIsNull : Expand
            {
                [Fact]
                public void ExpandedValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentArea();
                    var contentAreaPropertyModel = CreateMockPropertyModel(mockProperty, false);
                    contentAreaPropertyModel.Expand(new CultureInfo("en"));

                    Assert.Null(contentAreaPropertyModel.ExpandedValue);
                }
            }

            public class WhenValueIsEmpty : Expand
            {
                [Fact]
                public void ExpandedValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentArea();
                    var contentAreaPropertyModel = CreateMockPropertyModel(mockProperty, false);                    
                    contentAreaPropertyModel.Expand(new CultureInfo("en"));

                    Assert.Null(contentAreaPropertyModel.ExpandedValue);
                }
            }

            public class WhenValueIsNotNullAndEmpty : Expand
            {
                [Fact]
                public void ExpandedValueShouldNotNull()
                {
                    var mockProperty = new PropertyContentArea();
                    var contentAreaPropertyModel = CreateMockPropertyModel(mockProperty, false);
                    contentAreaPropertyModel.Value = new List<ContentAreaItemModel>() { new ContentAreaItemModel() };
                    contentAreaPropertyModel.Expand(new CultureInfo("en"));

                    Assert.NotNull(contentAreaPropertyModel.ExpandedValue);
                }

                [Fact]
                public void RequestFromCMA_ExpandedValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentArea();
                    var contentAreaPropertyModel = CreateMockPropertyModel(mockProperty, false, true);
                    contentAreaPropertyModel.Value = new List<ContentAreaItemModel>() { new ContentAreaItemModel() };
                    contentAreaPropertyModel.Expand(new CultureInfo("en"));

                    Assert.Null(contentAreaPropertyModel.ExpandedValue);
                }
            }
        }
    }
}
