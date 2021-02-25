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
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void Update_WhenInPutModelIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().Update(Guid.NewGuid(), null, new SaveContentOptions()));
        }

        [Fact]
        public void Put_WhenCommmonDraftContentIsNull_ShouldThrowException()
        {
            _mockContentLoader
                .Setup(m => m.Get<IContent>(It.IsAny<ContentReference>()))
                .Returns(new PageData());

            _mockContentVersionRepository
                .Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns((ContentVersion)null);

            _mockPermanentLinkMapper
              .Setup(x => x.Find(It.IsAny<Guid>()))
              .Returns(() => new PermanentLinkMap(Guid.NewGuid(), ContentReference.EmptyReference));

            var subject = Subject(_mockContentRepository.Object);
            var contentGuid = Guid.NewGuid();
            var content = new ContentApiCreateModel
            {
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "en", DisplayName = "English" },
                Name = "New name"
            };
            var exception = Assert.Throws<ErrorException>(() => subject.Update(contentGuid, content, new SaveContentOptions()));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal($"The content with id '{contentGuid}' does not exist.", exception.Message);
        }

        [Fact]
        public void Update_WhenContentIsValid_ShouldCallTheSaveMethodOnce()
        {
            var content = new ImageData();
            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            var contentDraftVersion = new ContentVersion(ContentReference.StartPage, "name", VersionStatus.CheckedIn,
                DateTime.Now, "savedBy", "changedBy", 1, "en", true, true);

            _mockPermanentLinkMapper
                .Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(() => new PermanentLinkMap(Guid.NewGuid(), ContentReference.EmptyReference));

            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(contentDraftVersion);

            _mockContentRepository
                .Setup(m => m.Get<IContent>(It.IsAny<ContentReference>()))
                .Returns(content);

            _mockContentRepository
                .Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>()))
                .Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var inputModel = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "ImageData" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = VersionStatus.Published
            };

            var subject = Subject(_mockContentRepository.Object);
            subject.Update(Guid.NewGuid(), inputModel, new SaveContentOptions());

            _mockContentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            Assert.Equal(SaveAction.Publish, saveAction);
        }

        [Fact]
        public void Update_WhenLocalizableContent_AndNotProvideMasterLanguage_AndHasNonBranchSpecificProperty_ShouldThrow()
        {
            var content = new Mock<PageData>();
            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;
            var innerBlock = new InnerBlock();

            content.Setup(x => x.MasterLanguage).Returns(new CultureInfo("en"));

            var contentDraftVersion = new ContentVersion(ContentReference.StartPage, "name",
                VersionStatus.CheckedIn, DateTime.Now, "savedBy", "changedBy", 1, "en", true, true);

            innerBlock.Property.Add("Title", new PropertyString("original inner block title"));
            var propertyCollection = new PropertyDataCollection
            {
                new CustomBlockProperty(innerBlock)
                {
                    Name = "CustomBlockProperty"
                }
            };

            content.Setup(x => x.Property).Returns(propertyCollection);
            var blockPropertyModel = new BlockPropertyModel()
            {
                Name = "SomeBlock",
                Properties = new Dictionary<string, object>()
                {
                    { "title", null }
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

            _mockPermanentLinkMapper
                .Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(() => new PermanentLinkMap(Guid.NewGuid(), ContentReference.EmptyReference));
            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(contentDraftVersion);
            _mockContentRepository
                .Setup(m => m.Get<IContent>(It.IsAny<ContentReference>()))
                .Returns(content.Object);
            _mockContentRepository
                .Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>()))
                .Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var subject = Subject(_mockContentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.Update(Guid.NewGuid(), inputModel, new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("Cannot provide non-branch specific property 'CustomBlockProperty' when not passing the master language", exception.Message);
        }

        [Fact]
        public void Update_WhenProviderContentWithLanguageDoestNotAlreadyExist__ShouldThrow()
        {
            var content = new Mock<PageData>();
            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;
            var innerBlock = new InnerBlock();

            content.Setup(x => x.MasterLanguage).Returns(new CultureInfo("sv"));

            var contentDraftVersion = new ContentVersion(ContentReference.StartPage, "name",
                VersionStatus.CheckedIn, DateTime.Now, "savedBy", "changedBy", 1, "sv", true, true);

            innerBlock.Property.Add("Title", new PropertyString("original inner block title"));
            var propertyCollection = new PropertyDataCollection
            {
                new CustomBlockProperty(innerBlock)
                {
                    Name = "CustomBlockProperty"
                }
            };

            content.Setup(x => x.Property).Returns(propertyCollection);
            var blockPropertyModel = new BlockPropertyModel()
            {
                Name = "SomeBlock",
                Properties = new Dictionary<string, object>()
                {
                    { "title", null }
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

            _mockPermanentLinkMapper
                .Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(() => new PermanentLinkMap(Guid.NewGuid(), ContentReference.EmptyReference));
            _mockContentVersionRepository
                .Setup(m => m.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(contentDraftVersion);
            _mockContentRepository
                .Setup(m => m.Get<IContent>(It.IsAny<ContentReference>()))
                .Returns(content.Object);
            _mockContentRepository
                .Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>()))
                .Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var contentGuid = new Guid("25741514-A839-4624-936C-66AAE62578F4");

            var subject = Subject(_mockContentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.Update(contentGuid, inputModel, new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal($"The provided language does not exist for content with id '{contentGuid}'.", exception.Message);
        }
    }
}
