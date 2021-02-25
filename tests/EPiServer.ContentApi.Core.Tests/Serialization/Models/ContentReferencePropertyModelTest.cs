using System;
using System.Globalization;
using System.Security.Principal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.Security;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class ContentReferencePropertyModelTest
    {
        private Mock<ContentLoaderService> _mockContentLoaderService;
        private Mock<ContentConvertingService> _mockContentConvertingService;
        private Mock<IContentAccessEvaluator> _mockContentAccessEvaluator;
        private Mock<ISecurityPrincipal> _mockPrincipalAccessor;
        private Mock<UrlResolverService> _mockUrlResolverService;

        public ContentReferencePropertyModelTest()
        {
            _mockContentLoaderService = new Mock<ContentLoaderService>();
            _mockContentConvertingService = new Mock<ContentConvertingService>();
            _mockContentAccessEvaluator = new Mock<IContentAccessEvaluator>();
            _mockPrincipalAccessor = new Mock<ISecurityPrincipal>();
            _mockUrlResolverService = new Mock<UrlResolverService>();
        }

        private ContentReferencePropertyModel CreateMockPropertyModel(PropertyContentReference property,
            bool excludePersonalizedContent, bool isContentManagementRequest = false)
        {
            return new ContentReferencePropertyModel(property,
                new TestConverterContext(excludePersonalizedContent,
                    isContentManagementRequest ? ContextMode.Edit : ContextMode.Default), _mockContentLoaderService.Object,
                _mockContentConvertingService.Object, _mockContentAccessEvaluator.Object, _mockPrincipalAccessor.Object,
                _mockUrlResolverService.Object);
        }

        public class Constructor : ContentReferencePropertyModelTest
        {
            public class WhenPropertyIsNull : Constructor
            {
                [Fact]
                public void ItShouldThrowArgumentNulLException()
                {
                    Assert.Throws<ArgumentNullException>(() => CreateMockPropertyModel(null, true));
                }
            }

            public class WhenPropertyIsNotNullAndContentLinkIsNull : Constructor
            {
                [Fact]
                public void ValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentReference();
                    Assert.Null(CreateMockPropertyModel(mockProperty, false).Value);
                }
            }

            public class WhenPropertyIsNotNullAndContentLinkIsNotNull : Constructor
            {
                [Fact]
                public void ValueShouldNotNull()
                {
                    var mockProperty = new Mock<PropertyContentReference>();
                    mockProperty.Setup(x => x.ProviderName).Returns("Test Provider");
                    mockProperty.Setup(x => x.GuidValue).Returns(Guid.NewGuid());
                    mockProperty.Setup(x => x.ContentLink).Returns(new ContentReference(1));

                    Assert.NotNull(CreateMockPropertyModel(mockProperty.Object, false).Value);
                }

                [Fact]
                public void UrlShouldBeResolved()
                {
                    const string url = "http://somewhere.com/else";
                    var contentLink = new ContentReference(42);

                    var mockProperty = new Mock<PropertyContentReference>();
                    mockProperty.Setup(x => x.ContentLink).Returns(contentLink);

                    _mockUrlResolverService.Setup(x => x.ResolveUrl(contentLink, null)).Returns(url);

                    Assert.Equal(url, CreateMockPropertyModel(mockProperty.Object, false).Value.Url);
                }
            }
        }

        public class Expand : ContentReferencePropertyModelTest
        {
            public class IfValueIsNull : Expand
            {
                [Fact]
                public void ExpandedValueShouldBeNull()
                {
                    var mockProperty = new PropertyContentReference();
                    var contentReferencePropertyModel = CreateMockPropertyModel(mockProperty, false);

                    contentReferencePropertyModel.Expand(new CultureInfo("en"));

                    Assert.Null(contentReferencePropertyModel.ExpandedValue);
                }
            }

            public class IfValueIsNotNull : Expand
            {
                private Mock<PropertyContentReference> _mockProperty;

                public IfValueIsNotNull()
                {
                    _mockProperty = new Mock<PropertyContentReference>();
                    _mockProperty.Setup(x => x.ProviderName).Returns("Test Provider");
                    _mockProperty.Setup(x => x.GuidValue).Returns(Guid.NewGuid());
                    _mockProperty.Setup(x => x.ContentLink).Returns(new ContentReference(1));
                }

                public class IfCannotGetContentFromContentLoader : IfValueIsNotNull
                {
                    [Fact]
                    public void ExpandedValueShouldBeNull()
                    {
                        _mockContentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns((IContent)null);
                        var contentReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, false);

                        contentReferencePropertyModel.Expand(new CultureInfo("en"));

                        Assert.Null(contentReferencePropertyModel.ExpandedValue);
                    }
                }

                public class IfCanGetContentFromContentLoader : IfValueIsNotNull
                {
                    public IfCanGetContentFromContentLoader()
                    {
                        _mockContentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(Mock.Of<IContent>());
                        _mockContentConvertingService.Setup(x => x.ConvertToContentApiModel(It.IsAny<IContent>(), It.IsAny<ConverterContext>())).Returns(new ContentApiModel());
                    }

                    public class IfContentManagementRequestIsTrue : IfCanGetContentFromContentLoader
                    {
                        [Fact]
                        public void ItShouldIgnoreExpandedValue()
                        {
                            var contentReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, false, true);
                            contentReferencePropertyModel.Expand(new CultureInfo("en"));

                            Assert.Null(contentReferencePropertyModel.ExpandedValue);
                        }
                    }

                    public class IfExcludePersonalizedContentIsTrue : IfCanGetContentFromContentLoader
                    {
                        public class IfAnonymousPricipalDoesNotHaveAccessWithContent : IfExcludePersonalizedContentIsTrue
                        {
                            [Fact]
                            public void ExpandedValueShouldBeNull()
                            {
                                _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);
                                var contentReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, true);

                                contentReferencePropertyModel.Expand(new CultureInfo("en"));

                                Assert.Null(contentReferencePropertyModel.ExpandedValue);
                            }
                        }

                        public class IfAnonymousPricipalHaveAccessWithContent : IfExcludePersonalizedContentIsTrue
                        {
                            [Fact]
                            public void ExpandedValueShouldNotNull()
                            {
                                _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);
                                var contentReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, true);

                                contentReferencePropertyModel.Expand(new CultureInfo("en"));

                                Assert.NotNull(contentReferencePropertyModel.ExpandedValue);
                            }
                        }
                    }

                    public class IfExcludePersonalizedContentIsFalse : IfCanGetContentFromContentLoader
                    {
                        public class IfCurrentPrincipalDoesNotHaveAccessWithContent : IfExcludePersonalizedContentIsTrue
                        {
                            [Fact]
                            public void ExpandedValueShouldBeNull()
                            {
                                _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);
                                _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<TestPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);
                                var contentReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, false);

                                contentReferencePropertyModel.Expand(new CultureInfo("en"));

                                Assert.Null(contentReferencePropertyModel.ExpandedValue);
                            }
                        }

                        public class IfCurrentPrincipalHaveAccessWithContent : IfExcludePersonalizedContentIsTrue
                        {
                            [Fact]
                            public void ExpandedValueShouldNotNull()
                            {
                                _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);
                                _mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<TestPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);
                                var contentReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, false);

                                contentReferencePropertyModel.Expand(new CultureInfo("en"));

                                Assert.NotNull(contentReferencePropertyModel.ExpandedValue);
                            }
                        }
                    }
                }
            }
        }
    }
}
