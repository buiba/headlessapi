using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Microsoft.Owin.Testing;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup
{
    public class CommerceServiceFixture : IDisposable
    {
        private readonly DatabaseFixture _cmsDatabaseFixture;
        private readonly CommerceDatabaseFixture _commerceDatabaseFixture;
        private readonly EPiServerFixture _episerverFixture;

        private readonly TestServer _server;
        public HttpClient Client => _server.HttpClient;
        public IContentRepository ContentRepository { get; }

        public CommerceServiceFixture()
        {
            try
            {
                _cmsDatabaseFixture = new DatabaseFixture("CMS_Test");
                EPiServerTestInitialization.CmsDatabase = _cmsDatabaseFixture.CmsDatabase;
                _commerceDatabaseFixture = new CommerceDatabaseFixture();
                _episerverFixture = new EPiServerFixture();
                _server = TestServer.Create<Startup>();
                ContentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }
        public async Task WithContentItems(ContentReference[] contentLinks, Func<Task> run)
        {
            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                ClearContentItems(contentLinks);
            }
        }

        public void ClearContentItems(IEnumerable<ContentReference> contentLinks)
        {
            foreach (var contentLink in contentLinks)
            {
                try
                {
                    ContentRepository.Delete(contentLink, true, Security.AccessLevel.NoAccess);
                }
                catch { }
            }
        }
        public void Dispose()
        {
            _server?.Dispose();
            _episerverFixture?.Dispose();
            _commerceDatabaseFixture?.Dispose();
            _cmsDatabaseFixture?.Dispose();
        }
    }
}
