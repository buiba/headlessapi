using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache
{
    [Collection(IntegrationTestCollection.Name)]
    public class OutputCacheFilter : IAsyncLifetime
    {
        private const string V2Uri = "api/episerver/v2.0/content";
        private ServiceFixture _fixture;
        private ContentReference _securedReference;
        private ContentReference _publicReference;
        private string _autorizedRole = "Authorized";

        public OutputCacheFilter(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync()
        {            
            _securedReference = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en").ContentLink.ToReferenceWithoutVersion();
            _publicReference = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en").ContentLink.ToReferenceWithoutVersion();
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
        public async Task ExecuteAction_WhenResponseIsNot200_ShouldNotAddEtagAndCacheHeaders()
        {
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_securedReference}");
            Assert.Null(contentResponse.Headers.ETag);
            Assert.Null(contentResponse.Headers.CacheControl);
        }

        [Fact]
        public async Task ExecuteAction_WhenEvaluateResultIsNotCacheable_ShouldNotAddEtagAndCacheHeaders()
        {
            using (var userContext = new UserScope("authenticated", _autorizedRole))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_securedReference}");
                Assert.Null(contentResponse.Headers.ETag);
                Assert.Null(contentResponse.Headers.CacheControl);
            }            
        }

        [Fact]
        public async Task ExecuteAction_WhenEvaluateResultIsCacheable_ShouldAddEtagAndCacheHeaders()
        {
            var contentApiConfiguration = ServiceLocator.Current.GetInstance<ContentApiConfiguration>();            

            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{_publicReference}");
            Assert.NotNull(contentResponse.Headers.ETag);
            Assert.True(contentResponse.Headers.CacheControl.Public);
            Assert.Equal(contentResponse.Headers.CacheControl.SharedMaxAge, contentApiConfiguration.Default().HttpResponseExpireTime);            
        }
    }
}
