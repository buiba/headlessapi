
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class GetItems
    {
        private const string BaseV2Uri = "api/episerver/v2.0/content";
        private readonly ServiceFixture _fixture;

        public GetItems(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetItems_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, BaseV2Uri + $"?references=some-content-reference");

            var contentResponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(contentResponse);
        }

        [Fact]
        public async Task GetItems_WhenContentReferenceIsInvalid_ShouldThrowBadRequest()
        {         
            var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?references=invalid-content-reference");
            AssertResponse.BadRequest(contentResponse);

            var errorResponse = await contentResponse.Content.ReadAs<ErrorResponse>();
            Assert.Equal(ErrorCode.InvalidParameter, errorResponse.Error.Code);
            Assert.Equal("The content reference is not in a valid format", errorResponse.Error.Message);
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentReferenceAsString_ShouldReturnContent()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?references={parent.ContentLink},{child.ContentLink}");
                AssertResponse.OK(contentResponse);
                var contentItems = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(2, contentItems.Count);
                Assert.Equal(parent.Name, (string)contentItems[0]["name"]);
                Assert.Equal(child.Name, (string)contentItems[1]["name"]);
            });
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentGuids_ShouldReturnContent()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var contentItems = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(2, contentItems.Count);
                Assert.Equal(parent.Name, (string)contentItems[0]["name"]);
                Assert.Equal(child.Name, (string)contentItems[1]["name"]);
            });
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentGuidsAndSpecificProperties_ShouldReturnContentWithOnlySelectedProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?guids={parent.ContentGuid},{child.ContentGuid}&select=name");
                AssertResponse.OK(contentResponse);
                var contentItems = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(2, contentItems.Count);
                Assert.Equal(parent.Name, (string)contentItems[0]["name"]);
                Assert.Equal(child.Name, (string)contentItems[1]["name"]);
                Assert.Null(contentItems[0]["url"]);
                Assert.Null(contentItems[0]["category"]);
                Assert.Null(contentItems[1]["url"]);
                Assert.Null(contentItems[1]["category"]);
            });
        }

        [Fact]
        public async Task GetItems_WhenRequestedWithContentReferencesAndSpecificProperties_ShouldReturnContentWithOnlySelectedProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(BaseV2Uri + $"?references={parent.ContentLink},{child.ContentLink}&select=name");
                AssertResponse.OK(contentResponse);
                var contentItems = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(2, contentItems.Count);
                Assert.Equal(parent.Name, (string)contentItems[0]["name"]);
                Assert.Equal(child.Name, (string)contentItems[1]["name"]);
                Assert.Null(contentItems[0]["url"]);
                Assert.Null(contentItems[0]["category"]);
                Assert.Null(contentItems[1]["url"]);
                Assert.Null(contentItems[1]["category"]);
            });
        }
    }
}
