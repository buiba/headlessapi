using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class TrackingChildren: TrackingTestBase
    {
        public TrackingChildren(ServiceFixture fixture): base(fixture)
        {
        }

        #region IT tracking for personalied properties
        [Fact]
        public async Task RequestWithReference_WhenChildrenNotContainsPersonalizeProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestWithReference_WhenChildrenContainsNonPersonalizeContentArea_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentArea = CreateContentArea(false);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, init: p => p.MainContentArea = contentArea);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestWithReference_WhenChildrenContainsPersonalizeContentArea_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentArea = CreateContentArea(true);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, init: p => p.MainContentArea = contentArea);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestByGuid_WhenChildrenContainsPersonalizeContentArea_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentArea = CreateContentArea(true);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, init: p => p.MainContentArea = contentArea);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentGuid}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }
        #endregion

        #region IT tracking for personalied xhtml string
        [Fact]
        public async Task RequestWithReference_WhenChildrenContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, init: p => p.MainBody = xhtmlString);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestWithReference_WhenChildrenContainsPersonalizeXhtmlString_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, init: p => p.MainBody = xhtmlString);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestByGuid_WhenChildrenContainsPersonalizeXhtmlString_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, init: p => p.MainBody = xhtmlString);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentGuid}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }
        #endregion

        #region IT tracking for referenced content, for both request by content reference and guid

        [Fact()]
        public async Task Request_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var firstChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            var secondChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, firstChild, secondChild }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink.ID}/children"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.Equal(3, contentTrackingContext.ReferencedContent.Count);
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(parent.ContentLink));
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(firstChild.ContentLink));
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(secondChild.ContentLink));
            });
        }

        [Fact()]
        public async Task RequestByGuid_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
        
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink.ID}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                // ancestor are tracked (1) parent (2) parent's parent - root in en (3) root because root is also parent of child.
                Assert.Equal(3, contentTrackingContext.ReferencedContent.Count);

                // ContentLoaderService returns all parent of leave including "parent" and "root" page (without id and langue)
                // in trackingcontentfilter, we track parent.parentLink = root, but this api returns language and our code currently has not differentiated
                // root with langauge and root without language so that we have 3 items
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(parent.ContentLink));
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(ContentReference.RootPage));
                //Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(child.ContentLink));
            });
        }

        #endregion
    }
}
