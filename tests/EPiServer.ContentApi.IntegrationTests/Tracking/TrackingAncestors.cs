using EPiServer.ContentApi.Core.Serialization;
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
    public class TrackingAncestors : TrackingTestBase
    {
        public TrackingAncestors(ServiceFixture fixture): base(fixture)
        {
        }

        #region IT tracking for personalized property, for both request by content reference and guid
        [Fact]
        public async Task RequestParentsWithReference_WhenParentNotContainsPersonalizeProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithGuid_WhenParentNotContainsPersonalizeProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentGuid}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithReference_WhenParentContainsNonPersonalizeContentArea_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentArea = CreateContentArea(false);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithGuid_WhenParentContainsNonPersonalizeContentArea_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentArea = CreateContentArea(false);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentGuid}/ancestors");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithReference_WhenParentContainsPersonalizeContentArea_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentArea = CreateContentArea(true);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithGuid_WhenParentContainsPersonalizeContentArea_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentArea = CreateContentArea(true);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentGuid}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }
        #endregion

        #region IT tracking for personalized xhtml string, for both request by content reference and guid
        [Fact]
        public async Task RequestParentsWithReference_WhenParentContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithGuid_WhenParentContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentGuid}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithReference_WhenParentContainsPersonalizeXhtmlString_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestParentsWithGuid_WhenParentContainsPersonalizeXhtmlString_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentGuid}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }
        #endregion

        #region IT tracking for referenced content, for both request by content reference and guid

        [Fact]
        public async Task Request_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(contentTrackingContext.ReferencedContent.Any());
            });
        }

        [Fact]
        public async Task RequestByGuid_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors"); ;
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(contentTrackingContext.ReferencedContent.Any());
            });
        }

        #endregion
    }
}
