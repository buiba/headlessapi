using System;
using System.Globalization;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void GetCommonDraft_ByContentGuid_WhenContentIsNotExposedToApi_ShouldThrowException()
        {
            var permanentLinkMap = new PermanentLinkMap(Guid.NewGuid(), _pageID);
            _mockPermanentLinkMapper.Setup(m => m.Find(_contentGuidTest)).Returns(permanentLinkMap);
            var content = new Mock<PageData>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(content.Object);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(false);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject().GetCommonDraft(_contentGuidTest, _language));
            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void GetCommonDraft_ByContentReference_WhenContentIsNotExposedToApi_ShouldThrowException()
        {
            var content = new Mock<PageData>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(content.Object);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(false);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject().GetCommonDraft(_pageID, _language));
            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void GetCommonDraft_ByContentReference_WhenContentIsNotExistInLanguageBranch_ShouldReturnNull()
        {
            _mockContentVersionRepository.Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(() => null);

            var result = Subject().GetCommonDraft(_pageID, _language);
            Assert.Null(result);
        }

        [Fact]
        public void GetCommonDraft_ByContentReference_WhenContentIsNotExist_ShouldThrowException()
        {
            var expectedException = new ContentNotFoundException($"Content with id {_pageID} was not found");
            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Throws(expectedException);
            var problemDetail = Assert.Throws<ErrorException>(() => Subject().GetCommonDraft(_pageID, _language));

            Assert.Equal(HttpStatusCode.NotFound, problemDetail.StatusCode);
            Assert.Equal(expectedException.Message, problemDetail.ErrorResponse.Error.Message);
        }

        [Fact]
        public void GetCommonDraft_ByContentReference_WhenContentExist_ShouldReturnCorrectContent()
        {
            var content = new Mock<IContent>();
            var contentDraftVersion = new ContentVersion(ContentReference.StartPage, "name", VersionStatus.CheckedIn,
                DateTime.Now, "savedBy", "changedBy", 1, _language, true, true);

            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), _language))
                .Returns(contentDraftVersion);
            _mockContentRepository.Setup(m => m.Get<IContent>(contentDraftVersion.ContentLink))
                .Returns(content.Object);

            var result = Subject(_mockContentRepository.Object).GetCommonDraft(_pageID, _language);
            Assert.Equal(content.Object.ContentLink, result.ContentLink);
        }

        [Fact]
        public void GetCommonDraft_ByContentReference_WhenLanguageIsNull_ShouldReturnMasterLanguageContent()
        {
            var content = new Mock<PageData>();
            const string masterLanguage = "en";
            content.Setup(c => c.MasterLanguage).Returns(new CultureInfo(masterLanguage));
            content.Setup(c => c.Language).Returns(new CultureInfo(masterLanguage));

            var contentDraftVersion = new ContentVersion(ContentReference.StartPage, "name", VersionStatus.CheckedIn,
                DateTime.Now, "savedBy", "changedBy", 1, masterLanguage, true, true);

            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), masterLanguage))
                .Returns(contentDraftVersion);
            _mockContentLoader.Setup(m => m.Get<IContent>(contentDraftVersion.ContentLink))
                .Returns(content.Object);
            _mockContentRepository.Setup(m => m.Get<IContent>(contentDraftVersion.ContentLink))
                .Returns(content.Object);

            var result = Subject(_mockContentRepository.Object).GetCommonDraft(contentDraftVersion.ContentLink, null);
            Assert.Equal(content.Object.ContentLink, result.ContentLink);
            Assert.Equal(content.Object.MasterLanguage, (result as ILocalizable)?.Language);
        }

        [Fact]
        public void GetCommonDraft_ByContentGuid_WhenPermanentLinkMapIsNull_ShouldThrowException()
        {
            _mockPermanentLinkMapper.Setup(m => m.Find(_contentGuidTest)).Returns(() => null);

            var result = Assert.Throws<ErrorException>(() => Subject().GetCommonDraft(_contentGuidTest, _language));
            Assert.Equal(HttpStatusCode.NotFound, result.ErrorResponse.StatusCode);
            Assert.Equal($"Content with guid {_contentGuidTest} was not found", result.ErrorResponse.Error.Message);
        }

        [Fact]
        public void GetCommonDraft_ByContentGuid_WhenPermanentLinkMapIsNotNull_AndContentIsNotExistInLanguageBranch_ShouldReturnNull()
        {
            var content = new Mock<IContent>();
            var contentDraftVersion = new ContentVersion(ContentReference.StartPage, "name", VersionStatus.CheckedIn,
                DateTime.Now, "savedBy", "changedBy", 1, _language, true, true);
            var permanentLinkMap = new PermanentLinkMap(Guid.NewGuid(), _pageID);

            _mockPermanentLinkMapper.Setup(m => m.Find(_contentGuidTest)).Returns(permanentLinkMap);
            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(() => null);

            var result = Subject().GetCommonDraft(_contentGuidTest, _language);
            Assert.Null(result);
        }

        [Fact]
        public void GetCommonDraft_ByContentGuid_WhenPermanentLinkMapIsNotNull_ShouldReturnCorrectContent()
        {
            var content = new Mock<IContent>();
            var contentDraftVersion = new ContentVersion(ContentReference.StartPage, "name", VersionStatus.CheckedIn,
                DateTime.Now, "savedBy", "changedBy", 1, _language, true, true);
            var permanentLinkMap = new PermanentLinkMap(Guid.NewGuid(), _pageID);

            _mockPermanentLinkMapper.Setup(m => m.Find(_contentGuidTest)).Returns(permanentLinkMap);
            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(permanentLinkMap.ContentReference, _language))
                .Returns(contentDraftVersion);
            _mockContentRepository.Setup(m => m.Get<IContent>(contentDraftVersion.ContentLink))
                .Returns(content.Object);

            var result = Subject(_mockContentRepository.Object).GetCommonDraft(_contentGuidTest, _language);
            Assert.Equal(content.Object.ContentLink, result.ContentLink);
        }
    }
}
