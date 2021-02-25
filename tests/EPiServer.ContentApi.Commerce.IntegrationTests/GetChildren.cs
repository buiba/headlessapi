using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class GetChildren : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;
        
        public GetChildren(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetChildren_WhenContentIsProduct_ShouldGetChildren()
        {
            var product = GetWithDefaultName<ProductContent>(CatalogContentLink);
            var child = GetWithDefaultName<VariationContent>(product.ContentLink);
            await _fixture.WithContentItems(new[] {product.ContentLink, child.ContentLink}, async () =>
            {
                var childrenResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{product.ContentGuid}/children");
                AssertResponse.OK(childrenResponse);
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());

                Assert.Single(children);
                Assert.Equal(child.Name, (string) children[0]["name"]);
            });
        }

        [Fact]
        public async Task GetChildren_WhenContentIsBundle_ShouldGetChildren()
        {
            var bundle = GetWithDefaultName<BundleContent>(CatalogContentLink);
            var child = GetWithDefaultName<VariationContent>(bundle.ContentLink);
            await _fixture.WithContentItems(new[] {bundle.ContentLink, child.ContentLink}, async () =>
            {
                var childrenResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{bundle.ContentGuid}/children");
                
                AssertResponse.OK(childrenResponse);
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());

                Assert.Single(children);
                Assert.Equal(child.Name, (string) children[0]["name"]);
            });
        }

        [Fact]
        public async Task GetChildren_WhenContentIsPackage_ShouldGetChildren()
        {
            var package = GetWithDefaultName<PackageContent>(CatalogContentLink);
            var child = GetWithDefaultName<VariationContent>(package.ContentLink);

            await _fixture.WithContentItems(new[] {package.ContentLink, child.ContentLink}, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{package.ContentGuid}/children");
                AssertResponse.OK(contentResponse);
                var children = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());

                Assert.Single(children);
                Assert.Equal(child.Name, (string) children[0]["name"]);
            });
        }

        [Fact]
        public async Task GetChildren_WhenContentExistButHaveNoChildren_ShouldOkAndEmptyList()
        {
            var parent = GetWithDefaultName<PackageContent>(CatalogContentLink);
            await _fixture.WithContentItems(new[] { parent.ContentLink }, async () =>
            {
                var childrenResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{parent.ContentGuid}/children");
                AssertResponse.OK(childrenResponse);
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Empty(children);
            });
        }

        [Fact]
        public async Task GetChildren_WhenContentDoesNotExist_ShouldReturnNotFound()
        {
            var childrenResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{Guid.NewGuid()}/children");
            AssertResponse.NotFound(childrenResponse);
        }

        [Fact]
        public async Task GetChildren_WhenSeveralChildren_ShouldSupportPaging()
        {
            var product = GetWithDefaultName<ProductContent>(CatalogContentLink);
            var firstChild = GetWithDefaultName<VariationContent>(product.ContentLink);
            var secondChild = GetWithDefaultName<VariationContent>(product.ContentLink);
            await _fixture.WithContentItems(new[] { product.ContentLink, firstChild.ContentLink, secondChild.ContentLink}, async () =>
            {
                var retrievedIds = new List<Guid>();

                var childrenResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{product.ContentGuid}/children?top=1");
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string) children[0]["contentLink"]["guidValue"]));

                var pagingToken = childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName);

                var httpRequestMessage =
                    new HttpRequestMessage(HttpMethod.Get, Constants.ContentApiV2Url + $"/{product.ContentGuid}/children");
                httpRequestMessage.Headers.Add(PagingConstants.ContinuationTokenHeaderName, pagingToken);

                childrenResponse = await _fixture.Client.SendAsync(httpRequestMessage);
                children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string) children[0]["contentLink"]["guidValue"]));

                Assert.Contains(firstChild.ContentGuid, retrievedIds);
                Assert.Contains(secondChild.ContentGuid, retrievedIds);
            });
        }

        [Fact]
        public async Task GetChildren_WhenContinuationTokenIsPassedAndtop_ShouldReturnBadRequest()
        {
            var product = GetWithDefaultName<ProductContent>(CatalogContentLink);
            var firstChild = GetWithDefaultName<VariationContent>(product.ContentLink);
            var secondChild = GetWithDefaultName<VariationContent>(product.ContentLink);
            await _fixture.WithContentItems(new[] { product.ContentLink, firstChild.ContentLink, secondChild.ContentLink }, async () =>
            {
                var retrievedIds = new List<Guid>();
                var childrenResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{product.ContentGuid}/children?top=1");
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string)children[0]["contentLink"]["guidValue"]));

                var pagingToken = childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Constants.ContentApiV2Url + $"/{product.ContentGuid}/children?top=1");
                httpRequestMessage.Headers.Add(PagingConstants.ContinuationTokenHeaderName, pagingToken);

                childrenResponse = await _fixture.Client.SendAsync(httpRequestMessage);
                AssertResponse.StatusCode(HttpStatusCode.BadRequest, childrenResponse);
            });
        }

        [Fact]
        public async Task GetChildren_WhenChildExistInLanguageButNotParent_ShouldReturnChild()
        {
            var product = GetWithDefaultName<ProductContent>(CatalogContentLink, language: "en");
            var child = GetWithDefaultName<VariationContent>(product.ContentLink, language: "sv");
            await _fixture.WithContentItems(new[] { product.ContentLink, child.ContentLink }, async () =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, Constants.ContentApiV2Url + $"/{product.ContentGuid}/children");
                httpRequest.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));
                var childrenResponse = await _fixture.Client.SendAsync(httpRequest);
                AssertResponse.OK(childrenResponse);
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                Assert.Equal("sv", (string)children[0]["language"]["name"]);
            });
        }

        [Fact]
        public async Task GetChildren_ForNodes_WhenChildExist_ShouldReturnChild()
        {
            var node = GetWithDefaultName<NodeContent>(CatalogContentLink);
            var child = GetWithDefaultName<VariationContent>(node.ContentLink);
            await _fixture.WithContentItems(new[] { node.ContentLink, child.ContentLink }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{node.ContentGuid}/children");
                AssertResponse.OK(contentResponse);
                var children = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());

                Assert.Single(children);
                Assert.Equal(child.Name, (string)children[0]["name"]);
            });
        }

        [Fact]
        public async Task GetChildren_ForCatalog_WhenChildrenExist_ShouldReturnChildren()
        {
            var nodeParent = GetWithDefaultName<NodeContent>(CatalogContentLink);
            var nodeChild = GetWithDefaultName<NodeContent>(nodeParent.ContentLink);
            var child = GetWithDefaultName<VariationContent>(nodeParent.ContentLink);

            await _fixture.WithContentItems(new[] { nodeChild.ContentLink, child.ContentLink }, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(Constants.ContentApiV2Url + $"/{nodeParent.ContentLink}/children");
                AssertResponse.OK(contentResponse);
                var children = JArray.Parse(await contentResponse.Content.ReadAsStringAsync());

                Assert.Equal(2, children.Count);
                Assert.Equal(nodeChild.Name, (string)children[0]["name"]);
                Assert.Equal(child.Name, (string)children[1]["name"]);
            });
        }
    }
}
