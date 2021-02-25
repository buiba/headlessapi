using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class TrackingItems: TrackingTestBase
    {
        public TrackingItems(ServiceFixture fixture): base(fixture)
        {

        }
        #region IT tracking for personalized property
        [Fact]
        public async Task RequestItemsWithReferences_WhenContentNotContainsPersonalizeProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?references={parent.ContentLink},{child.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var parentRefContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(parent.ContentLink, new CultureInfo("en"))];
                var childRefContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(child.ContentLink, new CultureInfo("en"))];
                Assert.False(parentRefContentMetadata.PersonalizedProperties.Any());
                Assert.False(childRefContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithGuids_WhenContentNotContainsPersonalizeProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithReferences_WhenRequestContainsNonPersonalizeContentArea_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentArea = CreateContentArea(false);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?references={parent.ContentLink},{child.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithGuids_WhenRequestContainsNonPersonalizeContentArea_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentArea = CreateContentArea(false);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithReferences_WhenRequestContainsPersonalizeContentArea_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentArea = CreateContentArea(true);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?references={parent.ContentLink},{child.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithGuids_WhenRequestContainsPersonalizeContentArea_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentArea = CreateContentArea(true);
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainContentArea = contentArea);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }
        #endregion

        #region IT tracking for personalized xhtmlstring
        [Fact]
        public async Task RequestItemsWithReferences_WhenRequestContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?references={parent.ContentLink},{child.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithGuids_WhenRequestContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.False(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithReferences_WhenRequestContainsPersonalizeXhtmlString_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?references={parent.ContentLink},{child.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }

        [Fact]
        public async Task RequestItemsWithGuids_WhenRequestContainsPersonalizeXhtmlString_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);

            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.True(GetAllPersonalizedProperties(contentTrackingContext).Any());
            });
        }
        #endregion

        #region IT tracking for referenced content, for both request by content reference and guid

        [Fact]
        public async Task RequestItemsWithReferences_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var firstChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            var secondChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);

            await _fixture.WithContentItems(new[] { parent, firstChild, secondChild }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?references={firstChild.ContentLink},{secondChild.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                // firstchild, secondchild, and their parent (parent) are tracked
                Assert.Equal(3, contentTrackingContext.ReferencedContent.Count);

                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(parent.ContentLink));
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(firstChild.ContentLink));
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(secondChild.ContentLink));
            });
        }

        [Fact]
        public async Task RequestByGuid_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var firstChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            var secondChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);

            await _fixture.WithContentItems(new[] { parent, firstChild, secondChild }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"?guids={firstChild.ContentGuid},{secondChild.ContentGuid}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                // firstchild, secondchild, and their parent (parent) are tracked
                Assert.Equal(3, contentTrackingContext.ReferencedContent.Count);

                // assert with item.contentlink instead of contentguid because they are the same in this context.
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(parent.ContentLink));
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(firstChild.ContentLink));
                Assert.Contains(contentTrackingContext.ReferencedContent, item => item.Key.ContentLink.CompareToIgnoreWorkID(secondChild.ContentLink));
            });
        }

        #endregion
    }
}
