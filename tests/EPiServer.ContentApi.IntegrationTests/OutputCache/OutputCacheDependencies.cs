using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache
{
    [Collection(IntegrationTestCollection.Name)]
    public class OutputCacheDependencies
    {
        private ServiceFixture _fixture;
        private IContentCacheKeyCreator _contentCacheKeyCreator;

        public OutputCacheDependencies(ServiceFixture fixture)
        {
            _fixture = fixture;
            _contentCacheKeyCreator = ServiceLocator.Current.GetInstance<IContentCacheKeyCreator>();
        }

        [Fact]
        public void EventListener_WhenANewContentIsPublished_ShouldAddParentListing()
        {
            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                var newPublishedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
                Assert.Contains(_contentCacheKeyCreator.CreateChildrenCacheKey(ContentReference.StartPage, null), captureProvider.CapturedDependencyKeys);
                _fixture.ContentRepository.Delete(newPublishedContent.ContentLink, true, Security.AccessLevel.NoAccess);
            }
        }

        [Fact]
        public void EventListener_WhenANewVersionIsPublished_ShouldAddContent()
        {
            var content = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en").CreateWritableClone();
            content.Name = Guid.NewGuid().ToString("N");
            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                _fixture.ContentRepository.Save(content, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);
                Assert.Contains(_contentCacheKeyCreator.CreateLanguageCacheKey(content.ContentLink, "en"), captureProvider.CapturedDependencyKeys);
                _fixture.ContentRepository.Delete(content.ContentLink, true, Security.AccessLevel.NoAccess);
            }           
        }

        [Fact]
        public void EventListener_WhenDelete_ShouldAddContentAndDescendentsAndListings()
        {
            var newPublishedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var firstLevel = _fixture.GetWithDefaultName<StandardPage>(newPublishedContent.ContentLink, true, "en");
            var secondLevel = _fixture.GetWithDefaultName<StandardPage>(firstLevel.ContentLink, true, "en");

            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                _fixture.ContentRepository.Delete(newPublishedContent.ContentLink, true, Security.AccessLevel.NoAccess);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(newPublishedContent.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(firstLevel.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(secondLevel.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateChildrenCacheKey(ContentReference.StartPage, null), captureProvider.CapturedDependencyKeys);
            }
            
        }

        [Fact]
        public void EventListener_WhenDeleteChildren_ShouldAddDescendentsAndListings()
        {
            var newPublishedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var firstLevel = _fixture.GetWithDefaultName<StandardPage>(newPublishedContent.ContentLink, true, "en");
            var secondLevel = _fixture.GetWithDefaultName<StandardPage>(firstLevel.ContentLink, true, "en");

            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                _fixture.ContentRepository.DeleteChildren(newPublishedContent.ContentLink, true, Security.AccessLevel.NoAccess);
                Assert.DoesNotContain(_contentCacheKeyCreator.CreateCommonCacheKey(newPublishedContent.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(firstLevel.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(secondLevel.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateChildrenCacheKey(newPublishedContent.ContentLink, null), captureProvider.CapturedDependencyKeys);
            }

            _fixture.ContentRepository.Delete(newPublishedContent.ContentLink, true, Security.AccessLevel.NoAccess);
        }

        [Fact]
        public void EventListener_WhenDeleteLanguage_ShouldAddContentWithCommonKey()
        {
            var newPublishedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var swedishVersion = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(newPublishedContent.ContentLink, true, "sv");

            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                _fixture.ContentRepository.DeleteLanguageBranch(newPublishedContent.ContentLink, "sv", Security.AccessLevel.NoAccess);
                Assert.DoesNotContain(_contentCacheKeyCreator.CreateLanguageCacheKey(newPublishedContent.ContentLink, "en"), captureProvider.CapturedDependencyKeys);
                Assert.DoesNotContain(_contentCacheKeyCreator.CreateLanguageCacheKey(newPublishedContent.ContentLink, "sv"), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(newPublishedContent.ContentLink), captureProvider.CapturedDependencyKeys);
            }

            _fixture.ContentRepository.Delete(newPublishedContent.ContentLink, true, Security.AccessLevel.NoAccess);
        }

        [Fact]
        public void EventListener_WhenMoved_ShouldAddContentDescendentsAndListings()
        {
            var newPublishedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var child = _fixture.GetWithDefaultName<StandardPage>(newPublishedContent.ContentLink, true, "en");
            var descendent = _fixture.GetWithDefaultName<StandardPage>(child.ContentLink, true, "en");

            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                _fixture.ContentRepository.Move(newPublishedContent.ContentLink, ContentReference.RootPage, AccessLevel.NoAccess, AccessLevel.NoAccess);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(newPublishedContent.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(child.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(descendent.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateChildrenCacheKey(ContentReference.StartPage, null), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateChildrenCacheKey(ContentReference.RootPage, null), captureProvider.CapturedDependencyKeys);
            }

            
            _fixture.ContentRepository.Delete(newPublishedContent.ContentLink, true, Security.AccessLevel.NoAccess);
        }

        [Fact]
        public void EventListener_WhenAccessRightsAreChanged_ShouldAddContentDescendentsAndListings()
        {
            var newPublishedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var child = _fixture.GetWithDefaultName<StandardPage>(newPublishedContent.ContentLink, true, "en");
            var descendent = _fixture.GetWithDefaultName<StandardPage>(child.ContentLink, true, "en");
            var contentSecurityRepository = ServiceLocator.Current.GetInstance<IContentSecurityRepository>();
            var acl = contentSecurityRepository.Get(newPublishedContent.ContentLink).CreateWritableClone() as IContentSecurityDescriptor;
            acl.ToLocal();
            acl.RemoveEntry(acl.Entries.Single(e => e.Name.Equals(EveryoneRole.RoleName)));

            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                contentSecurityRepository.Save(newPublishedContent.ContentLink, acl, SecuritySaveType.Replace);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(newPublishedContent.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(child.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateCommonCacheKey(descendent.ContentLink), captureProvider.CapturedDependencyKeys);
                Assert.Contains(_contentCacheKeyCreator.CreateChildrenCacheKey(ContentReference.StartPage, null), captureProvider.CapturedDependencyKeys);
            }

            _fixture.ContentRepository.Delete(newPublishedContent.ContentLink, true, Security.AccessLevel.NoAccess);
        }

        [Fact]
        public void EventListener_WhenSiteDefinitionsAreChanged_ShouldSendSiteDependency()
        {
            var siteDefinitionRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
            var startPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true, "en");
            var siteDefinition = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "SomeSiteName",
                SiteUrl = new Uri("http://somesite.com"),
                StartPage = startPage.ContentLink
            };

            var captureProvider = new CapturingOutputCacheProvider();
            using (new OutputCacheProviderScope(captureProvider))
            {
                siteDefinitionRepository.Save(siteDefinition);
                Assert.Contains($"{SiteOutputCacheEvaluator.SiteDependency}{ReferencedSiteMetadata .DefaultInstance.Id}", captureProvider.CapturedDependencyKeys);
            }

            siteDefinitionRepository.Delete(siteDefinition.Id);
            _fixture.ContentRepository.Delete(startPage.ContentLink, true, Security.AccessLevel.NoAccess);
        }
    }
}
