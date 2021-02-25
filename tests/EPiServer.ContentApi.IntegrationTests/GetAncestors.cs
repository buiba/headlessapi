
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class GetAncestors
    {
        private const string V2Uri = "api/episerver/v2.0/content";
        private readonly ServiceFixture _fixture;

        public GetAncestors(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAncestors_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, V2Uri + "/some-content-link/ancestors");

            var contentResponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(contentResponse);
        }

        [Fact]
        public async Task GetAncestors_WhenContentReferenceIsInvalid_ShouldThrowBadRequest()
        {  
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/invalid-content-reference/ancestors");
            AssertResponse.BadRequest(contentResponse);
            var errorResponse = await contentResponse.Content.ReadAs<ErrorResponse>();
            Assert.Equal(ErrorCode.InvalidParameter, errorResponse.Error.Code);
            Assert.Equal("The content reference is not in a valid format", errorResponse.Error.Message);
        }

        [Fact]
        public async Task GetAncestors_WhenContentHasAncestors_ShouldReturnAncestors()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var ancestorResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentGuid}/ancestors");
                AssertResponse.OK(ancestorResponse);
                var ancestors = JArray.Parse(await ancestorResponse.Content.ReadAsStringAsync());
                Assert.Equal(3, ancestors.Count); //Root, Startpage, Parent
                Assert.Equal(parent.Name, (string)ancestors[0]["name"]);
                Assert.Equal("Start", (string)ancestors[1]["name"]);
                Assert.Equal("Root", (string)ancestors[2]["name"]);
            });
        }

        [Fact]
        public async Task GetAncestors_ByContentReference_WhenContentHasAncestors_ShouldReturnAncestors()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var ancestorResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors");
                AssertResponse.OK(ancestorResponse);
                var ancestors = JArray.Parse(await ancestorResponse.Content.ReadAsStringAsync());
                Assert.Equal(3, ancestors.Count); //Root, Startpage, Parent
                Assert.Equal(parent.Name, (string)ancestors[0]["name"]);
                Assert.Equal("Start", (string)ancestors[1]["name"]);
                Assert.Equal("Root", (string)ancestors[2]["name"]);
            });
        }

        [Fact]
        public async Task GetAncestors_WhenContentDoesNotExist_ShouldReturnNotFound()
        {
            var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{Guid.NewGuid()}/ancestors");
            AssertResponse.NotFound(childrenResponse);
        }

        [Fact]
        public async Task GetAncestors_WhenAncestorDoesNotExistInSpecifiedLanguage_ShouldFallbackToMaster()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true, "sv");
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{child.ContentGuid}/ancestors");
                httpRequest.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));
                var ancestorResponse = await _fixture.Client.SendAsync(httpRequest);
                AssertResponse.OK(ancestorResponse);
                var ancestors = JArray.Parse(await ancestorResponse.Content.ReadAsStringAsync());
                Assert.Equal(3, ancestors.Count); //Root, Startpage, Parent
                Assert.Equal(parent.Name, (string)ancestors[0]["name"]);
            });
        }

        [Fact]
        public async Task GetAncestors_WhenRequestedWithContentGuidAndSpecificProperties_ShouldReturnContentWithOnlySelectedProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var ancestorResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentGuid}/ancestors?select=name");
                AssertResponse.OK(ancestorResponse);
                var ancestors = JArray.Parse(await ancestorResponse.Content.ReadAsStringAsync());
                Assert.Equal(parent.Name, (string)ancestors[0]["name"]);
                Assert.Equal("Start", (string)ancestors[1]["name"]);
                Assert.Equal("Root", (string)ancestors[2]["name"]);
                Assert.Null(ancestors[0]["url"]);
                Assert.Null(ancestors[0]["category"]);
                Assert.Null(ancestors[1]["url"]);
                Assert.Null(ancestors[1]["category"]);
                Assert.Null(ancestors[2]["url"]);
                Assert.Null(ancestors[2]["category"]);
            });
        }

        [Fact]
        public async Task GetAncestors_WhenRequestedWithContentReferenceAndSpecificProperties_ShouldReturnContentWithOnlySelectedProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var ancestorResponse = await _fixture.Client.GetAsync(V2Uri + $"/{child.ContentLink}/ancestors?select=name");
                AssertResponse.OK(ancestorResponse);
                var ancestors = JArray.Parse(await ancestorResponse.Content.ReadAsStringAsync());
                Assert.Equal(parent.Name, (string)ancestors[0]["name"]);
                Assert.Equal("Start", (string)ancestors[1]["name"]);
                Assert.Equal("Root", (string)ancestors[2]["name"]);
                Assert.Null(ancestors[0]["url"]);
                Assert.Null(ancestors[0]["category"]);
                Assert.Null(ancestors[1]["url"]);
                Assert.Null(ancestors[1]["category"]);
                Assert.Null(ancestors[2]["url"]);
                Assert.Null(ancestors[2]["category"]);
            });
        }
    }
}
