using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Web;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache.ETag
{
    [Collection(IntegrationTestCollection.Name)]
    public partial class Content : IAsyncLifetime
    {
        private const string V2Uri = "api/episerver/v2.0/content";
        private ServiceFixture _fixture;
        private StandardPage _page;
        private StandardPage _linkedPage;

        public Content(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync()
        {
            _linkedPage = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            _page = _fixture.GetWithDefaultName<StandardPage>(_linkedPage.ContentLink, true, init: page =>
            {
                var contentArea = new ContentArea { Items = { new ContentAreaItem { ContentLink = _linkedPage.ContentLink } } };
                page.MainContentArea = contentArea;
            });
            var swedishVersion = _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(_page.ContentLink, true, "sv", init: page =>
            {
                var contentArea = new ContentArea { Items = { new ContentAreaItem { ContentLink = _linkedPage.ContentLink } } };
                page.MainContentArea = contentArea;
            });

            //Create some extra children for GetChildren tests
            _fixture.GetWithDefaultName<StandardPage>(_linkedPage.ContentLink, true);
            _fixture.GetWithDefaultName<StandardPage>(_linkedPage.ContentLink, true);
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _fixture.ContentRepository.Delete(_linkedPage.ContentLink, true, Security.AccessLevel.NoAccess);
            return Task.CompletedTask;
        }

        private string AbsoluteContentUrl(ContentReference contentLink) 
            => new Uri(SiteDefinition.Current.SiteUrl, _fixture.UrlResolver.GetUrl(contentLink, null, new Web.Routing.VirtualPathArguments 
            { 
                ContextMode = ContextMode.Default, 
                ValidateTemplate = false 
            })).ToString();
    }
}
