using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Search.Commerce.Internal;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace EPiServer.ContentApi.Search.Commerce.Tests
{
    public class FindCommerceEventsAssociationServiceTests
    {
        private static FindCommerceEventsAssociationService _subject;

        private static Mock<IContentRepository> _contentRepository;
        private static Mock<IContentEvents> _contentEvents;
        private static Mock<IContentSecurityRepository> _contentSecurityRepository;        
        private static Mock<ContentEventIndexerWrapper> _contentEventIndexerWrapper;

        private ContentReference _pageContentReference;
        private ContentReference _nodeContentReference;        

        public FindCommerceEventsAssociationServiceTests()
        {
            _pageContentReference = new ContentReference(1);
            var pageContent = new PageData(new AccessControlList(), new PropertyDataCollection());

            _nodeContentReference = new ContentReference(contentID: 1073741825, versionID: 0, providerName: "CatalogContent");
            var nodeContent = new NodeContent()
            {
                ContentLink = _nodeContentReference
            };

            _contentRepository = new Mock<IContentRepository>();
            _contentRepository.Setup(x => x.Get<IContent>(It.Is<ContentReference>(i => i.Equals(_nodeContentReference))))
                .Returns(nodeContent);
            _contentRepository.Setup(x => x.Get<IContent>(It.Is<ContentReference>(i => i.Equals(_pageContentReference))))
                .Returns(pageContent);

            var productContentLink = new ContentReference(contentID: 1, versionID: 0, providerName: "CatalogContent");       
            _contentRepository.Setup(x => x.GetDescendents(It.IsAny<ContentReference>()))
                .Returns(new List<ContentReference>() { productContentLink, productContentLink });            

            _contentEvents = new Mock<IContentEvents>();

            _contentSecurityRepository = new Mock<IContentSecurityRepository>();                      

            _contentEventIndexerWrapper = new Mock<ContentEventIndexerWrapper>(null);
            _contentEventIndexerWrapper.Setup(x => x.SavingContent(It.IsAny<ContentReference>()));

            _subject = new FindCommerceEventsAssociationService(_contentEvents.Object, _contentSecurityRepository.Object, _contentRepository.Object, _contentEventIndexerWrapper.Object);
        }

        [Fact]
        public void AttachContentSecurityEvent_ShouldIndexDescendents_WhenTypeIsNodeContentBase()
        {
            _subject.AttachContentSecurityEvent(this, new ContentSecurityEventArg(_nodeContentReference, new ContentAccessControlList(), SecuritySaveType.Replace));

            _contentEventIndexerWrapper.Verify(x => x.SavingContent(It.IsAny<ContentReference>()), Times.Exactly(2));
        }

        [Fact]
        public void AttachContentSecurityEvent_ShouldNotIndexDescendents_WhenTypeIsNotNodeContentBase()
        {
            _subject.AttachContentSecurityEvent(this, new ContentSecurityEventArg(_pageContentReference, new ContentAccessControlList(), SecuritySaveType.Replace));

            _contentEventIndexerWrapper.Verify(x => x.SavingContent(It.IsAny<ContentReference>()), Times.Never);
        }
    }
}
