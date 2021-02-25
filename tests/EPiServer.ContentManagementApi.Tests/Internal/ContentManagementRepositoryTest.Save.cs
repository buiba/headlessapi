using System;
using System.Collections.Generic;
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
        public void Save_WhenContentIsIVersionable_AndNotHaveStatus_ShouldThrowsException()
        {
            var content = new ContentApiCreateModel
            {
                Name = "Page",
                ContentType = new List<string>() { "SomeType" },
                ParentLink = new ContentReferenceInputModel() { Id = 1, GuidValue = Guid.NewGuid() },
                Language = new LanguageModel() { Name = "en" }
            };

            Assert.Throws<ErrorException>(() => Subject().Save(content, Mock.Of<PageData>(), new SaveContentOptions()));
        }

        [Fact]
        public void Save_WhenContentIsNotIVersionable_AndHaveStatus_ShouldThrowsException()
        {
            var content = new ContentApiCreateModel
            {
                ContentType = new List<string>() { "ContentFolder" },
                ParentLink = new ContentReferenceInputModel() { Id = 1, GuidValue = Guid.NewGuid() },
                Status = VersionStatus.Published
            };

            var error = Assert.Throws<ErrorException>(() => Subject().Save(content, Mock.Of<ContentFolder>(), new SaveContentOptions()));
            Assert.Equal(ProblemCode.ContentNotVersionable, error.ErrorResponse.Error.Code);
        }

        [Fact]
        public void Save_WhenContentIsNotIRoutable_AndHaveRouteSegment_ShouldThrowsException()
        {
            var content = new ContentApiCreateModel
            {
                ContentType = new List<string>() { "BasicContent" },
                ParentLink = new ContentReferenceInputModel() { Id = 1, GuidValue = Guid.NewGuid() },
                RouteSegment = "test/some-route"
            };

            var error = Assert.Throws<ErrorException>(() => Subject().Save(content, Mock.Of<BasicContent>(), new SaveContentOptions()));
            Assert.Equal(ProblemCode.ContentNotRoutable, error.ErrorResponse.Error.Code);
        }

        [Theory]
        [InlineData(ContentValidationMode.Minimal, true)]
        [InlineData(ContentValidationMode.Complete, false)]
        public void Save_WithValidationMode_ShouldHasCorrespondingSkipValidationFlag(ContentValidationMode validationMode, bool shouldContainSkipValidationFlag)
        {
            IContent saved = null;
            SaveAction saveAction = SaveAction.CheckOut;

            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "PageData" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.StartPage.ID },
                Status = VersionStatus.CheckedOut
            };

            var subject = Subject(contentRepository.Object);

            subject.Save(content, Mock.Of<PageData>(), new SaveContentOptions(validationMode));

            contentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            Assert.Equal(shouldContainSkipValidationFlag, saveAction.HasFlag(SaveAction.SkipValidation));
        }        

        [Fact]
        public void Save_WithNotVersionableContent_AndNoStatusAvailable_ShouldCallSaveOnInnerRepository_WithSaveActionDefault()
        {
            IContent saved = null;
            SaveAction saveAction = SaveAction.CheckOut;

            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "SysContentFolder" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID }
            };

            var subject = Subject(contentRepository.Object);

            subject.Save(content, Mock.Of<ContentFolder>(), new SaveContentOptions());

            contentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            Assert.Equal(SaveAction.Default | SaveAction.ForceNewVersion, saveAction);
        }

        [Theory]
        [MemberData(nameof(ValidStatus))]
        public void Save_WithValidStatus_ShouldCallSaveOnInnerRepository_WithCorrespondingSaveAction(VersionStatus status, SaveAction expectedSaveAction)
        {
            IContent saved = null;
            SaveAction saveAction = SaveAction.ForceCurrentVersion;

            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "PageData" },
                Language = new LanguageModel() { Name = "en" },
                RouteSegment = "/routesegment",
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = status
            };

            var subject = Subject(contentRepository.Object);

            subject.Save(content, Mock.Of<PageData>(), new SaveContentOptions());

            contentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            Assert.Equal(expectedSaveAction | SaveAction.ForceNewVersion, saveAction);
        }

        [Theory]
        [MemberData(nameof(InvalidStatus))]
        public void Save_WithInvalidStatus_ShouldThrow(VersionStatus status)
        {
            IContent saved = null;
            SaveAction saveAction = SaveAction.ForceCurrentVersion;

            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "PageData" },
                Language = new LanguageModel() { Name = "en" },
                RouteSegment = "/routesegment",
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = status
            };

            var subject = Subject(contentRepository.Object);

            Assert.Throws<ErrorException>(() => subject.Save(content, Mock.Of<PageData>(), new SaveContentOptions()));
            contentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Never);
        }

        [Fact]
        public void Save_WhenStatusTransitionIsInvalid_ShouldThrow()
        {
            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "PageData" },
                Language = new LanguageModel() { Name = "en" },
                RouteSegment = "/routesegment",
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = VersionStatus.CheckedOut
            };

            _mockStatusTransitionEvaluator.Setup(x => x.Evaluate(It.IsAny<IContent>(), It.IsAny<SaveAction>())).Returns(StatusTransition.Invalid);

            var errorException = Assert.Throws<ErrorException>(() => Subject().Save(content, Mock.Of<PageData>(), new SaveContentOptions()));
            Assert.StartsWith("A content item cannot be created with status", errorException.ErrorResponse.Error.Message);
        }

        [Fact]
        public void Save_WhenNextStatusIsDelayedPublish_AndStartPublishNotAvailable_ShouldThrow()
        {
            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "PageData" },
                Language = new LanguageModel() { Name = "en" },
                RouteSegment = "/routesegment",
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID },
                Status = VersionStatus.CheckedOut
            };

            _mockStatusTransitionEvaluator.Setup(x => x.Evaluate(It.IsAny<IContent>(), It.IsAny<SaveAction>())).Returns(new StatusTransition(VersionStatus.CheckedOut, VersionStatus.DelayedPublish, true));

            var errorException = Assert.Throws<ErrorException>(() => Subject().Save(content, Mock.Of<PageData>(), new SaveContentOptions()));
            Assert.Equal("StartPublish must be set when content item is set for scheduled publishing", errorException.ErrorResponse.Error.Message);
        }

        [Fact]
        public void Save_WhenNextStatusIsDelayedPublish_AndNotVersionable_ShouldThrow()
        {
            IContent saved = null;
            SaveAction saveAction = SaveAction.CheckOut;

            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                ContentType = new List<string>() { "SysContentFolder" },
                ParentLink = new ContentReferenceInputModel() { Id = ContentReference.GlobalBlockFolder.ID }
            };

            var subject = Subject(contentRepository.Object);
            _mockStatusTransitionEvaluator.Setup(x => x.Evaluate(It.IsAny<IContent>(), It.IsAny<SaveAction>())).Returns(new StatusTransition(VersionStatus.CheckedOut, VersionStatus.DelayedPublish, true));

            var errorException = Assert.Throws<ErrorException>(() => subject.Save(content, Mock.Of<ContentFolder>(), new SaveContentOptions()));
            Assert.Equal("StartPublish must be set when content item is set for scheduled publishing", errorException.ErrorResponse.Error.Message);
        }

        [Fact]
        public void Save_WhenPropertyValueConverterCannotBeResolved_ShouldThrow()
        {
            IContent saved = null;

            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => saved = c);

            var content = new ContentApiCreateModel
            {
                Name = "Some Content",
                ContentType = new List<string>() { "BaseType", "SomeType" },
                ParentLink = new ContentReferenceInputModel() { GuidValue = Guid.NewGuid() },
                Properties = new Dictionary<string, object>()
                {
                    { "Prop1", new LongStringPropertyModel() },
                    { "Prop2", new DateTimePropertyModel() }
                }
            };

            var subject = Subject(contentRepository.Object);

            Assert.Throws<ErrorException>(() => subject.Save(content, Mock.Of<PageData>(), new SaveContentOptions()));
        }
    }
}
