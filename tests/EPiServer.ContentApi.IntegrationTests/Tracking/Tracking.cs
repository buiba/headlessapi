using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class Tracking : TrackingTestBase
    {
        public Tracking(ServiceFixture fixture) : base(fixture)
        {

        }

        [Fact]
        public async Task Request_WhenARequestIsExecuted_ShouldBeTheSameContextThroughOutTheRequest()
        {
            await _fixture.Client.GetAsync(V2Uri + $"/{IntegrationTestCollection.StartPageGuId}");
            var captureContentFilter = ServiceLocator.Current.GetAllInstances<IContentFilter>().OfType<CaptureContextFilter>().Single();
            var captureContentApiModelFilter = ServiceLocator.Current.GetAllInstances<IContentApiModelFilter>().OfType<CaptureContextFilter>().Single();
            Assert.Same(captureContentFilter.LatestContentTrackingContext, captureContentApiModelFilter.LatestContentApiModelTrackingContext);
        }

        [Fact]
        public async Task Request_WhenDifferentRequestsAreExecuted_ShouldBeDifferentContexts()
        {
            await _fixture.Client.GetAsync(V2Uri + $"/{IntegrationTestCollection.StartPageGuId}");
            var firstRequestContext = ServiceLocator.Current.GetAllInstances<IContentFilter>().OfType<CaptureContextFilter>().Single().LatestContentTrackingContext;
            await _fixture.Client.GetAsync(V2Uri + $"/{IntegrationTestCollection.StartPageGuId}");
            var secondRequestContext = ServiceLocator.Current.GetAllInstances<IContentFilter>().OfType<CaptureContextFilter>().Single().LatestContentTrackingContext;
            Assert.NotSame(firstRequestContext, secondRequestContext);
        }

        [Fact]
        public async Task Request_WhenContentNotContainsPersonalizedProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.False(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task Request_WhenARequestIsExecuted_SavedTimeShouldBeSet()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.NotNull(refContentMetadata.SavedTime);
                Assert.Equal(refContentMetadata.SavedTime.Value.Date, DateTime.Now.Date);
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsNonPersonalizeContentArea_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentArea = CreateContentArea(false);
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.ContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.False(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsPersonalizedContentArea_ShouldBeAbleToTrackPersonalizedProperties()
        {
            // initialize contentArea that contains content item with personalized setting
            var contentArea = CreateContentArea(true);
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.ContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.True(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsMultiplePersonalizedProperties_ShouldBeAbleToTrackAllPersonalizedProperties()
        {
            // initialize contentAreas that contains content item with personalized setting
            var contentArea = CreateContentArea(true);
            var secondaryContentArea = CreateContentArea(true);
            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => { p.ContentArea = contentArea; p.SecondaryContentArea = secondaryContentArea; });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                var propertyList = refContentMetadata.PersonalizedProperties;
                Assert.Contains(nameof(PropertyPage.ContentArea), propertyList);
                Assert.Contains(nameof(PropertyPage.SecondaryContentArea), propertyList);
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.False(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsPersonalizedXhtmlString_ShouldBeAbleToTrackPersonalizedProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.True(refContentMetadata.PersonalizedProperties.Any());
            });
        }        

        #region IT tracking for expanded property, for both request by content reference and guid
        [Fact]
        public async Task RequestWithExpandedProperty_WhenRequestContainsNonPersonalizeProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.False(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task RequestByGuidWithExpandedProperty_WhenRequestContainsNonPersonalizeProperties_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink.ToReferenceWithoutVersion(), new CultureInfo("en"))];
                Assert.False(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task RequestWithExpandedProperty_WhenRequestContainsPersonalizeProperties_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid, AllowedRoles = new string[] { _visitorGroup.Id.ToString() } });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.True(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task RequestByGuidWithExpandedProperty_WhenRequestContainsPersonalizeProperties_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid, AllowedRoles = new string[] { _visitorGroup.Id.ToString() } });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink.ToReferenceWithoutVersion(), new CultureInfo("en"))];
                Assert.True(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task RequestWithExpandedProperty_WhenRequestContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            contentAreaItem.MainBody = xhtmlString;
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.False(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task RequestByGuidWithExpandedProperty_WhenRequestContainsNonPersonalizeXhtmlString_PersonalizedPropertiesOfTrackingContextShouldBeEmpty()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            contentAreaItem.MainBody = xhtmlString;
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink.ToReferenceWithoutVersion(), new CultureInfo("en"))];
                Assert.False(refContentMetadata.PersonalizedProperties.Any());
            });
        }

        [Fact]
        public async Task RequestWithExpandedProperty_WhenRequestContainsPersonalizeXhtmlString_ShouldBeAbleToTrackPersonalizeProperties()
        {
            var xhtmlString = CreatePersonalizedXhtmlString();
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            contentAreaItem.MainBody = xhtmlString;
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid, AllowedRoles = new string[] { _visitorGroup.Id.ToString() } });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                var refContentMetadata = contentTrackingContext.ReferencedContent[new LanguageContentReference(page.ContentLink, new CultureInfo("en"))];
                Assert.True(refContentMetadata.PersonalizedProperties.Any());
            });
        }
        #endregion

        #region IT tracking for referenced content, for both request by content reference and guid, adn with expanded properties
        [Fact]
        public async Task Request_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains(page.ContentLink, contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task RequestByGuid_WhenRequestContainsReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains(page.ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task RequestWithExpandedProperty_WhenRequestReferencedContent_ShouldBeAbleToTrackReferencedContentProperties()
        {
            var externalUrl = "http://www.external.url/";
            var xhtmlString = new XhtmlString(string.Format("<a href=\"{0}\">link</a>", externalUrl));
            var contentAreaItem = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            contentAreaItem.MainBody = xhtmlString;
            var contentArea = CreateContentArea(false);
            contentArea.Items.Add(new ContentAreaItem { ContentLink = contentAreaItem.ContentLink, ContentGuid = contentAreaItem.ContentGuid });
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: c => c.MainContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.MainContentArea)}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains(contentAreaItem.ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsContentLinks_ShouldBeAbleToTrackRefererredContent()
        {
            var firstReferredPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var secondReferredPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var thirdReferredPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.TargetReference = firstReferredPage.ContentLink;
                p.ContentReferenceList = new List<ContentReference>() { secondReferredPage.ContentLink };
                p.Links = new LinkItemCollection() { new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(thirdReferredPage.ContentGuid, ".aspx"), Text = thirdReferredPage.Name } };
            });

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains(firstReferredPage.ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
                Assert.Contains(secondReferredPage.ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
                Assert.Contains(thirdReferredPage.ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsPropertyUrl_ShouldBeAbleToTrackRefererredContent()
        {
            var targetedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var url = new Url(PermanentLinkUtility.GetPermanentLinkVirtualPath(targetedPage.ContentGuid, ".aspx"));

            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.Url = url);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains(targetedPage.ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsLinkInMultipleLanguage_ShouldBeAbleToTrackRefererredContentInTheSameLanguage()
        {
            var targetedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var svTargetedPage = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(targetedPage.ContentLink, true, "sv");
            var url = PermanentLinkUtility.GetPermanentLinkVirtualPath(targetedPage.ContentGuid, ".aspx");
            url = UriUtil.AddLanguageSelection(url, "en");

            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.Url = new Url(url));

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains(new LanguageContentReference(targetedPage.ContentLink.ToReferenceWithoutVersion(), targetedPage.Language), contentTrackingContext.ReferencedContent.Keys);
                Assert.DoesNotContain(new LanguageContentReference(svTargetedPage.ContentLink.ToReferenceWithoutVersion(), svTargetedPage.Language), contentTrackingContext.ReferencedContent.Keys);
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsContentArea_ShouldBeAbleToTrackRefererredContent()
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var contentArea = new ContentArea();
            contentArea.Items.Add(new ContentAreaItem
            {
                ContentLink = (block as IContent).ContentLink,
                ContentGuid = (block as IContent).ContentGuid
            });

            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p => p.ContentArea = contentArea);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains((block as IContent).ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsXhtmlStringWithContentFragment_ShouldBeAbleToTrackRefererredContent()
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);

            var templateLoader = ServiceLocator.Current.GetInstance<ITemplateControlLoader>();
            var securityMarkup = ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>().CreateSecuredFragmentMarkupGenerator();
            var publishedStateAssesor = ServiceLocator.Current.GetInstance<IPublishedStateAssessor>();
            var contentAccessEvaluator = ServiceLocator.Current.GetInstance<IContentAccessEvaluator>();
            var contextModeResolver = ServiceLocator.Current.GetInstance<IContextModeResolver>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var fragment = new ContentFragment(contentRepository, securityMarkup,
                    new DisplayOptions(), publishedStateAssesor, contextModeResolver, contentAccessEvaluator, new Dictionary<string, object>())
            { ContentLink = (block as IContent).ContentLink };

            var xhtmlString = new XhtmlString();
            xhtmlString.Fragments.Add(fragment);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains((block as IContent).ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenRequestContainsXhtmlStringWithUrlFragment_ShouldBeAbleToTrackRefererredContent()
        {
            var targetedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var url = PermanentLinkUtility.GetPermanentLinkVirtualPath(targetedPage.ContentGuid, ".aspx");
            var xhtmlString = new XhtmlString(String.Format("<a href=\"{0}\">MyLink</a>", url));

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p => p.MainBody = xhtmlString);

            await _fixture.WithContent(page, async () =>
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var contentTrackingContext = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                Assert.Contains((targetedPage as IContent).ContentLink.ToReferenceWithoutVersion(), contentTrackingContext.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenLinkItemCollectionContainsDeletedContentUrl_ShouldNotTrackRefererredContent()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Links = new LinkItemCollection() { new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(linkedPage.ContentGuid, ".aspx"), Text = linkedPage.Name } };
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                AssertResponse.OK(response);
                Assert.DoesNotContain(linkedPage.ContentLink.ToReferenceWithoutVersion(), context.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenUrlLinkToDeletedContent_ShouldNotTrackRefererredContent()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var page = _fixture.GetWithDefaultName<PropertyPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Url = PermanentLinkUtility.GetPermanentLinkVirtualPath(linkedPage.ContentGuid, ".aspx");
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                AssertResponse.OK(response);
                Assert.DoesNotContain(linkedPage.ContentLink.ToReferenceWithoutVersion(), context.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenContentAreaContainsDeletedContent_ShouldNotTrackEmptyContentReference()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);            

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                var contentArea = CreateContentArea(false);
                contentArea.Items.Add(new ContentAreaItem { ContentLink = linkedPage.ContentLink, ContentGuid = linkedPage.ContentGuid });
                p.MainContentArea = contentArea;
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                AssertResponse.OK(response);
                Assert.DoesNotContain(ContentReference.EmptyReference, context.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        [Fact]
        public async Task Request_WhenXhtmlStringContainsDeletedContent_ShouldNotTrackEmptyContentReference()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var templateLoader = ServiceLocator.Current.GetInstance<ITemplateControlLoader>();
            var securityMarkup = ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>().CreateSecuredFragmentMarkupGenerator();
            var publishedStateAssesor = ServiceLocator.Current.GetInstance<IPublishedStateAssessor>();
            var contentAccessEvaluator = ServiceLocator.Current.GetInstance<IContentAccessEvaluator>();
            var contextModeResolver = ServiceLocator.Current.GetInstance<IContextModeResolver>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                var fragment = new ContentFragment(contentRepository, securityMarkup,
                   new DisplayOptions(), publishedStateAssesor, contextModeResolver, contentAccessEvaluator, new Dictionary<string, object>())
                { ContentLink = (linkedPage as IContent).ContentLink };

                var xhtmlString = new XhtmlString();
                xhtmlString.Fragments.Add(fragment);

                p.MainBody = xhtmlString;
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                AssertResponse.OK(response);
                Assert.DoesNotContain(ContentReference.EmptyReference, context.ReferencedContent.Keys.Select(c => c.ContentLink));
            });
        }

        #endregion
    }

    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    [ServiceConfiguration(typeof(IContentFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class CaptureContextFilter : IContentFilter, IContentApiModelFilter
    {
        private readonly IContentApiTrackingContextAccessor _contentApiTrackingContextAccessor;

        public CaptureContextFilter(IContentApiTrackingContextAccessor contentApiTrackingContextAccessor)
        {
            _contentApiTrackingContextAccessor = contentApiTrackingContextAccessor;
        }

        public ContentApiTrackingContext LatestContentTrackingContext { get; private set; }
        public ContentApiTrackingContext LatestContentApiModelTrackingContext { get; private set; }

        public Type HandledContentModel => typeof(IContent);

        public Type HandledContentApiModel => typeof(ContentApiModel);

        public void Filter(IContent content, ConverterContext converterContext) => LatestContentTrackingContext = _contentApiTrackingContextAccessor.Current;

        public void Filter(ContentApiModel contentApiModel, ConverterContext converterContext) => LatestContentApiModelTrackingContext = _contentApiTrackingContextAccessor.Current;
    }
}
