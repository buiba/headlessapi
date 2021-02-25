using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.Core;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Filtering
{
	public class ContentApiSiteFilterTests
	{
		private Mock<IContentLoader> _contentLoader;
        private ContentApiSiteFilter siteFilter;
        private ContentApiConfiguration _apiConfig;
        private Mock<ISiteDefinitionResolver>  siteDefinitionResolver;
        private SiteDefinition siteDefinition;

        public ContentApiSiteFilterTests()
		{
			_contentLoader = new Mock<IContentLoader>();
            _apiConfig = new ContentApiConfiguration();
            siteDefinitionResolver = new Mock<ISiteDefinitionResolver>();

            siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid()
            };
            siteDefinitionResolver.Setup(x => x.GetByContent(It.IsAny<ContentReference>(), It.IsAny<bool>()))
            .Returns(siteDefinition);

            siteFilter = new ContentApiSiteFilter(siteDefinitionResolver.Object, _apiConfig, _contentLoader.Object);
        }

        [Fact]
		public void ShouldFilterContent_ShouldThrowArgumentException_WhenContentIsNull()
		{
			Assert.Throws<ArgumentException>(() => siteFilter.ShouldFilterContent(null, SiteDefinition.Current));
		}

		[Fact]
		public void ShouldFilterContent_ShouldNotFilterContent_WhenFilteringIsDisabled()
		{
            _apiConfig.Default().SetMultiSiteFilteringEnabled(false);

            Assert.False(siteFilter.ShouldFilterContent(new PageData(), new SiteDefinition()));
		}

		[Fact]
		public void ShouldFilterContent_ShouldFilterContent_WhenSiteMismatchAndFilteringIsEnabled()
		{
            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

			Assert.True(siteFilter.ShouldFilterContent(new PageData(), new SiteDefinition()));
		}

		[Fact]
		public void ShouldFilterContent_ShouldNotFilterContent_WhenSiteMatchAndFilteringIsEnabled()
		{
            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

            Assert.False(siteFilter.ShouldFilterContent(new PageData(), siteDefinition));
		}

		[Fact]
		public void ShouldFilterContent_ShouldReturnFalse_WhenContentIsInGlobalAssetsFolder_AndFilteringIsEnabled()
		{
			var mockGlobalAssetFolder = new ContentFolder() { ContentLink = SiteDefinition.Current.GlobalAssetsRoot };
			_contentLoader.Setup(x => x.GetAncestors(It.IsAny<ContentReference>())).Returns(new List<IContent>() { mockGlobalAssetFolder });

            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

            Assert.False(siteFilter.ShouldFilterContent(new PageData(), siteDefinition));
		}

		[Fact]
		public void ShouldFilterContent_ShouldReturnFalse_WhenContentIsInGlobalAssetsFolder_AndFilteringIsDisabled()
		{
			var mockGlobalAssetFolder = new ContentFolder() { ContentLink = SiteDefinition.Current.GlobalAssetsRoot };
			_contentLoader.Setup(x => x.GetAncestors(It.IsAny<ContentReference>())).Returns(new List<IContent>() { mockGlobalAssetFolder });

            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

            Assert.False(siteFilter.ShouldFilterContent(new PageData(), siteDefinition));
		}

		[Fact]
		public void FilterContents_ShouldFilterContent_WhenNull()
		{
            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

            var content = new List<IContent>()
			{
				null
			};

			Assert.Empty(siteFilter.FilterContents(content, siteDefinition));
		}

		[Fact]
		public void FilterContents_ShouldFilterContent_WhenSiteMismatchAndFilteringIsEnabled()
		{
			var otherSiteDefinition = new SiteDefinition();
			var siteContentLink = new ContentReference(12);
			var otherSiteContentLink = new ContentReference(13);

			siteDefinitionResolver.Setup(x => x.GetByContent(It.Is<ContentReference>(reference => reference == siteContentLink), It.IsAny<bool>()))
				.Returns(siteDefinition);
			siteDefinitionResolver.Setup(x => x.GetByContent(It.Is<ContentReference>(reference => reference == otherSiteContentLink), It.IsAny<bool>()))
				.Returns(otherSiteDefinition);

            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

            var content = new List<IContent>()
			{
				new BasicContent()
				{
					ContentLink = siteContentLink
				},
				new BasicContent()
				{
					ContentLink = otherSiteContentLink
				},
			};

			Assert.Single(siteFilter.FilterContents(content, siteDefinition), x => x.ContentLink == siteContentLink);
		}
	}
}
