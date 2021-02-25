using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Security;
using Moq;
using System;
using System.Globalization;
using System.Security.Principal;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
	public class PageReferencePropertyModelTest
	{
		private Mock<ContentLoaderService> _mockContentLoaderService;
		private Mock<ContentConvertingService> _mockContentConvertingService;
		private Mock<IContentAccessEvaluator> _mockContentAccessEvaluator;
		private Mock<ISecurityPrincipal> _mockPrincipalAccessor;
        private Mock<UrlResolverService> _mockUrlResolverService;

        public PageReferencePropertyModelTest()
		{
			_mockContentLoaderService = new Mock<ContentLoaderService>();
			_mockContentConvertingService = new Mock<ContentConvertingService>();
			_mockContentAccessEvaluator = new Mock<IContentAccessEvaluator>();
			_mockPrincipalAccessor = new Mock<ISecurityPrincipal>();
            _mockUrlResolverService = new Mock<UrlResolverService>();
        }

        private PageReferencePropertyModel CreateMockPropertyModel(PropertyPageReference propertyData, bool excludePersonalizedContent)
		{
			return new PageReferencePropertyModel(propertyData, new TestConverterContext(excludePersonalizedContent), _mockContentLoaderService.Object, _mockContentConvertingService.Object, _mockContentAccessEvaluator.Object, _mockPrincipalAccessor.Object, _mockUrlResolverService.Object);
		}

		public class Constructor : PageReferencePropertyModelTest
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
					var mockProperty = new PropertyPageReference();
					Assert.Null(CreateMockPropertyModel(mockProperty, false).Value);
				}
			}

			public class WhenPropertyIsNotNullAndContentLinkIsNotNull : Constructor
			{
				[Fact]
				public void ValueShouldNotBeNull()
				{
					var mockProperty = new Mock<PropertyPageReference>();
					mockProperty.Setup(x => x.ProviderName).Returns("Quan Provider");
					mockProperty.Setup(x => x.GuidValue).Returns(Guid.NewGuid());
					mockProperty.Setup(x => x.ContentLink).Returns(new PageReference(1));

					Assert.NotNull(CreateMockPropertyModel(mockProperty.Object, false).Value);
				}
			}
		}

		public class Expand : PageReferencePropertyModelTest
		{
			public class IfValueIsNull : Expand
			{
				[Fact]
				public void ExpandedValueShouldBeNull()
				{
					var mockProperty = new PropertyPageReference();
					var pageReferencePropertyModel = CreateMockPropertyModel(mockProperty, false);

					pageReferencePropertyModel.Expand(new CultureInfo("en"));

					Assert.Null(pageReferencePropertyModel.ExpandedValue);
				}
			}

			public class IfValueIsNotNull : Expand
			{
				private Mock<PropertyPageReference> _mockProperty;
                public IfValueIsNotNull()
				{
					_mockProperty = new Mock<PropertyPageReference>();
					_mockProperty.Setup(x => x.ProviderName).Returns("Quan Provider");
					_mockProperty.Setup(x => x.GuidValue).Returns(Guid.NewGuid());
					_mockProperty.Setup(x => x.ContentLink).Returns(new PageReference(1));
                    _mockContentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(Mock.Of<IContent>());

                }

                public class IfCannotGetContentFromContentLoader : IfValueIsNotNull
				{
					[Fact]
					public void ExpandedValueShouldBeNull()
					{
						_mockContentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns((IContent)null);
                        var pageReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, false);
                     
                        pageReferencePropertyModel.Expand(new CultureInfo("en"));

						Assert.Null(pageReferencePropertyModel.ExpandedValue);
					}
				}

				public class IfCanGetContentFromContentLoader : IfValueIsNotNull
				{
					public IfCanGetContentFromContentLoader()
					{
                        _mockContentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(Mock.Of<IContent>());
                        _mockContentConvertingService.Setup(x => x.ConvertToContentApiModel(It.IsAny<IContent>(), It.IsAny<ConverterContext>())).Returns(new ContentApiModel());
					}

					public class IfExcludePersonalizedContentIsTrue : IfCanGetContentFromContentLoader
					{
						public class IfAnonymousPricipalDoesNotHaveAccessWithContent : IfExcludePersonalizedContentIsTrue
						{
							[Fact]
							public void ExpandedValueShouldBeNull()
							{
								_mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);
								var pageReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, true);

								pageReferencePropertyModel.Expand(new CultureInfo("en"));

								Assert.Null(pageReferencePropertyModel.ExpandedValue);
							}
						}

						public class IfAnonymousPricipalHaveAccessWithContent : IfExcludePersonalizedContentIsTrue
						{
							[Fact]
							public void ExpandedValueShouldNotNull()
							{
								_mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);
								var pageReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, true);

								pageReferencePropertyModel.Expand(new CultureInfo("en"));

								Assert.NotNull(pageReferencePropertyModel.ExpandedValue);
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
								var pageRencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, false);

								pageRencePropertyModel.Expand(new CultureInfo("en"));

								Assert.Null(pageRencePropertyModel.ExpandedValue);
							}
						}

						public class IfCurrentPrincipalHaveAccessWithContent : IfExcludePersonalizedContentIsTrue
						{
							[Fact]
							public void ExpandedValueShouldNotNull()
							{
								_mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<GenericPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);
								_mockContentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<TestPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);
								var pageReferencePropertyModel = CreateMockPropertyModel(_mockProperty.Object, false);

								pageReferencePropertyModel.Expand(new CultureInfo("en"));

								Assert.NotNull(pageReferencePropertyModel.ExpandedValue);
							}
						}
					}
				}
			}
		}
	}
}
