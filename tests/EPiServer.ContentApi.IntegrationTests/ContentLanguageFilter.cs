using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class ContentLanguageFilter : IAsyncLifetime
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private ServiceFixture _fixture;
        private IContent _page;

        public ContentLanguageFilter(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync()
        {
            _page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            _fixture.CreateLanguageBranchWithDefaultName<StandardPage>(_page.ContentLink, true, "sv");

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _fixture.ContentRepository.Delete(_page.ContentLink, true, AccessLevel.NoAccess);
            return Task.CompletedTask;
        }

        [Fact]
        public async Task RequestByContentReference_WhenRequestContainsHeaderLanguage_ContentLanguageShouldBeSetAccordingly()
        {   
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{_page.ContentLink.ToReferenceWithoutVersion()}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));
           
            await _fixture.Client.SendAsync(requestMessage);
            var contentLanguage = ServiceLocator.Current.GetAllInstances<IContentApiModelFilter>().OfType<CaptureContentLanguageFilter>().Single().ContentLanguage;
            Assert.Equal("sv", contentLanguage.Name);                           
        }

        [Fact]
        public async Task RequestByGuid_WhenRequestContainsHeaderLanguage_ContentLanguageShouldBeSetAccordingly()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{_page.ContentGuid}");
            requestMessage.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));

            await _fixture.Client.SendAsync(requestMessage);
            var contentLanguage = ServiceLocator.Current.GetAllInstances<IContentApiModelFilter>().OfType<CaptureContentLanguageFilter>().Single().ContentLanguage;
            Assert.Equal("sv", contentLanguage.Name);
        }
    }    

    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]    
    internal class CaptureContentLanguageFilter : IContentApiModelFilter
    {
        private readonly IContentLanguageAccessor _contentLanguageAccessor;

        public CaptureContentLanguageFilter(IContentLanguageAccessor contentLanguageAccessor)
        {
            _contentLanguageAccessor = contentLanguageAccessor;
        }

        public CultureInfo ContentLanguage { get; private set; }

        public Type HandledContentApiModel => typeof(ContentApiModel);

        public void Filter(ContentApiModel contentApiModel, ConverterContext converterContext) => ContentLanguage = _contentLanguageAccessor.Language;
    }
}
