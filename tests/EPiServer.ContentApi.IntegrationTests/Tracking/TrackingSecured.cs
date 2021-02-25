using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Core.Internal;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class TrackingSecured : IAsyncLifetime
    {
        private const string V2Uri = "api/episerver/v2.0/content";
        private ContentReference _securedReference;
        private ContentReference _publicReference;
        private string _autorizedRole = "Authorized";
        private ServiceFixture _fixture;

        public TrackingSecured(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync()
        {
            _publicReference = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en").ContentLink.ToReferenceWithoutVersion();
            _securedReference = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en").ContentLink.ToReferenceWithoutVersion();
            var contentAccessRepository = ServiceLocator.Current.GetInstance<IContentSecurityRepository>();
            var securityDescriptor = contentAccessRepository.Get(_securedReference).CreateWritableClone() as IContentSecurityDescriptor;
            securityDescriptor.ToLocal(true);
            securityDescriptor.RemoveEntry(securityDescriptor.Entries.Single(e => e.Name.Equals(EveryoneRole.RoleName)));
            securityDescriptor.AddEntry(new AccessControlEntry(_autorizedRole, AccessLevel.Read, SecurityEntityType.Role));
            contentAccessRepository.Save(_securedReference, securityDescriptor, SecuritySaveType.Replace);

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _fixture.ContentRepository.Delete(_securedReference, true, AccessLevel.NoAccess);
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Get_WhenASecuredContentIsRequested_ShouldTrackSecuredContent()
        {
            using (var userContext = new UserScope("authenticated", _autorizedRole))
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{_securedReference}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.Contains(_securedReference, context.SecuredContent);
            }
        }


        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task GetChildren_WhenPartOfResponseIsSecured_ShouldTrackSecuredContent(bool authorized)
        {
            using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{ContentReference.StartPage}/children");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                Assert.Contains(_securedReference, context.SecuredContent);
                Assert.DoesNotContain(_publicReference, context.SecuredContent);
            }
        }


        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task GetChildren_WhenChildrenIsPublic_ShouldNotTrackSecuredContent(bool authorized)
        {
            var child = _fixture.GetWithDefaultName<StandardPage>(_publicReference, true, "en");

            await _fixture.WithContent(child, async () =>
            {
                using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
                {
                    await _fixture.Client.GetAsync(V2Uri + $"/{_publicReference}/children");
                    var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                    Assert.Empty(context.SecuredContent);
                }
            });
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task GetAncestors_WhenAncestorIsSecured_ShouldTrackSecuredContent(bool authorized)
        {
            var child = _fixture.GetWithDefaultName<StandardPage>(_securedReference, true, "en");

            await _fixture.WithContent(child, async () =>
            {
                using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
                {
                    await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors");
                    var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                    Assert.Contains(_securedReference, context.SecuredContent);
                }
            });
        }

        [Fact]
        public async Task Get_WhenContentContainsMetaDataProperty_ShouldNotTrackSecuredContentInMetaDataProperty()
        {
            var page = _fixture.GetWithDefaultName<StandardPage>(_securedReference, true);

            using (var userContext = new UserScope("authenticated", _autorizedRole))
            {
                await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand=*");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;

                // Since PageParentLink is a metadata property, it should not be tracked
                Assert.DoesNotContain(_securedReference.ToReferenceWithoutVersion(), context.SecuredContent);
            }
        }

        [Fact]
        public async Task Get_WhenContentContainsEmptyContentReference_ShouldNotTrackSecuredContent()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var pageWithContentAreaReference = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: page =>
            {
                var contentArea = new ContentArea();               
                var contentFragment = ServiceLocator.Current.GetInstance<ContentFragmentFactory>().CreateContentFragment(linkedPage.ContentLink, Guid.Empty, ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>().CreateSecuredFragmentMarkupGenerator());
                contentArea.Fragments.Add(contentFragment);
                page.MainContentArea = contentArea;
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(pageWithContentAreaReference, async () =>
            {                
                await _fixture.Client.GetAsync(V2Uri + $"/{pageWithContentAreaReference.ContentGuid}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;                
                Assert.DoesNotContain(ContentReference.EmptyReference, context.SecuredContent);                
            });
        }

        [Fact]
        public async Task GetWithExpand_WhenLinkItemCollectionContainsDeletedContent_ShouldNotTrackSecuredContent()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.Links = new LinkItemCollection() { new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(linkedPage.ContentGuid, ".aspx"), Text = linkedPage.Name } };
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.Links)}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                AssertResponse.OK(response);
                Assert.DoesNotContain(linkedPage.ContentLink.ToReferenceWithoutVersion(), context.SecuredContent);
            });
        }

        [Fact]
        public async Task GetWithExpand_WhenContentReferenceContainsDeletedContent_ShouldNotTrackSecuredContent()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.TargetReference = linkedPage.ContentLink;
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.TargetReference)}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                AssertResponse.OK(response);
                Assert.DoesNotContain(linkedPage.ContentLink.ToReferenceWithoutVersion(), context.SecuredContent);
            });
        }

        [Fact]
        public async Task GetWithExpand_WhenContentReferenceListContainsDeletedContent_ShouldNotTrackSecuredContent()
        {
            var linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);

            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, init: p =>
            {
                p.ContentReferenceList = new List<ContentReference>(new[] { linkedPage.ContentLink });
            });

            _fixture.ContentRepository.Delete(linkedPage.ContentLink, true, AccessLevel.NoAccess);

            await _fixture.WithContent(page, async () =>
            {
                var response = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentLink}?expand={nameof(StandardPage.ContentReferenceList)}");
                var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                AssertResponse.OK(response);
                Assert.DoesNotContain(linkedPage.ContentLink.ToReferenceWithoutVersion(), context.SecuredContent);
            });
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task Get_WhenContentAreaWithSecuredContent_ShouldTrackSecuredContent(bool authorized)
        {
            var pageWithContentAreaReference = _fixture.GetWithDefaultName<StandardPage>(_publicReference, true, "en", page =>
            {
                var contentArea = new ContentArea();
                var securedFragment = ServiceLocator.Current.GetInstance<ContentFragmentFactory>().CreateContentFragment(_securedReference, Guid.Empty, ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>().CreateSecuredFragmentMarkupGenerator());
                contentArea.Fragments.Add(securedFragment);
                var publicFragment = ServiceLocator.Current.GetInstance<ContentFragmentFactory>().CreateContentFragment(_publicReference, Guid.Empty, ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>().CreateSecuredFragmentMarkupGenerator());
                contentArea.Fragments.Add(publicFragment);
                page.MainContentArea = contentArea;
            });

            await _fixture.WithContent(pageWithContentAreaReference, async () =>
            {
                using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
                {
                    await _fixture.Client.GetAsync(V2Uri + $"/{pageWithContentAreaReference.ContentGuid}");
                    var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                    Assert.Contains(_securedReference, context.SecuredContent);
                    Assert.DoesNotContain(_publicReference, context.SecuredContent);
                }
            });
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task Get_WhenXhtmlStringWithSecuredContent_ShouldTrackSecuredContent(bool authorized)
        {
            var securityMarkup = ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>().CreateSecuredFragmentMarkupGenerator();
            var publishedStateAssesor = ServiceLocator.Current.GetInstance<IPublishedStateAssessor>();
            var contentAccessEvaluator = ServiceLocator.Current.GetInstance<IContentAccessEvaluator>();
            var contextModeResolver = ServiceLocator.Current.GetInstance<IContextModeResolver>();

            var pageWithContentAreaReference = _fixture.GetWithDefaultName<StandardPage>(_publicReference, true, "en", page =>
            {
                var xhtmlString = new XhtmlString();
                xhtmlString.Fragments.Add(new StaticFragment("First"));
                xhtmlString.Fragments.Add(new ContentFragment(ServiceLocator.Current.GetInstance<IContentLoader>(), securityMarkup,
                    new DisplayOptions(), publishedStateAssesor, contextModeResolver, contentAccessEvaluator, new Dictionary<string, object>())
                { ContentLink = _securedReference });
                xhtmlString.Fragments.Add(new StaticFragment("Second"));
                page.MainBody = xhtmlString;
            });

            await _fixture.WithContent(pageWithContentAreaReference, async () =>
            {
                using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
                {
                    await _fixture.Client.GetAsync(V2Uri + $"/{pageWithContentAreaReference.ContentGuid}");
                    var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                    Assert.Contains(_securedReference, context.SecuredContent);
                }
            });
        }


        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task Get_WhenASecuredContentReferenceIsExpanded_ShouldTrackContent(bool authorized)
        {
            var pageWithContentReference = _fixture.GetWithDefaultName<StandardPage>(_publicReference, true, "en", page =>
            {
                page.TargetReference = _securedReference;
            });

            await _fixture.WithContent(pageWithContentReference, async () =>
            {
                using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
                {
                    await _fixture.Client.GetAsync(V2Uri + $"/{pageWithContentReference.ContentGuid}?expand={nameof(StandardPage.TargetReference)}");
                    var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                    Assert.Contains(_securedReference, context.SecuredContent);
                }
            });
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task Get_WhenASecuredContentReferenceListIsExpanded_ShouldTrackContent(bool authorized)
        {
            var pageWithContentReferenceList = _fixture.GetWithDefaultName<StandardPage>(_publicReference, true, "en", page =>
            {
                page.ContentReferenceList = new List<ContentReference>(new []{ _securedReference });
            });

            await _fixture.WithContent(pageWithContentReferenceList, async () =>
            {
                using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
                {
                    await _fixture.Client.GetAsync(V2Uri + $"/{pageWithContentReferenceList.ContentGuid}?expand={nameof(StandardPage.ContentReferenceList)}");
                    var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                    Assert.Contains(_securedReference, context.SecuredContent);
                }
            });
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task Get_WhenASecuredLinkCollectionIsExpanded_ShouldTrackContent(bool authorized)
        {
            var securedPage = _fixture.ContentRepository.Get<IContent>(_securedReference);
            var pageWithLinkCollection = _fixture.GetWithDefaultName<StandardPage>(_publicReference, true, "en", page =>
            {
                var links = new LinkItemCollection { new LinkItem { Href = PermanentLinkUtility.GetPermanentLinkVirtualPath(securedPage.ContentGuid, ".aspx"), Text = securedPage.Name } };
                page.Links = links;
            });

            await _fixture.WithContent(pageWithLinkCollection, async () =>
            {
                using (var userContext = new UserScope("authenticated", authorized ? _autorizedRole : null))
                {
                    await _fixture.Client.GetAsync(V2Uri + $"/{pageWithLinkCollection.ContentGuid}?expand={nameof(StandardPage.Links)}");
                    var context = ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>().Current;
                    Assert.Contains(_securedReference, context.SecuredContent);
                }
            });
        }
    }
}
