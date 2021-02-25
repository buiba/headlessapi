using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Modules;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Business
{
    public class ContentLoaderServiceTest : TestBase
    {
        private readonly ContentReference _pageID_1 = new ContentReference(1);
        private readonly ContentReference _pageID_2 = new ContentReference(2);

        private static readonly Guid PUBLISHED_PAGE_GUID = Guid.NewGuid();
        private static readonly int PUBLISHED_PAGE_LINK = 11;
        private static readonly int DRAFT_PAGE_LINK = 12;
        private static readonly int EXPIRED_PAGE_LINK = 13;
        protected static readonly int UNAUTHORIZED_PAGE_LINK = 14;

        protected readonly PageData _unauthorizedPage;
        protected readonly PageData _draftPage;
        protected readonly PageData _expiredPage;
        protected readonly PageData _publishedPage;
        protected PropertyDataCollection _publishedProperties = new PropertyDataCollection
        {
            { MetaDataProperties.PageLink, new PropertyContentReference(PUBLISHED_PAGE_LINK) },
            { MetaDataProperties.PageGUID, new PropertyString(PUBLISHED_PAGE_GUID.ToString()) },
            { MetaDataProperties.PagePendingPublish, new PropertyBoolean(false) },
            { MetaDataProperties.PageStopPublish, new PropertyDate(DateTime.UtcNow.AddDays(2)) },
            { MetaDataProperties.PageWorkStatus, new PropertyNumber(4) }
        };

        protected readonly Mock<IContentLoader> _defaultContentLoader;
        protected readonly new Mock<IPermanentLinkMapper> _permanentLinkMapper;
        protected readonly Mock<IContentProviderManager> _contentProviderManager;
        protected readonly Mock<IUrlResolver> _urlResolver;
        protected readonly new Mock<ContextModeResolver> _contextModeResolver;
        protected readonly ContentLoaderServiceExpose Subject;

        public ContentLoaderServiceTest()
        {
            var draftProperties = new PropertyDataCollection
            {
                { MetaDataProperties.PageLink, new PropertyContentReference(DRAFT_PAGE_LINK) },
                { MetaDataProperties.PagePendingPublish, new PropertyBoolean(true) },
                { MetaDataProperties.PageStopPublish, new PropertyDate(DateTime.UtcNow.AddDays(2)) },
               
            };

            var expiredProperties = new PropertyDataCollection
            {
                { MetaDataProperties.PageLink, new PropertyContentReference(EXPIRED_PAGE_LINK) },
                { MetaDataProperties.PagePendingPublish, new PropertyBoolean(false) },
                { MetaDataProperties.PageStopPublish, new PropertyDate(DateTime.UtcNow.AddDays(-2)) },

            };

            var unauthorizedProperties = new PropertyDataCollection
            {
                { "PageLink", new PropertyContentReference(UNAUTHORIZED_PAGE_LINK) },
                { "PagePendingPublish", new PropertyBoolean(false) },
                { "PageStopPublish", new PropertyDate(DateTime.UtcNow.AddDays(2)) },

            };

            _unauthorizedPage = new PageData(new AccessControlList(), unauthorizedProperties);
            _expiredPage = new PageData(new AccessControlList(), expiredProperties);
            _draftPage = new PageData(new AccessControlList(), draftProperties);
            _publishedPage = new PageData(new AccessControlList(), _publishedProperties) { Status = VersionStatus.Published };
            IContent content_1 = new PageData(new AccessControlList(), _publishedProperties) { Status = VersionStatus.Published , StopPublish = DateTime.UtcNow.AddDays(2) };

            _defaultContentLoader = new Mock<IContentLoader>();
            _defaultContentLoader.Setup(svc => svc.TryGet<IContent>(It.Is<ContentReference>(x => x == _pageID_1), It.IsAny<CultureInfo>(), out content_1)).Returns(true);
            _defaultContentLoader.Setup(svc => svc.TryGet<IContent>(It.Is<ContentReference>(x => x == _pageID_1), It.IsAny<LanguageSelector>(), out content_1)).Returns(true);
            _defaultContentLoader.Setup(svc => svc.Get<IContent>(It.Is<ContentReference>(x => x == _pageID_1), It.IsAny<LoaderOptions>())).Returns(new PageData(new AccessControlList(), _publishedProperties));           
            _defaultContentLoader.Setup(svc => svc.Get<IContent>(It.Is<ContentReference>(x => x == _pageID_2), It.IsAny<LoaderOptions>())).Returns(new PageData(new AccessControlList(), _publishedProperties));
            _defaultContentLoader.Setup(svc => svc.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<LanguageSelector>())).Returns(new List<IContent>() { new PageData() });

            _permanentLinkMapper = new Mock<IPermanentLinkMapper>();
            _permanentLinkMapper.Setup(p => p.Find(PUBLISHED_PAGE_GUID)).Returns(new PermanentLinkMap(PUBLISHED_PAGE_GUID, _pageID_1));

            _contentProviderManager = new Mock<IContentProviderManager>();

            _urlResolver = new Mock<IUrlResolver>();

            var moduleResourceResolver = new Mock<IModuleResourceResolver>();
            moduleResourceResolver.Setup(m => m.ResolvePath("CMS", null)).Returns("/episerver/cms");
            var apiConfig = new ContentApiConfiguration();
            apiConfig.Default().SetEnablePreviewMode(true);
            _contextModeResolver = new Mock<ContextModeResolver>(moduleResourceResolver.Object);

            Subject = new ContentLoaderServiceExpose(_defaultContentLoader.Object, _permanentLinkMapper.Object, _urlResolver.Object, _contextModeResolver.Object, _contentProviderManager.Object);
        }

        public class GetItemsWithOptions : ContentLoaderServiceTest
        {
            [Fact]
            public void ShouldReturnEmptyList_WhenReferenceList_IsNull()
            {
                Assert.Empty(Subject.GetItemsWithOptions((IEnumerable<ContentReference>)null, "en"));
            }

            [Fact]
            public void ShouldReturnEmptyList_WhenReferenceList_IsEmpty()
            {
                Assert.Empty(Subject.GetItemsWithOptions(Enumerable.Empty<ContentReference>(), "en"));
            }     

            [Fact]
            public void ShouldFilterOutDraftAndExpiredContent()
            {
                _defaultContentLoader.Setup(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<LanguageSelector>())).Returns(new List<PageData>() { _expiredPage, _draftPage, _publishedPage });
                var contents = Subject.GetItemsWithOptions(new List<ContentReference>() { _pageID_1, _pageID_2 }, "en");

                _defaultContentLoader.Verify(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.Is<LanguageSelector>(ls => ls.Language.Name == "en")), Times.Once);

                Assert.DoesNotContain(contents, x => x.ContentLink.Equals(_draftPage.ContentLink));
                Assert.DoesNotContain(contents, x => x.ContentLink.Equals(_expiredPage.ContentLink));
                Assert.Contains(contents, x => x.ContentLink.Equals(_publishedPage.ContentLink));
            }
        }

        public class GetAncestors : ContentLoaderServiceTest
        {
            public GetAncestors()
            {
                _defaultContentLoader.Setup(svc => svc.GetAncestors(It.Is<ContentReference>(re => re == _pageID_1))).Returns(new List<IContent>());
            }

            [Fact]
            public void ShouldReturnContents_WithFallbackLanguageSelector()
            {
                var expectedResult = Subject.GetAncestors(_pageID_1, "en");
                Assert.NotNull(expectedResult);
                _defaultContentLoader.Verify(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.Is<LanguageSelector>(ls => ls.Language.Name == "en")), Times.Once);
            }

            [Fact]
            public void ShouldThrowNotFoundException_WhenContentIsDraft()
            {
                Assert.Throws<ContentNotFoundException>(() => Subject.GetAncestors(_draftPage.ContentLink, "en"));
            }

            [Fact]
            public void ShouldThrowNotFoundException_WhenContentIsExpired()
            {
                Assert.Throws<ContentNotFoundException>(() => Subject.GetAncestors(_expiredPage.ContentLink, "en"));
            }

            [Fact]
            public void ShouldReturnContents_WhenContentGuidMatchesExistingContent()
            {
                var expectedResult = Subject.GetAncestors(PUBLISHED_PAGE_GUID, "en");
                Assert.NotNull(expectedResult);
                _defaultContentLoader.Verify(x => x.GetAncestors(_pageID_1), Times.Once);
            }

            [Fact]
            public void ShouldThrowNotFoundException_WhenContentWithGuidDoesNotExist()
            {
                Assert.Throws<ContentNotFoundException>(() => Subject.GetAncestors(Guid.NewGuid(), "en"));
            }

            [Fact]
            public void ShouldFilterOutDraftAndExpiredContent()
            {
                _defaultContentLoader.Setup(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<LanguageSelector>())).Returns(new List<PageData>() { _expiredPage, _draftPage, _publishedPage });
                var ancestors = Subject.GetAncestors(_pageID_1, "en");

                Assert.DoesNotContain(ancestors, x => x.ContentLink.Equals(_draftPage.ContentLink));
                Assert.DoesNotContain(ancestors, x => x.ContentLink.Equals(_expiredPage.ContentLink));
                Assert.Contains(ancestors, x => x.ContentLink.Equals(_publishedPage.ContentLink));
            }
        }        

        public class GetWithGuidAndLanguageString : ContentLoaderServiceTest
        {
            public GetWithGuidAndLanguageString()
            {
                _defaultContentLoader.Setup(svc => svc.Get<IContent>(It.IsAny<Guid>(), It.IsAny<LanguageSelector>())).Returns(new PageData(new AccessControlList(), _publishedProperties));
            }

            [Fact]
            public void ShouldReturnContents_WithFallbackLanguageSelector()
            {
                var expectedResult = Subject.Get(Guid.NewGuid(), "en");
                Assert.NotNull(expectedResult);
                _defaultContentLoader.Verify(x => x.Get<IContent>(It.IsAny<Guid>(), It.Is<LanguageSelector>(ls => ls.Language.Name == "en")), Times.Once);
            }

            [Fact]
            public void ShouldFilterOutDraftContent()
            {
                _defaultContentLoader.Setup(x => x.Get<IContent>(It.IsAny<Guid>(), It.IsAny<LanguageSelector>())).Returns(_draftPage);
                var content = Subject.Get(Guid.NewGuid(), "en");

                Assert.Null(content);               
            }

            [Fact]
            public void ShouldFilterOutExpiredContent()
            {
                _defaultContentLoader.Setup(x => x.Get<IContent>(It.IsAny<Guid>(), It.IsAny<LanguageSelector>())).Returns(_expiredPage);
                var content = Subject.Get(Guid.NewGuid(), "en");

                Assert.Null(content);
            }

            [Fact]
            public void ShouldReturnPublishedContent()
            {
                _defaultContentLoader.Setup(x => x.Get<IContent>(It.IsAny<Guid>(), It.IsAny<LanguageSelector>())).Returns(_publishedPage);
                var content = Subject.Get(Guid.NewGuid(), "en");

                Assert.NotNull(content);
            }
        }

        public class GetWithContentReferenceAndLanguageString : ContentLoaderServiceTest
        {
            [Fact]
            public void ShouldReturnContents_WithFallbackLanguageSelector()
            {
                var expectedResult = Subject.Get(_pageID_1, "en");
                Assert.NotNull(expectedResult);
                _defaultContentLoader.Verify(x => x.Get<IContent>(It.IsAny<ContentReference>(), It.Is<LanguageSelector>(ls => ls.Language.Name == "en")), Times.Once);
            }

            [Fact]
            public void ShouldFilterOutDraftContent()
            {
                _defaultContentLoader.Setup(x => x.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>())).Returns(_draftPage);
                var content = Subject.Get(_pageID_1, "en");

                Assert.Null(content);
            }

            [Fact]
            public void ShouldFilterOutExpiredContent()
            {
                _defaultContentLoader.Setup(x => x.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>())).Returns(_expiredPage);
                var content = Subject.Get(_pageID_1, "en");

                Assert.Null(content);
            }

            [Fact]
            public void ShouldReturnPublishedContent()
            {
                _defaultContentLoader.Setup(x => x.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>())).Returns(_publishedPage);
                var content = Subject.Get(_pageID_1, "en");

                Assert.NotNull(content);
            }
        }

        [Obsolete]
        public class GetByUrl : ContentLoaderServiceTest
        {
            [Fact]
            public void ShouldReturnContentWhenExactMatch()
            {
                var url = "http://localhost/en/some/content/url/";
                _urlResolver.Setup(u => u.Route(It.Is<UrlBuilder>(ub => url.Equals((string)ub)), ContextMode.Default)).Returns(Mock.Of<IContent>());
                Assert.NotNull(Subject.GetByUrl(url, true, true));
            }

            [Fact]
            public void ShouldReturnContentWhenSubPartMatchesAndExactMatchIsFalse()
            {
                var url = "http://localhost/en/some/content/url/";
                var contentUrl = "/en/some/content/";
                _urlResolver.Setup(u => u.Route(It.Is<UrlBuilder>(ub => contentUrl.Equals(ub.Path)), ContextMode.Default)).Returns(Mock.Of<IContent>());
                Assert.NotNull(Subject.GetByUrl(url, false, true));
            }

            [Fact]
            public void ShouldReturnNullWhenSubPartMatchesButExactMatchIsTrue()
            {
                var url = "http://localhost/en/some/content/url/";
                var contentUrl = "/en/some/content/";
                _urlResolver.Setup(u => u.Route(It.Is<UrlBuilder>(ub => contentUrl.Equals(ub.Path)), ContextMode.Default)).Returns(Mock.Of<IContent>());
                Assert.Null(Subject.GetByUrl(url, true, true));
            }

            [Fact]
            public void ShouldReturnContent_WhenEditModeIsTrue()
            {
                // Arrange
                var url = "http://localhost/episerver/cms,,123?epieditmode=True";
                var contentLoaderService = new ContentLoaderServiceExpose(_defaultContentLoader.Object, _permanentLinkMapper.Object, _urlResolver.Object, _contextModeResolver.Object, Mock.Of<IContentProviderManager>());
                _urlResolver.Setup(u => u.Route(It.Is<UrlBuilder>(ub => url.Equals((string)ub)), ContextMode.Edit)).Returns(Mock.Of<IContent>());

                // Act
                var result = contentLoaderService.GetByUrl(url, false, true);

                // Assert
                Assert.NotNull(result);
            }

            [Fact]
            public void ShouldReturnNull_WhenPreviewIsNotAllowed()
            {
                // Arrange
                var url = "http://localhost/episerver/cms,,123?epieditmode=True";
                var contentLoaderService = new ContentLoaderServiceExpose(_defaultContentLoader.Object, _permanentLinkMapper.Object, _urlResolver.Object, _contextModeResolver.Object, Mock.Of<IContentProviderManager>());
                _urlResolver.Setup(u => u.Route(It.Is<UrlBuilder>(ub => url.Equals((string)ub)), ContextMode.Edit)).Returns(Mock.Of<IContent>());

                // Act
                var result = contentLoaderService.GetByUrl(url, false, false);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void ShouldReturnContent_WhenPreviewModeIsTrue()
            {
                // Arrange
                var url = "http://localhost/episerver/cms,,123?epieditmode=False";
                var contentLoaderService = new ContentLoaderServiceExpose(_defaultContentLoader.Object, _permanentLinkMapper.Object, _urlResolver.Object, _contextModeResolver.Object, Mock.Of<IContentProviderManager>());
                _urlResolver.Setup(u => u.Route(It.Is<UrlBuilder>(ub => url.Equals((string)ub)), ContextMode.Preview)).Returns(Mock.Of<IContent>());

                // Act
                var result = contentLoaderService.GetByUrl(url, false, true);

                // Assert
                Assert.NotNull(result);
            }
        }

        public class ShouldContentBeExposed : ContentLoaderServiceTest
        {

            [Fact]
            public void ShouldReturnTrue_WhenContentIsDraft()
            {
                var shouldContentBeExposed = Subject.ShouldContentBeExposed_Exposed(_draftPage);
                Assert.False(shouldContentBeExposed);
            }

            [Fact]
            public void ShouldReturnTrue_WhenContentIsExpired()
            {
                var shouldContentBeExposed = Subject.ShouldContentBeExposed_Exposed(_expiredPage);
                Assert.False(shouldContentBeExposed);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsNotExpiredOrUnpublished()
            {
                var content = Subject.Get(_pageID_1, "en");
                Assert.True(Subject.ShouldContentBeExposed_Exposed(content));
                _defaultContentLoader.Verify(x => x.Get<IContent>(It.IsAny<ContentReference>(), It.Is<LanguageSelector>(ls => ls.Language.Name == "en")), Times.Once);
            }
        }

        public class GetContentInternal: ContentLoaderServiceTest
        {
            [Fact]
            public void ShouldReturnNull_WhenContentReferenceIsNullOrEmpty()
            {
                var result = Subject.Get(null, CultureInfo.CurrentCulture, true);
                Assert.Null(result);
            }

            [Fact]
            public void ShouldReturnContent_WhenAlwaysExposeContentIsTrue()
            {
                _defaultContentLoader
                    .Setup(x => x.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>()))
                    .Returns(_draftPage);

                var content = Subject.Get(_draftPage.ContentLink,  CultureInfo.GetCultureInfo("en"), alwaysExposeContent:true);

                _defaultContentLoader.Verify(
                    x => x.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>()),
                    Times.Once);
                Assert.Equal(content.ContentLink, _draftPage.ContentLink);
            }

            [Fact]
            public void ShouldNotReturnDraftContent_WhenAlwaysExposeContentIsFalse()
            {
                _defaultContentLoader
                    .Setup(x => x.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>()))
                    .Returns(_draftPage);

                var content = Subject.Get(_pageID_1,  CultureInfo.GetCultureInfo("en"), alwaysExposeContent:false);

                _defaultContentLoader.Verify(
                    x => x.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>()),
                    Times.Once);
                Assert.Null(content);
            }
        }
    }
}
