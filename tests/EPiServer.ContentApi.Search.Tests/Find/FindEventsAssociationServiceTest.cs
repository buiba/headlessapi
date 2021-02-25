using System.Collections.Generic;
using System.Globalization;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Find.Cms;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Search.Tests.Find
{
    public class FindEventsAssociationServiceTests
    {
        private static Mock<IContentEvents> _contentEvents;
        private static Mock<IContentSecurityRepository> _contentSecurityRepository;
        private static Mock<IContentRepository> _contentRepository;
        private static ContentEventIndexer _contentEventIndexer;
        private static Mock<IContentProviderManager> _contentProviderManager;
        private static Mock<ContentAssetHelper> _contentAssetHelper;
        private static Mock<ContentEventIndexerWrapper> _contentEventIndexerWrapper;
        private static FindEventsAssociationService _findEventsAssociationService;

        [Fact]
        public void AttachContentEvent_ShouldIndexReferencedItem_WhenTypeIsPageLinkReference()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.PageLinkReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.CmsContentEvents(this, new ContentEventArgs(contentReference));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Once);
        }

        [Fact]
        public void AttachContentEvent_ShouldIndexSingleReferencedItem_WhenDuplicateReferencesExist()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.PageLinkReference
                },
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.PageLinkReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.CmsContentEvents(this, new ContentEventArgs(contentReference));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Once);
        }

        [Fact]
        public void AttachContentEvent_ShouldNotIndexReferencedItem_WhenTypeIsExternalReference()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.ExternalReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.CmsContentEvents(this, new ContentEventArgs(contentReference));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Never);
        }

        [Fact]
        public void AttachContentEvent_ShouldNotIndexReferencedItem_WhenReferenceIsSelf()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "13",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.ExternalReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.CmsContentEvents(this, new ContentEventArgs(contentReference));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Never);
        }

        [Fact]
        public void AttachContentSecurityEvent_ShouldIndexReferencedItem_WhenTypeIsPageLinkReference()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.PageLinkReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.AttachContentSecurityEvent(this, new ContentSecurityEventArg(contentReference, new ContentAccessControlList(), SecuritySaveType.Replace));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Once);
        }

        [Fact]
        public void AttachContentSecurityEvent_ShouldIndexSingleReferencedItem_WhenDuplicateReferencesExist()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.PageLinkReference
                },
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.PageLinkReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.AttachContentSecurityEvent(this, new ContentSecurityEventArg(contentReference, new ContentAccessControlList(), SecuritySaveType.Replace));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Once);
        }

        [Fact]
        public void AttachContentSecurityEvent_ShouldNotIndexReferencedItem_WhenTypeIsExternalReference()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "14",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.ExternalReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.AttachContentSecurityEvent(this, new ContentSecurityEventArg(contentReference, new ContentAccessControlList(), SecuritySaveType.Replace));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Never);
        }

        [Fact]
        public void AttachContentSecurityEvent_ShouldNotIndexReferencedItem_WhenReferenceIsSelf()
        {
            SetupInitializationModule(new List<ReferenceInformation>()
            {
                new ReferenceInformation()
                {
                    OwnerID = new ContentReference(14),
                    OwnerLanguage = new CultureInfo("en-US"),
                    OwnerName = "13",
                    ReferencedID = new ContentReference(13),
                    ReferencedLanguage = new CultureInfo("en-US"),
                    ReferencedName = "13",
                    ReferenceType = (int) ReferenceType.ExternalReference
                }
            });

            var contentReference = new ContentReference(13);
            _findEventsAssociationService.AttachContentSecurityEvent(this, new ContentSecurityEventArg(contentReference, new ContentAccessControlList(), SecuritySaveType.Replace));

            _contentEventIndexerWrapper.Verify(
                c => c.SavingContent(It.Is<ContentReference>(reference => reference == new ContentReference(14))), Times.Never);
        }

        private static void SetupInitializationModule(List<ReferenceInformation> referencedContent)
        {
            _contentEvents = new Mock<IContentEvents>();

            _contentSecurityRepository = new Mock<IContentSecurityRepository>();
            _contentRepository = new Mock<IContentRepository>();

            _contentRepository.Setup(x => x.GetReferencesToContent(It.IsAny<ContentReference>(), It.IsAny<bool>()))
                .Returns(referencedContent);

#pragma warning disable CS0618 // Type or member is obsolete
            _contentEventIndexer = new ContentEventIndexer(new SimplePage());
#pragma warning restore CS0618 // Type or member is obsolete
            _contentProviderManager = new Mock<IContentProviderManager>();
            _contentAssetHelper = new Mock<ContentAssetHelper>();

            _contentEventIndexerWrapper = new Mock<ContentEventIndexerWrapper>(_contentEventIndexer);
            _contentEventIndexerWrapper.Setup(x => x.SavingContent(It.IsAny<ContentReference>()));
            _findEventsAssociationService = new FindEventsAssociationService(_contentEvents.Object, _contentSecurityRepository.Object, _contentRepository.Object, _contentEventIndexerWrapper.Object);
        }
    }
}
