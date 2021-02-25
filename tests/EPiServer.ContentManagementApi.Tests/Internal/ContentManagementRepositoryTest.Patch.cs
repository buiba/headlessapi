using System;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.Core;
using Moq;
using Xunit;
using EPiServer.ContentApi.Error.Internal;
using System.Net;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using System.Globalization;
using System.Collections.Generic;
using EPiServer.DataAccess;
using EPiServer.Web;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void Patch_ByContentReference_WhenContentLinkIsNull_ShouldThrow()
        {
            var subject = Subject();
            Assert.Throws<ArgumentNullException>(() => subject.Patch((ContentReference)null, new Models.Internal.ContentApiPatchModel(), new SaveContentOptions()));
        }

        [Fact]
        public void Patch_ByContentReference_WhenInputContentIsNull_ShouldThrowException()
        {
            var subject = Subject();
            Assert.Throws<ArgumentNullException>(() => subject.Patch(new ContentReference(1), null, new SaveContentOptions()));
        }

        [Fact]
        public void Patch_ByContentReference_WhenContentIsNotExposedToApi_ShouldThrowException()
        {
            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(new PageData());
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(false);

            var subject = Subject(_mockContentRepository.Object);
            var contentLink = new ContentReference(1);
            var content = new ContentApiPatchModel
            {
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "en", DisplayName = "English" },
                Name = "New name"
            };
            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));
            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        }

        [Fact]
        public void Patch_ByContentReference_WhenCommmonDraftContentIsNull_ShouldThrowException()
        {
            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(new PageData());
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns((ContentVersion)null);

            var subject = Subject(_mockContentRepository.Object);
            var contentLink = new ContentReference(1);
            var content = new ContentApiPatchModel
            {
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "en", DisplayName = "English" },
                Name = "New name"
            };
            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal($"The content with id '{contentLink}' does not exist.", exception.Message);
        }

        [Fact]
        public void Patch_ByContentReference_WhenProvidedContentLanguageDoesNotExist_ShouldThrowException()
        {
            var contentLink = new ContentReference(1);
            var properties = new PropertyDataCollection
            {
                { "PageLink", new PropertyPageReference(contentLink) },
                { "PageParentLink", new PropertyPageReference(new PageReference(16, 4)) }
            };

            IContent testPage = new PageData(new AccessControlList(), properties)
            {
                ParentLink = new PageReference(11),
                ExistingLanguages = new List<CultureInfo>()
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("fr-CA"),
                }
            };

            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            var subject = Subject(_mockContentRepository.Object);
            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testPage);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testPage);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);

            var content = new ContentApiPatchModel
            {
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "sv", DisplayName = "Swedish" },
                Name = "New name"
            };

            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal($"The provided language does not exist for content with id '{contentLink}'.", exception.Message);
        }

        [Fact]
        public void Patch_ByContentReference_WhenContentIsNotILocalizable_AndLanguageModelIsNotNull_ShouldThrowException()
        {
            var contentLink = new ContentReference(1);
            IContent testContent = new ContentFolder();
            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);

            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            var subject = Subject(_mockContentRepository.Object);
            var content = new ContentApiPatchModel
            {
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "en", DisplayName = "English" },
                Name = "New name"
            };
            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal($"Language cannot be set when content type is not localizable", exception.Message);
        }


        [Fact]
        public void Patch_ByContentReference_WhenContentIsNotIRoutable_AndHaveRouteSegment_ShouldThrowException()
        {
            var contentLink = new ContentReference(1);
            IContent testPage = new BasicContent();
            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testPage);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testPage);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            var subject = Subject(_mockContentRepository.Object);

            var content = new ContentApiPatchModel
            {
                RouteSegment = "test/some-route",
                Name = "Test Name",
                UpdatedMetadata = new HashSet<string> { nameof(ContentApiPatchModel.RouteSegment) }
            };

            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));
            Assert.Equal(ProblemCode.ContentNotRoutable, exception.ErrorResponse.Error.Code);
        }

        [Fact]
        public void Patch_ByContentReference_WhenStatusTransitionIsInvalid_ShouldThrowException()
        {
            var contentLink = new ContentReference(1);
            IContent testContent = new ContentFolder();
            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            _mockStatusTransitionEvaluator.Setup(x => x.Evaluate(It.IsAny<IContent>(), It.IsAny<SaveAction>())).Returns(StatusTransition.Invalid);

            var content = new ContentApiPatchModel
            {
                Name = "Test Name"
            };

            var subject = Subject(_mockContentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.StartsWith("A content item cannot be created with status", exception.ErrorResponse.Error.Message);
        }

        [Fact]
        public void Patch_ByContentReference_WhenNextStatusIsDelayedPublish_AndStartPublishNotAvailable_ShouldThrowException()
        {
            var contentLink = new ContentReference(1);
            var properties = new PropertyDataCollection
            {
                { "PageLink", new PropertyPageReference(contentLink) },
                { "PageParentLink", new PropertyPageReference(new PageReference(16, 4)) },
                { "PageMasterLanguageBranch", new PropertyString("en")},
                { "PageCategory", new PropertyCategory()},
                { "PageName", new PropertyString("MyPage")}
            };

            IContent testPage = new PageData(new AccessControlList(), properties)
            {
                ParentLink = new PageReference(11),
                ExistingLanguages = new List<CultureInfo>()
                {
                    new CultureInfo("en"),
                    new CultureInfo("sv"),
                }
            };
            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testPage);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testPage);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            _mockStatusTransitionEvaluator.Setup(x => x.Evaluate(It.IsAny<IContent>(), It.IsAny<SaveAction>())).Returns(new StatusTransition(VersionStatus.CheckedOut, VersionStatus.DelayedPublish, true));

            var content = new ContentApiPatchModel
            {
                Name = "Test Name",
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "en", DisplayName = "English" },
                Status = VersionStatus.CheckedOut
            };

            var subject = Subject(_mockContentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("StartPublish must be set when content item is set for scheduled publishing", exception.ErrorResponse.Error.Message);
        }

        [Fact]
        public void Patch_ByContentReference_WhenNextStatusIsDelayedPublish_AndContentIsNotVersionable_ShouldThrowException()
        {
            var contentLink = new ContentReference(1);
            IContent testContent = new ContentFolder();
            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            _mockStatusTransitionEvaluator.Setup(x => x.Evaluate(It.IsAny<IContent>(), It.IsAny<SaveAction>())).Returns(new StatusTransition(VersionStatus.CheckedOut, VersionStatus.DelayedPublish, true));

            var content = new ContentApiPatchModel
            {
                Name = "Test Name"
            };

            var subject = Subject(_mockContentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("StartPublish must be set when content item is set for scheduled publishing", exception.ErrorResponse.Error.Message);
        }

        [Theory]
        [MemberData(nameof(VersionableContentData))]
        public void Patch_ByContentReference_WhenContentIsNotIVersionable_AndHaveVersionableData_ShouldThrowException(ContentApiPatchModel content)
        {
            var contentLink = new ContentReference(1);
            IContent testContent = new ContentFolder();
            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            var subject = Subject(_mockContentRepository.Object);
            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentLink, content, new SaveContentOptions()));
            Assert.Equal(ProblemCode.ContentNotVersionable, exception.ErrorResponse.Error.Code);
        }

        public static TheoryData VersionableContentData => new TheoryData<ContentApiPatchModel>
        {
           new ContentApiPatchModel
           {
                RouteSegment = "test/some-route",
                Name = "Test Name",
                Status = VersionStatus.Published
           },
           new ContentApiPatchModel
           {
                RouteSegment = "test/some-route",
                Name = "Test Name",
                StartPublish = DateTime.Now
           },
          new ContentApiPatchModel
           {
                RouteSegment = "test/some-route",
                Name = "Test Name",
                StopPublish = DateTime.Now
           }
        };

        [Theory]
        [MemberData(nameof(ValidVersionStatus))]
        public void Patch_ByContentReference_WithValidStatus_ShouldCallSaveOnInnerRepository_WithCorrespondingSaveAction(VersionStatus status, SaveAction expectedSaveAction)
        {
            var contentLink = new ContentReference(1);
            var properties = new PropertyDataCollection
            {
                { "PageName", new PropertyString("Page Name")},
                { "PageLink", new PropertyPageReference(contentLink) },
                { "PageParentLink", new PropertyPageReference(new PageReference(16, 4)) },
                { "PageURLSegment", new PropertyString("segment")},
                { "PageMasterLanguageBranch", new PropertyString("en")},
                { "PageCategory", new PropertyCategory()},
                { "PageStartPublish", new PropertyDate(DateTime.UtcNow)},
                { "PageStopPublish", new PropertyDate(DateTime.UtcNow.AddYears(1))}
            };

            IContent testContent = new PageData(new AccessControlList(), properties)
            {
                ParentLink = new PageReference(11),
                ExistingLanguages = new List<CultureInfo>()
                {
                    new CultureInfo("en"),
                    new CultureInfo("sv"),
                }
            };

            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);

            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            _mockContentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var content = new ContentApiPatchModel
            {
                Name = "Some Content",
                RouteSegment = "/routesegment",
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "en", DisplayName = "English" },
                StartPublish = DateTime.UtcNow,
                StopPublish = DateTime.UtcNow.AddYears(1),
                Status = status,
                UpdatedMetadata = new HashSet<string>
                {
                    nameof(ContentApiPatchModel.RouteSegment),
                    nameof(ContentApiPatchModel.StartPublish),
                    nameof(ContentApiPatchModel.StopPublish),
                    nameof(ContentApiPatchModel.Status)
                }
            };

            var subject = Subject(_mockContentRepository.Object);

            subject.Patch(contentLink, content, new SaveContentOptions());

            _mockContentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            Assert.Equal(expectedSaveAction, saveAction);
        }

        [Fact]
        public void Patch_ByContentGuid_WhenPermanentLinkMapIsNull_ShouldThrowException()
        {
            _mockPermanentLinkMapper.Setup(x => x.Find(It.IsAny<System.Guid>())).Returns((PermanentLinkMap)null);
            var subject = Subject(_mockContentRepository.Object);
            var contentGuid = System.Guid.NewGuid();

            var exception = Assert.Throws<ErrorException>(() => subject.Patch(contentGuid, new ContentApiPatchModel(), new SaveContentOptions()));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal($"No content with the provided content guid {contentGuid} exists.", exception.Message);
        }

        [Theory]
        [MemberData(nameof(ValidVersionStatus))]
        public void Patch_ByContentGuid_WithValidStatus_ShouldCallSaveOnInnerRepository_WithCorrespondingSaveAction(VersionStatus status, SaveAction expectedSaveAction)
        {
            var contentLink = new ContentReference(1);
            var contentGuid = System.Guid.NewGuid();

            var properties = new PropertyDataCollection
            {
                { "PageName", new PropertyString("PageName")},
                { "PageLink", new PropertyPageReference(contentLink) },
                { "PageParentLink", new PropertyPageReference(new PageReference(16, 4)) },
                { "PageURLSegment", new PropertyString("segment")},
                { "PageMasterLanguageBranch", new PropertyString("en")},
                { "PageCategory", new PropertyCategory()},
                { "PageStartPublish", new PropertyDate(DateTime.UtcNow)},
                { "PageStopPublish", new PropertyDate(DateTime.UtcNow.AddYears(1))}
            };

            IContent testContent = new PageData(new AccessControlList(), properties)
            {
                ParentLink = new PageReference(11),
                ExistingLanguages = new List<CultureInfo>()
                {
                    new CultureInfo("en"),
                    new CultureInfo("sv"),
                }
            };

            var contentVersion = new ContentVersion(contentLink, "", VersionStatus.Published, DateTime.Now, "", "", 0, "en", true, false);
            _mockPermanentLinkMapper.Setup(x => x.Find(It.IsAny<System.Guid>())).Returns(new PermanentLinkMap(contentGuid, contentLink));
            _mockContentLoader.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentRepository.Setup(m => m.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockContentVersionRepository.Setup(x => x.LoadCommonDraft(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(contentVersion);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);

            IContent saved = null;
            SaveAction saveAction = SaveAction.Default;

            _mockContentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Callback<IContent, SaveAction, AccessLevel>((c, s, a) => { saved = c; saveAction = s; });

            var content = new ContentApiPatchModel
            {
                Name = "Some Content",
                RouteSegment = "/routesegment",
                Language = new ContentApi.Core.Serialization.Models.LanguageModel { Name = "en", DisplayName = "English" },
                StartPublish = DateTime.UtcNow,
                StopPublish = DateTime.UtcNow.AddYears(1),
                Status = status,
                UpdatedMetadata = new HashSet<string>
                {
                    nameof(ContentApiPatchModel.RouteSegment),
                    nameof(ContentApiPatchModel.StartPublish),
                    nameof(ContentApiPatchModel.StopPublish),
                    nameof(ContentApiPatchModel.Status)
                }
            };

            var subject = Subject(_mockContentRepository.Object);

            subject.Patch(contentGuid, content, new SaveContentOptions());

            _mockContentRepository.Verify(x => x.Save(saved, saveAction, It.IsAny<AccessLevel>()), Times.Once);
            Assert.Equal(expectedSaveAction, saveAction);
        }

        public static TheoryData ValidVersionStatus => new TheoryData<VersionStatus, SaveAction>
        {
            { VersionStatus.CheckedIn, SaveAction.CheckIn | SaveAction.ForceCurrentVersion },
            { VersionStatus.CheckedOut, SaveAction.CheckOut | SaveAction.ForceCurrentVersion },
            { VersionStatus.Published, SaveAction.Publish | SaveAction.ForceCurrentVersion},
            { VersionStatus.DelayedPublish, SaveAction.Schedule | SaveAction.ForceCurrentVersion },
            { VersionStatus.Rejected, SaveAction.Reject | SaveAction.ForceCurrentVersion},
            { VersionStatus.AwaitingApproval, SaveAction.RequestApproval | SaveAction.ForceCurrentVersion }
        };
    }

}
