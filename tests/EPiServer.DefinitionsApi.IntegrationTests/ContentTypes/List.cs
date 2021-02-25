using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class List
    {
        private readonly ServiceFixture _fixture;

        public List(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ListAsync_ShouldReturnContentTypeInArray()
        {
            // Create content type
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };
            (await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType))).EnsureSuccessStatusCode();

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                .Which.Should().NotBeEmpty()
                .And.Contain(x => new Guid((string)x["id"]) == contentType.id);
        }


        [Fact]
        public async Task ListAsync_WhenProvidingTop_ShouldReturnLimitReturnedResultsAndProvideContinuationToken()
        {
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + "?top=1");

            AssertResponse.OK(response);

            Assert.NotEmpty(response.Headers.GetValues(ContinuationToken.HeaderName));

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                .Which.Should().HaveCountLessOrEqualTo(1);
        }

        [Fact]
        public async Task ListAsync_WhenProvidingBothTopAndContinuationToken_ShouldReturnBadRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ContentTypesController.RoutePrefix + "?top=100");
            request.Headers.Add(ContinuationToken.HeaderName, new ContinuationToken(0, 10).AsTokenString());

            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task ListAsync_WhenContinuationTokenHasInvalidContent_ShouldReturnBadRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ContentTypesController.RoutePrefix);
            request.Headers.Add(ContinuationToken.HeaderName, "Invalid token");

            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task ListAsync_ShouldFilterOutContentFolderTypesInArray()
        {
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                .Which.Should().NotBeEmpty()
                .And.Contain(x => new Guid((string)x["id"]) == SystemContentTypes.ContentFolder)
                .And.Contain(x => new Guid((string)x["id"]) == SystemContentTypes.ContentAssetFolder);
        }

        [Fact]
        public async Task ListAsync_ShouldNotReturnSystemContentTypeThatWillBeRemovedLongTerm()
        {
            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                .Which.Should().NotContain(x => new Guid((string)x["id"]) == SystemContentTypes.RootPage)
                .And.NotContain(x => new Guid((string)x["id"]) == SystemContentTypes.RecycleBin);
        }

        [Fact]
        public async Task ListAsync_WhenContentTypeWithEditSettingsExists_ShouldReturnContentTypeWithEditSettings()
        {
            // Create content type
            var editSettings = new { displayName = "somename", description = "somedescription", available = true, order = 200 };
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString(), editSettings };
            (await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType))).EnsureSuccessStatusCode();

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                .Which.Should().NotBeEmpty()
                .And.Contain(x =>
                    new Guid((string)x["id"]) == contentType.id &&
                    (string)x["editSettings"]["displayName"] == editSettings.displayName &&
                    (string)x["editSettings"]["description"] == editSettings.description &&
                    (bool)x["editSettings"]["available"] == editSettings.available &&
                    (int)x["editSettings"]["order"] == editSettings.order
                );
        }

        [Fact]
        public async Task ListAsync_WhenContentTypeWithoutEditSettingsExists_ShouldReturnContentTypeWithoutEditSettings()
        {
            // Create content type
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };
            (await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType))).EnsureSuccessStatusCode();

            var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix);

            AssertResponse.OK(response);

            var content = await response.Content.ReadAsStringAsync();

            content.Should().BeValidJson()
                .Which.Should().BeAssignableTo<IEnumerable<JToken>>()
                .Which.Should().NotBeEmpty()
                .And.Contain(x => new Guid((string)x["id"]) == contentType.id && x["editSettings"] == null);
        }
    }
}
