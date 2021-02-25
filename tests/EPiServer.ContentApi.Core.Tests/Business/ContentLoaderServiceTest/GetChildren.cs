using EPiServer.Core;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using EPiServer.Security;
using Xunit;
using System.Linq;
using System;

namespace EPiServer.ContentApi.Core.Tests.Business
{
    public class GetChildren : ContentLoaderServiceTest
    {
        private readonly ContentReference _pageID_100 = new ContentReference(100);
        private readonly ContentReference _pageID_999 = new ContentReference(999);
        private readonly Guid _pageGuid_100 = Guid.NewGuid();

        private IContent page_100;
        private IContent nullContent;
        private readonly PagingToken _pagingToken = new PagingToken()
        {
            LastIndex = 0,
            Top = 4,
            TotalCount = 8
        };

        public GetChildren()
        {
            page_100 = new PageData(new AccessControlList(), _publishedProperties);

            _defaultContentLoader.Setup(svc => svc.TryGet(It.Is<ContentReference>(x => x == _pageID_100), It.IsAny<CultureInfo>(), out page_100)).Returns(true);
            _defaultContentLoader.Setup(svc => svc.TryGet(It.Is<ContentReference>(x => x == _pageID_100), It.IsAny<LanguageSelector>(), out page_100)).Returns(true);

            _defaultContentLoader.Setup(svc => svc.TryGet(It.Is<ContentReference>(x => x == _pageID_999), It.IsAny<CultureInfo>(), out nullContent)).Returns(false);
            _defaultContentLoader.Setup(svc => svc.GetChildren<IContent>(It.Is<ContentReference>(re => re == _pageID_100), It.IsAny<LoaderOptions>(), It.IsAny<int>(), It.IsAny<int>()))
                                                                                    .Returns(new List<IContent>() { new PageData(new AccessControlList(), _publishedProperties) });

            _permanentLinkMapper.Setup(p => p.Find(_pageGuid_100)).Returns(new Web.PermanentLinkMap(_pageGuid_100, _pageID_100));
        }

        [Fact]
        public void ShouldReturnEmptyList_WhenContentReference_IsNull()
        {
            Assert.Throws<ContentNotFoundException>(() => Subject.GetChildren(null, "en"));
        }

        [Fact]
        public void ShouldReturnEmptyList_WhenContentReference_IsEmpty()
        {
            Assert.Throws<ContentNotFoundException>(() => Subject.GetChildren(ContentReference.EmptyReference, "en"));
        }

        [Fact]
        public void ShouldThrowNotFoundException_WhenContentDoesNotExist()
        {
            Assert.Throws<ContentNotFoundException>(() => Subject.GetChildren(_pageID_999, "en"));
        }

        [Fact]
        public void ShouldThrowNotFoundException_WhenContentWithGuidDoesNotExist()
        {
            Assert.Throws<ContentNotFoundException>(() => Subject.GetChildren(Guid.NewGuid(), "en", new PagingToken { Top = 5 }, c => true));
        }

        [Fact]
        public void ShouldThrowNotFoundException_WhenContentIsDraft()
        {
            Assert.Throws<ContentNotFoundException>(() => Subject.GetChildren(_draftPage.ContentLink, "en"));
        }

        [Fact]
        public void ShouldThrowNotFoundException_WhenContentIsExpired()
        {
            Assert.Throws<ContentNotFoundException>(() => Subject.GetChildren(_expiredPage.ContentLink, "en"));
        }

        [Fact]
        public void ShouldReturnChildren_WhenContentExisted()
        {
            Assert.Single(Subject.GetChildren(_pageID_100, "en"));
        }

        [Fact]
        public void ShouldFilterOutDraftAndExpiredContent()
        {
            _defaultContentLoader.Setup(x => x.GetChildren<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<PageData>() { _expiredPage, _draftPage, _publishedPage });
            var children = Subject.GetChildren(_pageID_100, "en");

            _defaultContentLoader.Verify(x => x.GetChildren<IContent>(It.IsAny<ContentReference>(), It.Is<LanguageSelector>(ls => ls.Language.Name == "en"), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Assert.DoesNotContain(children, x => x.ContentLink.Equals(_draftPage.ContentLink));
            Assert.DoesNotContain(children, x => x.ContentLink.Equals(_expiredPage.ContentLink));
            Assert.Contains(children, x => x.ContentLink.Equals(_publishedPage.ContentLink));
        }

        [Fact]
        public void UseDefaultValueIfTheyAreNotPassedThrough()
        {
            _defaultContentLoader.Setup(x => x.GetChildren<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<PageData>() { _expiredPage, _draftPage, _publishedPage });
            var children = Subject.GetChildren(_pageID_100, "en");
            _defaultContentLoader.Verify(x => x.GetChildren<IContent>(It.IsAny<ContentReference>(), It.Is<LanguageSelector>(ls => ls.Language.Name == "en"), It.Is<int>(startIndex => startIndex == PagingConstants.DefaultStartIndex), It.Is<int>(maxRows => maxRows == PagingConstants.DefaultMaxRows)), Times.Once);
        }

        private bool ReturnAllContentPredicate(IContent content)
        {
            return true;
        }

        private bool FilterUnauthorizeContentPredicate(IContent content)
        {
            if (content.ContentLink.ID == UNAUTHORIZED_PAGE_LINK)
            {
                return false;
            }

            return true;
        }

        [Fact]
        public void ShouldGetChildrenWithPaging_WhenPagingTokenPassed()
        {
            _defaultContentLoader.Setup(x => x.GetChildren<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<PageData>() { _publishedPage, _publishedPage, _publishedPage, _publishedPage });

            var contentQueryRange = Subject.GetChildren(_pageID_100, "en", _pagingToken, ReturnAllContentPredicate);

            Assert.True(contentQueryRange.PagedResult.PagedItems.Count() == 4);
        }

        [Fact]
        public void ShouldFilterChildrenByGivenPredicate_WhenPredicatePassed()
        {
            _defaultContentLoader.Setup(x => x.GetChildren<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<PageData>() { _unauthorizedPage, _publishedPage, _publishedPage, _publishedPage, _publishedPage, _publishedPage });

            var children = Subject.GetChildren(_pageID_100, "en", _pagingToken, FilterUnauthorizeContentPredicate);

            Assert.Equal(4, children.PagedResult.PagedItems.Count());
        }

        [Fact]
        public void ShouldReturnCorrectLastIndex_WhenNoSufficentRightToViewFirstItem()
        {
            var pagingToken = new PagingToken()
            {
                Top = 2,
                TotalCount = 18
            };

            _defaultContentLoader.Setup(x => x.GetChildren<IContent>(It.IsAny<ContentReference>(), It.IsAny<LanguageSelector>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<PageData>() { _unauthorizedPage, _publishedPage });

            var children = Subject.GetChildren(_pageID_100, "en", pagingToken, FilterUnauthorizeContentPredicate);

            Assert.Equal(2, children.PagedResult.PagedItems.Count());
            Assert.Equal(3, children.LastIndex);
        }
    }
}
