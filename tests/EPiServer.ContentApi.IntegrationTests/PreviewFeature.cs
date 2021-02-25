using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{

    [Collection(IntegrationTestCollection.Name)]
    public class PreviewFeature
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private ServiceFixture _fixture;

        public PreviewFeature(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task RequestingGet_WithoutEnablePreviewFeature_ShouldReturnContent()
        {
            using (var previewScope = new PreviewScope(false))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{IntegrationTestCollection.StartPageGuId}");
                AssertResponse.OK(contentResponse);
            }
        }

        [Fact]
        public async Task RequestingGetChildren_WithoutEnablePreviewFeature_ShouldReturnForbidden()
        {
            using (var previewScope = new PreviewScope(false))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{IntegrationTestCollection.StartPageGuId}/children");
                AssertResponse.StatusCode(System.Net.HttpStatusCode.Forbidden, contentResponse);
            }
        }

        [Fact]
        public async Task RequestingGetChildren_WithEnabledPreviewFeature_ShouldReturnContent()
        {
            using (var previewScope = new PreviewScope(true))
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{IntegrationTestCollection.StartPageGuId}/children");
                AssertResponse.OK(contentResponse);
            }
        }

        private class PreviewScope : IDisposable
        {
            private readonly ContentApiConfiguration _contentApiConfiguration;
            private readonly bool _orgValue;

            public PreviewScope(bool enablePreview)
            {
                _contentApiConfiguration = ServiceLocator.Current.GetInstance<ContentApiConfiguration>();
                _orgValue = _contentApiConfiguration.EnablePreviewFeatures;
                _contentApiConfiguration.EnablePreviewFeatures = enablePreview;
            }

            public void Dispose()
            {
                _contentApiConfiguration.EnablePreviewFeatures = _orgValue;
            }
        }
    }
}
