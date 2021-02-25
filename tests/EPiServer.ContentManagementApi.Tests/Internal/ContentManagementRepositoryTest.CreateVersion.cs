using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.ContentManagementApi.Serialization.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void CreateVersion_WhenInputContentIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().CreateVersion(Mock.Of<ContentReference>(), null, new SaveContentOptions()));
        }

        [Theory]
        [MemberData(nameof(InvalidInputContentLink))]
        public void CreateVersion_WhenInputContentLinkIsNullOrEmpty_ShouldThrow(ContentReference contentLink)
        {
            Assert.Throws<ArgumentNullException>(() => Subject().CreateVersion(contentLink, new ContentApiCreateModel(), new SaveContentOptions()));
        }

        [Fact]
        public void CreateVersion_WhenInputContentLinkIsNotFound_ShouldThrowNotFound()
        {
            var contentLink = new ContentReference(-1);
            _mockContentLoader.Setup(x => x.Get<IContent>(It.IsAny<ContentReference>())).Throws(new ContentNotFoundException($"The content with id '{contentLink}' does not exist."));
            var exception = Assert.Throws<ErrorException>(() => Subject().CreateVersion(contentLink, new ContentApiCreateModel(), new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal($"The content with id '{contentLink}' does not exist.", exception.Message);
        }

        [Fact]
        public void CreateVersion_WhenContentIsNotIVersionable_ShouldThrow()
        {
            var contentRepository = new Mock<IContentRepository>();
            var contentFolder = new Mock<ContentFolder>().Object;
            var content = contentFolder as IContent;
            var contentVersion = new ContentVersion(new ContentReference(1), "version name", VersionStatus.CheckedOut, DateTime.Now, "user", "status changed by", 1, "en", false, false);

            contentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out content)).Returns(true);
            contentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(content);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);

            var subject = Subject(contentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.CreateVersion(new ContentReference(1), new ContentApiCreateModel(), new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("Cannot create new version of non-versionable content", exception.Message);
        }

        [Fact]
        public void CreateVersion_WhenHasCommonDraft_AndValidContent_ShouldCallTheSaveMethodOnce()
        {
            var contentRepository = new Mock<IContentRepository>();
            var versionRepository = new Mock<IContentVersionRepository>();
            var contentVersion = new ContentVersion(new ContentReference(1), "version name", VersionStatus.CheckedOut, DateTime.Now, "user", "status changed by", 1, "en", false, false);

            var contentImage = new ImageData();
            var content = contentImage as IContent;

            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            contentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out content)).Returns(true);
            contentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(content);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);

            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var inputModel = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "ImageData" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = VersionStatus.Published
            };

            var subject = Subject(contentRepository.Object);
            subject.CreateVersion(new ContentReference(1), inputModel, new SaveContentOptions());

            contentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            _mockContentVersionRepository.Verify(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(SaveAction.Publish | SaveAction.ForceNewVersion, saveAction);
        }

        [Fact]
        public void CreateVersion_WhenNoCommonDraft_AndValidContent_ShouldCallCreateLanguageBranchOnce()
        {
            var contentRepository = new Mock<IContentRepository>();
            var versionRepository = new Mock<IContentVersionRepository>();
            var pageData = new PageData();
            pageData.Property.Add("PageMasterLanguageBranch", new PropertyString("en"));
            pageData.Property.Add("PageName", new PropertyString("page name"));
            pageData.Property.Add("PageURLSegment", new PropertyString("/test"));
            pageData.Property.Add("PageCategory", new PropertyCategory(new CategoryList()));
            pageData.Property.Add("PageStartPublish", new PropertyString("start publish"));
            pageData.Property.Add("PageStopPublish", new PropertyString("stop publish"));
            pageData.Property.Add("LanguageSpecificProperty", new PropertyString("stop publish") { IsLanguageSpecific = true });
            pageData.MasterLanguage = CultureInfo.GetCultureInfo("en");

            var content = pageData as IContent;
            ContentVersion contentVersion = null;

            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            contentRepository.Setup(x => x.TryGet(It.IsAny<ContentReference>(), out content)).Returns(true);
            contentRepository.Setup(m => m.CreateLanguageBranch<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>())).Returns(content);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);

            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var inputModel = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "ImageData" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = VersionStatus.Published,
                Language = new LanguageModel { Name = "en" }
            };

            var subject = Subject(contentRepository.Object);
            subject.CreateVersion(new ContentReference(1), inputModel, new SaveContentOptions());

            contentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            contentRepository.Verify(x => x.CreateLanguageBranch<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>()), Times.Once);
            _mockContentVersionRepository.Verify(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(SaveAction.Publish | SaveAction.ForceNewVersion, saveAction);
        }


        [Fact]
        public void CreateVersion_WhenLocalizableContent_AndNotProvideMasterLanguage_AndHaveNonBranchSpecificProperty_ShouldThrow()
        {
            var contentRepository = new Mock<IContentRepository>();
            var versionRepository = new Mock<IContentVersionRepository>();
            var pageData = new PageData();
            pageData.Property.Add("PageMasterLanguageBranch", new PropertyString("en"));
            pageData.Property.Add("PageName", new PropertyString("page name"));
            pageData.Property.Add("PageURLSegment", new PropertyString("/test"));
            pageData.Property.Add("PageCategory", new PropertyString("page category"));
            pageData.Property.Add("PageStartPublish", new PropertyString("start publish"));
            pageData.Property.Add("PageStopPublish", new PropertyString("stop publish"));
            pageData.Property.Add("NonBranchSpecific", new PropertyString("NonBranchSpecific") { IsLanguageSpecific = false });
            pageData.MasterLanguage = CultureInfo.GetCultureInfo("en");

            var content = pageData as IContent;
            ContentVersion contentVersion = null;

            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            contentRepository.Setup(x => x.TryGet(It.IsAny<ContentReference>(), out content)).Returns(true);
            contentRepository.Setup(m => m.CreateLanguageBranch<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>())).Returns(content);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);

            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var inputModel = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentType = new List<string>() { "PageData" },
                Language = new LanguageModel() { Name = "sv" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Properties = new Dictionary<string, object>
                {
                    { "NonBranchSpecific", null }
                }
            };

            var subject = Subject(contentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.CreateVersion(new ContentReference(1), inputModel, new SaveContentOptions()));

            _mockContentVersionRepository.Verify(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("Cannot provide non-branch specific property 'NonBranchSpecific' when not passing the master language", exception.Message);
        }

        [Fact]
        public void CreateVersion_WhenLocalizableContent_AndNotProvideMasterLanguage_AndHaveNestedNonBranchSpecificProperty_ShouldThrow()
        {
            var contentRepository = new Mock<IContentRepository>();
            var versionRepository = new Mock<IContentVersionRepository>();
            var pageData = new PageData();
            pageData.Property.Add("PageMasterLanguageBranch", new PropertyString("en"));
            pageData.Property.Add("PageName", new PropertyString("page name"));
            pageData.Property.Add("PageURLSegment", new PropertyString("/test"));
            pageData.Property.Add("PageCategory", new PropertyString("page category"));
            pageData.Property.Add("PageStartPublish", new PropertyString("start publish"));
            pageData.Property.Add("PageStopPublish", new PropertyString("stop publish"));

            var innerBlock = new InnerBlock();
            innerBlock.Property.Add("Title", new PropertyString("original inner block title"));

            pageData.Property.Add("CustomBlockProperty", new CustomBlockProperty(innerBlock)
            {
                Name = "CustomBlockProperty"
            });
            pageData.MasterLanguage = CultureInfo.GetCultureInfo("en");

            var content = pageData as IContent;
            ContentVersion contentVersion = null;

            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            contentRepository.Setup(x => x.TryGet(It.IsAny<ContentReference>(), out content)).Returns(true);
            contentRepository.Setup(m => m.CreateLanguageBranch<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>())).Returns(content);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);

            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var blockPropertyModel = new BlockPropertyModel()
            {
                Name = "SomeBlock",
                Properties = new Dictionary<string, object>()
                {
                    { "TITLE", null }
                }
            };

            var inputModel = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentType = new List<string>() { "PageData" },
                Language = new LanguageModel() { Name = "sv" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Properties = new Dictionary<string, object>
                {
                    { "CustomBlockProperty", blockPropertyModel }
                }
            };

            var subject = Subject(contentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.CreateVersion(new ContentReference(1), inputModel, new SaveContentOptions()));

            _mockContentVersionRepository.Verify(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("Cannot provide non-branch specific property 'CustomBlockProperty' when not passing the master language", exception.Message);
        }
    }
}
