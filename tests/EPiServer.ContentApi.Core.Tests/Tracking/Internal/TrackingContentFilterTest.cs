using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.Core.Tracking.Internal;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Globalization;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Tracking.Internal
{
    public class TrackingContentFilterTest
    {
        private Mock<ContentApiAuthorizationService> _authorizationService = new Mock<ContentApiAuthorizationService>();
        private IContentApiTrackingContextAccessor _contextAccessor;
        private Mock<IPermanentLinkMapper> _permanentLinkMapper = new Mock<IPermanentLinkMapper>();

        public TrackingContentFilterTest()
        {
            _contextAccessor = CreateContextAccessor();
        }

        [Fact]
        public void Filter_WhenContentIsNotPublicallyAccessible_ShouldTrackSecuredContent()
        {
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink);
           
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(false);
          
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            Assert.Contains(contentLink, _contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenContentIsPublicallyAccessible_ShouldNotTrackSecuredContent()
        {
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink);
            
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(true);
                       
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            Assert.Empty(_contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenASecuredContentReferenceIsExpanded_ShouldTrackSecuredForProperty()
        {
            var securedPropertyName = "SecuredReference";
            var securedLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyContentReference { Name = securedPropertyName, Value = securedLink });
                        
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(securedLink)).Returns(false);
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(true);
                      
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext(expandedProperty: securedPropertyName));

            Assert.Contains(securedLink, _contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenASecuredContentReferenceIsNotExpanded_ShouldNotTrackSecuredForProperty()
        {
            var securedPropertyName = "SecuredReference";
            var securedLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyContentReference { Name = securedPropertyName, Value = securedLink });
                        
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(true);
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(securedLink)).Returns(false);
                      
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            Assert.Empty(_contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenASecuredContentReferenceListIsExpanded_ShouldTrackSecuredForProperty()
        {
            var securedPropertyName = "SecuredReference";
            var securedLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyContentReferenceList { Name = securedPropertyName, Value = new[] { securedLink } });

            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(true);
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(securedLink)).Returns(false);
                      
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext(expandedProperty: securedPropertyName));

            Assert.Contains(securedLink, _contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenASecuredContentReferenceListIsNotExpanded_ShouldNotTrackSecuredForProperty()
        {
            var securedPropertyName = "SecuredReference";
            var securedLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyContentReferenceList { Name = securedPropertyName, Value = new[] { securedLink } });

            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(true);
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(securedLink)).Returns(false);
                      
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            Assert.Empty(_contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenASecuredLinkCollectionIsExpanded_ShouldTrackSecuredForProperty()
        {
            var securedPropertyName = "SecuredReference";
            var securedLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var linkItem = new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(Guid.NewGuid(), ".aspx") };
            var content = CreateContent(contentLink, new PropertyLinkCollection { Name = securedPropertyName, Value = new LinkItemCollection(new[] { linkItem }) });
                        
            _permanentLinkMapper.Setup(u => u.Find(It.IsAny<Guid>())).Returns(CreatePermanentLinkMap(securedLink));

            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(true);
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(securedLink)).Returns(false);
                        
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, _permanentLinkMapper.Object);
            subject.Filter(content, CreateConverterContext(expandedProperty: securedPropertyName));

            Assert.Contains(securedLink, _contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenASecuredLinkCollectionIsNotExpanded_ShouldNotTrackSecuredForProperty()
        {
            var securedPropertyName = "SecuredReference";
            var securedLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var linkItem = new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(Guid.NewGuid(), ".aspx") };
            var content = CreateContent(contentLink, new PropertyLinkCollection { Name = securedPropertyName, Value = new LinkItemCollection(new[] { linkItem }) });
                        
            _permanentLinkMapper.Setup(u => u.Find(It.IsAny<Guid>())).Returns(CreatePermanentLinkMap(securedLink));

            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(content)).Returns(true);
            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(securedLink)).Returns(false);
                       
            var subject = new TrackingContentFilter(_contextAccessor, _authorizationService.Object, _permanentLinkMapper.Object);
            subject.Filter(content, CreateConverterContext());

            Assert.Empty(_contextAccessor.Current.SecuredContent);
        }

        [Fact]
        public void Filter_WhenContentNotContainsAnyContentReference_ShouldNotTrackReferredContent()
        {
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, null);           

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            // ReferencedContent should only contains the requested content's contentLink
            Assert.Single(_contextAccessor.Current.ReferencedContent);
            Assert.Contains(new LanguageContentReference(contentLink, null), _contextAccessor.Current.ReferencedContent.Keys);
        }


        [Fact]
        public void Filter_WhenContentContainsAContentReference_ShouldTrackReferredContent()
        {
            var propertyName = "ContentReference";
            var contentReferencePropertyValue = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyContentReference { Name = propertyName, Value = contentReferencePropertyValue });

            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(It.IsAny<IContent>())).Returns(true);            
                      
            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            Assert.Contains(new LanguageContentReference(contentReferencePropertyValue, null), _contextAccessor.Current.ReferencedContent.Keys);
        }
        
        [Fact]
        public void Filter_WhenContentContainsAContentReferenceList_ShouldTrackReferredContent()
        {
            var propertyName = "ContentReferenceList";
            var contentRefrenceValue = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyContentReferenceList { Name = propertyName, Value = new[] { contentRefrenceValue } });

            _authorizationService.Setup(a => a.IsAnonymousAllowedToAccessContent(It.IsAny<IContent>())).Returns(true);

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            Assert.Contains(new LanguageContentReference(contentRefrenceValue, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenContentContainsALinkItemCollection_ShouldTrackReferredContent()
        {
            var propertyName = "LinkItemCollection";
            var linkItem = new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(Guid.NewGuid(), ".aspx") };
            var linkItemContentLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyLinkCollection { Name = propertyName, Value = new LinkItemCollection(new[] { linkItem }) });
                       
            _permanentLinkMapper.Setup(u => u.Find(It.IsAny<Guid>())).Returns(CreatePermanentLinkMap(linkItemContentLink));

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), _permanentLinkMapper.Object);
            subject.Filter(content, CreateConverterContext());

            Assert.Contains(new LanguageContentReference(linkItemContentLink, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenContentContainsALinkItemCollectionWithSpecificLanguage_ShouldTrackReferredContentInTheSameLanguage()
        {
            var propertyName = "LinkItemCollection";
            var linkItem = new LinkItem { Href = UriUtil.AddLanguageSelection(PermanentLinkUtility.GetPermanentLinkVirtualPath(Guid.NewGuid(), ".aspx"), "en")};
            var linkItemContentLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, new PropertyLinkCollection { Name = propertyName, Value = new LinkItemCollection(new[] { linkItem }) });

            _permanentLinkMapper.Setup(u => u.Find(It.IsAny<Guid>())).Returns(CreatePermanentLinkMap(linkItemContentLink));

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), _permanentLinkMapper.Object);
            subject.Filter(content, CreateConverterContext());

            Assert.Contains(new LanguageContentReference(linkItemContentLink, new CultureInfo("en")), _contextAccessor.Current.ReferencedContent.Keys);
            Assert.DoesNotContain(new LanguageContentReference(linkItemContentLink, new CultureInfo("sv")), _contextAccessor.Current.ReferencedContent.Keys);
            Assert.DoesNotContain(new LanguageContentReference(linkItemContentLink, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenContentContainsAnExternalUrl_ShouldNotTrackReferredContent()
        {
            var urlResolver = new Mock<IUrlResolver>();
            var propertyUrl = new PropertyUrl
            {
                Name = "Url",
                LinkResolver = new Injected<IUrlResolver>(urlResolver.Object),
                Value = "http://external.com"
            };

            var linkItemContentLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, propertyUrl);            

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), Mock.Of<IPermanentLinkMapper>());
            subject.Filter(content, CreateConverterContext());

            // ReferencedContent should only contains the requested content's contentLink
            Assert.Single(_contextAccessor.Current.ReferencedContent);
            Assert.Contains(new LanguageContentReference(contentLink, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenContentContainsAUrl_ShouldTrackReferredContent()
        {
            var urlResolver = new Mock<IUrlResolver>();
            var propertyUrl = new PropertyUrl
            {
                Name = "Url",
                LinkResolver = new Injected<IUrlResolver>(urlResolver.Object),
                Value = new Url(PermanentLinkUtility.GetPermanentLinkVirtualPath(Guid.NewGuid(), ".aspx"))
            };            

            var linkItemContentLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, propertyUrl);

            _permanentLinkMapper.Setup(u => u.Find(It.IsAny<Guid>())).Returns(CreatePermanentLinkMap(linkItemContentLink));            

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), _permanentLinkMapper.Object);
            subject.Filter(content, CreateConverterContext());

            Assert.Contains(new LanguageContentReference(linkItemContentLink, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenContentContainsUrlWithSpecificLanguage_ShouldTrackReferredContentInTheSameLanguage()
        {
            var urlResolver = new Mock<IUrlResolver>();
            var propertyUrl = new PropertyUrl
            {
                Name = "Url",
                LinkResolver = new Injected<IUrlResolver>(urlResolver.Object),
                Value = new Url(UriUtil.AddLanguageSelection(PermanentLinkUtility.GetPermanentLinkVirtualPath(Guid.NewGuid(), ".aspx"), "en"))
            };

            var linkItemContentLink = new ContentReference(24);
            var contentLink = new ContentReference(12);
            var content = CreateContent(contentLink, propertyUrl);

            _permanentLinkMapper.Setup(u => u.Find(It.IsAny<Guid>())).Returns(CreatePermanentLinkMap(linkItemContentLink));

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), _permanentLinkMapper.Object);
            subject.Filter(content, CreateConverterContext());

            Assert.Contains(new LanguageContentReference(linkItemContentLink, new CultureInfo("en")), _contextAccessor.Current.ReferencedContent.Keys);
            Assert.DoesNotContain(new LanguageContentReference(linkItemContentLink, new CultureInfo("sv")), _contextAccessor.Current.ReferencedContent.Keys);
            Assert.DoesNotContain(new LanguageContentReference(linkItemContentLink, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenParentLinkIsValid_ShouldTrackReferredContentOfParent()
        {
            var content = new Mock<IContent>();
            var parentLink = new ContentReference(9999);
            content.Setup(c => c.ContentLink).Returns(new ContentReference(12));
            content.Setup(c => c.ParentLink).Returns(parentLink);
            content.Setup(c => c.Property).Returns(new PropertyDataCollection());

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), _permanentLinkMapper.Object);
            subject.Filter(content.Object, CreateConverterContext());

            Assert.Contains(new LanguageContentReference(parentLink, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenParentLinkIsNull_ShouldNotTrackReferredContentOfParent()
        {
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(12));
            content.Setup(c => c.ParentLink).Returns((ContentReference)null);
            content.Setup(c => c.Property).Returns(new PropertyDataCollection());

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), _permanentLinkMapper.Object);
            subject.Filter(content.Object, CreateConverterContext());

            Assert.DoesNotContain(new LanguageContentReference(null, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        [Fact]
        public void Filter_WhenParentLinkIsEmpty_ShouldNotTrackReferredContentOfParent()
        {
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(12));
            content.Setup(c => c.ParentLink).Returns(ContentReference.EmptyReference);
            content.Setup(c => c.Property).Returns(new PropertyDataCollection());

            var subject = new TrackingContentFilter(_contextAccessor, Mock.Of<ContentApiAuthorizationService>(), _permanentLinkMapper.Object);
            subject.Filter(content.Object, CreateConverterContext());

            Assert.DoesNotContain(new LanguageContentReference(ContentReference.EmptyReference, null), _contextAccessor.Current.ReferencedContent.Keys);
        }

        private IContentApiTrackingContextAccessor CreateContextAccessor()
        {
            var context = new ContentApiTrackingContext();
            var contextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            contextAccessor.Setup(c => c.Current).Returns(context);
            return contextAccessor.Object;
        }

        private ConverterContext CreateConverterContext(string expandedProperty = null)
            => new ConverterContext(new ContentApiOptions(), string.Empty, expandedProperty ?? string.Empty, false, CultureInfo.InvariantCulture);

        private PermanentLinkMap CreatePermanentLinkMap(ContentReference contentLink)
        {
            var content = CreateContent(contentLink);
            return new PermanentLinkMap(content.ContentGuid, contentLink);
        }

        private IContent CreateContent(ContentReference contentLink, PropertyData property = null)
        {
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(contentLink);
            content.Setup(c => c.ContentGuid).Returns(Guid.NewGuid());
            var propertyCollection = new PropertyDataCollection();
            if (property is object)
            {
                // Define PropertyDefinitionID so the property is not metadata property
                property.PropertyDefinitionID = 1;
                propertyCollection.Add(property);
            }
            content.Setup(c => c.Property).Returns(propertyCollection);
            return content.Object;
        }
    }
}
