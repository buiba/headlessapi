using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void Create_WhenInputContentIsNull_ShouldThrow()
        {
            var subject = Subject();

            Assert.Throws<ArgumentNullException>(() => subject.Create(null, new SaveContentOptions()));
        }

        [Fact]
        public void Create_WhenParentContentIsNotExposedToApi_ShouldThrowException()
        {
            var _ = _mockContent.Object;
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(false);

            var subject = Subject(_mockContentRepository.Object);
            var content = new ContentApiCreateModel
            {
                ContentType = new List<string>() { "SomeType" },
                ParentLink = new ContentReferenceInputModel() { Id = 1 },
            };
            var exception = Assert.Throws<ErrorException>(() => subject.Create(content, new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        }

        [Fact]
        public void Create_WhenClientSendBothParentIdAndParentGuid_ShouldGetParentById()
        {
            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.GetDefault<IContentData>(It.IsAny<ContentReference>(), It.IsAny<int>(), It.IsAny<CultureInfo>())).Returns(Mock.Of<PageData>());
            var _ = _mockContent.Object;
            contentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);

            var content = new ContentApiCreateModel
            {
                ContentType = new List<string>() { "SomeType" },
                ParentLink = new ContentReferenceInputModel() { Id = 1, GuidValue = Guid.NewGuid() },
                Language = new LanguageModel() { Name = "en" },
                Status = VersionStatus.CheckedOut
            };

            var subject = Subject(contentRepository: contentRepository.Object);

            subject.Create(content, new SaveContentOptions());

            contentRepository.Verify(x => x.TryGet(It.IsAny<ContentReference>(), out _), Times.Once);
            contentRepository.Verify(x => x.TryGet(It.IsAny<Guid>(), out _), Times.Never);
        }

        [Fact]
        public void Create_WhenClientDoesNotSendParentId_ShouldGetParentByGuid()
        {
            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.GetDefault<IContentData>(It.IsAny<ContentReference>(), It.IsAny<int>(), It.IsAny<CultureInfo>())).Returns(Mock.Of<PageData>());
            var _ = _mockContent.Object;
            contentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);

            var content = new ContentApiCreateModel
            {
                ContentType = new List<string>() { "SomeType" },
                ParentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                Language = new LanguageModel() { Name = "en" },
                Status = VersionStatus.CheckedOut
            };

            var subject = Subject(contentRepository: contentRepository.Object);

            subject.Create(content, new SaveContentOptions());

            contentRepository.Verify(x => x.TryGet<IContent>(It.IsAny<ContentReference>(), out _), Times.Never);
            contentRepository.Verify(x => x.TryGet<IContent>(It.IsAny<Guid>(), out _), Times.Once);
        }

        [Theory]
        [MemberData(nameof(InvalidParent))]
        public void Create_WhenParentIsInvalid_ShouldThrow(ContentReferenceInputModel parentLink)
        {
            var content = new ContentApiCreateModel
            {
                ContentType = new List<string>() { "SomeType" },
                ParentLink = parentLink
            };

            _mockContentRepository.Setup(x => x.Get<IContent>(It.IsAny<Guid>())).Throws(new ContentNotFoundException());
            var subject = Subject(_mockContentRepository.Object);

            var error = Assert.Throws<ErrorException>(() => subject.Create(content, new SaveContentOptions()));
            Assert.Equal(ProblemCode.InvalidParent, error.ErrorResponse.Error.Code);
        }

        [Fact]
        public void Create_WhenContentIsValid_ShouldCallTheSaveMethodOnce()
        {
            var contentRepository = new Mock<IContentRepository>();

            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });
            contentRepository.Setup(x => x.GetDefault<IContentData>(It.IsAny<ContentReference>(), It.IsAny<int>(), It.IsAny<CultureInfo>())).Returns(Mock.Of<PageData>());
            var _ = _mockContent.Object;
            contentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);

            var inputModel = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "PageData" },
                Language = new LanguageModel() { Name = "en" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = VersionStatus.Published
            };

            var subject = Subject(contentRepository.Object);
            subject.Create(inputModel, new SaveContentOptions());

            contentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            Assert.Equal(SaveAction.Publish | SaveAction.ForceNewVersion, saveAction);
        }
    }
}
