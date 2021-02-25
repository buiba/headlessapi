using System;
using Xunit;
using EPiServer.SpecializedProperties;
using Moq;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Security;
using System.Globalization;
using System.Security.Principal;
using System.Linq;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class TestPrincipal : IPrincipal
    {
        public IIdentity Identity
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsInRole(string role)
        {
            return true;
        }
    }

    public class ContentReferenceListPropertyModelTest
    {
        private Mock<IPermanentLinkMapper> _mockLinkMapper;
		private Mock<ContentLoaderService> _mockContentLoaderService;
        private Mock<ContentConvertingService> _mockContentConvertingService;
        private Mock<IContentAccessEvaluator> _mockContentAccessEvaluator;
        private Mock<ISecurityPrincipal> _mockPrincipalAccessor;
        private Mock<UrlResolverService> _mockUrlResolverService;

        public ContentReferenceListPropertyModelTest()
        {
            _mockLinkMapper = new Mock<IPermanentLinkMapper>();
            _mockContentLoaderService = new Mock<ContentLoaderService>();
            _mockContentConvertingService = new Mock<ContentConvertingService>();
            _mockContentConvertingService.Setup(c => c.ConvertToContentApiModel(It.IsAny<IContent>(), It.IsAny<ConverterContext>())).Returns(new ContentApiModel());
            _mockContentAccessEvaluator = new Mock<IContentAccessEvaluator>();
            _mockPrincipalAccessor = new Mock<ISecurityPrincipal>();
            _mockUrlResolverService = new Mock<UrlResolverService>();
        }

        private ContentReferenceListPropertyModel CreateMockPropertyModel(PropertyContentReferenceList property, bool excludePersonalizedContent, bool isContentManagementRequest = false)
        {
            return new ContentReferenceListPropertyModel(property,
                new TestConverterContext(excludePersonalizedContent, isContentManagementRequest ? ContextMode.Edit : ContextMode.Default, isContentManagementRequest),
                _mockLinkMapper.Object, _mockContentLoaderService.Object, _mockContentConvertingService.Object,
                _mockContentAccessEvaluator.Object, _mockPrincipalAccessor.Object, _mockUrlResolverService.Object);
        }

        public class Constructor : ContentReferenceListPropertyModelTest
        {
            public class WhenPropertyIsNull : Constructor
            {
                [Fact]
                public void ItShouldThrowArgumentNulLException()
                {
                    Assert.Throws<ArgumentNullException>(() => CreateMockPropertyModel(null, true));
                }
            }

            public class WhenPropertyIsNotNullAndListIsNull : Constructor
            {
                [Fact]
                public void ValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentReferenceList();
                    mockProperty.List = null;
                    Assert.Null(CreateMockPropertyModel(mockProperty, false).Value);
                }
            }

            public class WhenPropertyIsNotNullAndListHasNoElement : Constructor
            {
                [Fact]
                public void ValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentReferenceList();
                    mockProperty.List = new List<ContentReference>();
                    Assert.Null(CreateMockPropertyModel(mockProperty, false).Value);
                }
            }

            public class WhenPropertyIsNotNullAndListIsNotNull : Constructor
            {
                [Fact]
                public void ValueCountShouldEqualToPropertyListCount()
                {                    
                    var mockProperty = new PropertyContentReferenceList();
                    var mockContentReferenceList = new List<ContentReference>() { new ContentReference() };
                    mockProperty.List = mockContentReferenceList;
                    Assert.True(CreateMockPropertyModel(mockProperty, false).Value.Count == mockContentReferenceList.Count);
                }
            }
        }

        public class Expand : ContentReferenceListPropertyModelTest
        {
            public class IfValueIsNull : Expand
            {
                [Fact]
                public void ExpandedValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentReferenceList(new List<ContentReference>() { });
                    var contentReferenceListPropertyModel = CreateMockPropertyModel(mockProperty, false);

                    contentReferenceListPropertyModel.Expand(new CultureInfo("en"));

                    Assert.Null(contentReferenceListPropertyModel.ExpandedValue);
                }
            }

            public class IfValueIsNotNull : Expand
            {
                public class IfExcludePersonalizedContentIsTrue : IfValueIsNotNull
                {
                    [Fact]
                    public void ItShouldUseAnonymousPrincipalAndAddFilteredContentToExpandedValue()
                    {
                        var mockProperty = new PropertyContentReferenceList(new List<ContentReference>() { new ContentReference() });
                        _mockContentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<string>())).Returns(new List<IContent>() { new Mock<IContent>().Object });
                        _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);
                        var contentReferenceListPropertyModel = CreateMockPropertyModel(mockProperty, true);

                        contentReferenceListPropertyModel.Expand(new CultureInfo("en"));
                        
                        _mockContentAccessEvaluator.Verify(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>()), Times.Once);
                        Assert.True(contentReferenceListPropertyModel.ExpandedValue.Any());
                    }
                }

                public class IfContentManagementRequestIsTrue : IfValueIsNotNull
                {
                    [Fact]
                    public void ItShouldIgnoreExpandedValue()
                    {
                        var mockProperty = new PropertyContentReferenceList(new List<ContentReference>() { new ContentReference() });
                        var contentReferenceListPropertyModel = CreateMockPropertyModel(mockProperty, true, true);

                        contentReferenceListPropertyModel.Expand(new CultureInfo("en"));

                        Assert.Null(contentReferenceListPropertyModel.ExpandedValue);
                    }
                }

                public class IfExcludePersonalizedContentIsFalse : IfValueIsNotNull
                {                    

                    [Fact]
                    public void ItShouldUseCurrentPrincipalAndAddFilteredContentToExpandedValue()
                    {
                        var mockProperty = new PropertyContentReferenceList(new List<ContentReference>() { new ContentReference() });
                        _mockContentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<string>())).Returns(new List<IContent>() { new Mock<IContent>().Object });
                        _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);
                        _mockPrincipalAccessor.Setup(x => x.GetCurrentPrincipal()).Returns(new TestPrincipal());
                        var contentReferenceListPropertyModel = CreateMockPropertyModel(mockProperty, false);

                        contentReferenceListPropertyModel.Expand(new CultureInfo("en"));

                        _mockContentAccessEvaluator.Verify(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<TestPrincipal>(), It.IsAny<AccessLevel>()), Times.Once);
                        Assert.True(contentReferenceListPropertyModel.ExpandedValue.Any());
                    }
                }
            }
        }

    }
}
