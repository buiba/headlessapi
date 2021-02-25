using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class GetChildren
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private readonly ServiceFixture _fixture;

        public GetChildren(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetChildren_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, V2Uri + $"/some-content-reference/children");

            var contentResponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(contentResponse);
        }

        [Fact]
        public async Task GetChildren_WhenContentReferenceIsInvalid_ShouldThrowBadRequest()
        {          
            var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/invalid-content-reference/children");
            AssertResponse.BadRequest(contentResponse);

            var errorResponse = await contentResponse.Content.ReadAs<ErrorResponse>();
            Assert.Equal(ErrorCode.InvalidParameter, errorResponse.Error.Code);
            Assert.Equal("The content reference is not in a valid format", errorResponse.Error.Message);  
        }

        [Fact]
        public async Task GetChildren_WhenContentHasChildren_ShouldReturnChildren()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
             {
                 var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentGuid}/children");
                 AssertResponse.OK(childrenResponse);
                 var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                 Assert.Single(children);
                 Assert.Equal(child.Name, (string)children[0]["name"]);
             });
        }
        
        [Fact]
        public async Task GetChildren_WhenContentExistButHaveNoChildren_ShouldOkAndEmptyList()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            await _fixture.WithContentItems(new[] { parent }, async () =>
            {
                var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentGuid}/children");
                AssertResponse.OK(childrenResponse);
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Empty(children);
            });
        }

        [Fact]
        public async Task GetChildren_WhenContentDoesNotExist_ShouldReturnNotFound()
        {
            var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{Guid.NewGuid()}/children");
            AssertResponse.NotFound(childrenResponse);
        }

        [Fact]
        public async Task GetChildren_WhenChildExistInLanguageButNotParent_ShouldReturnChild()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            var swedishChild = _fixture.ContentRepository.CreateLanguageBranch<StandardPage>(child.ContentLink, CultureInfo.GetCultureInfo("sv"));
            swedishChild.Name = child.Name + "sv";
            _fixture.ContentRepository.Save(swedishChild, DataAccess.SaveAction.Publish, Security.AccessLevel.NoAccess);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{parent.ContentGuid}/children");
                httpRequest.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("sv"));
                var childrenResponse = await _fixture.Client.SendAsync(httpRequest);
                AssertResponse.OK(childrenResponse);
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                Assert.Equal(swedishChild.Language.Name, (string)children[0]["language"]["name"]);
            });
        }

        [Fact]
        public async Task GetChildren_WhenSeveralChildren_ShouldSupportPaging()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var firstChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            var secondChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            await _fixture.WithContentItems(new[] { parent, firstChild, secondChild }, async () =>
            {
                var retrievedIds = new List<Guid>();
                var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentGuid}/children?top=1");
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string)children[0]["contentLink"]["guidValue"]));

                var pagingToken = childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{parent.ContentGuid}/children");
                httpRequestMessage.Headers.Add(PagingConstants.ContinuationTokenHeaderName, pagingToken);

                childrenResponse = await _fixture.Client.SendAsync(httpRequestMessage);
                children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string)children[0]["contentLink"]["guidValue"]));

                Assert.Contains(firstChild.ContentGuid, retrievedIds);
                Assert.Contains(secondChild.ContentGuid, retrievedIds);
            });
        }

        [Fact]
        public async Task GetChildren_WhenContinuationTokenIsPassedAndtop_ShouldReturnBadRequest()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var firstChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            var secondChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            await _fixture.WithContentItems(new[] { parent, firstChild, secondChild }, async () =>
            {
                var retrievedIds = new List<Guid>();
                var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentGuid}/children?top=1");
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string)children[0]["contentLink"]["guidValue"]));

                var pagingToken = childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{parent.ContentGuid}/children?top=1");
                httpRequestMessage.Headers.Add(PagingConstants.ContinuationTokenHeaderName, pagingToken);

                childrenResponse = await _fixture.Client.SendAsync(httpRequestMessage);
                AssertResponse.StatusCode(HttpStatusCode.BadRequest, childrenResponse);
            });
        }

        [Fact]
        public async Task GetChildren_WhenTopIsLargerThan100_ShouldReturnBadRequest()
        {
            var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{Guid.NewGuid()}/children?top=101");
            AssertResponse.StatusCode(HttpStatusCode.BadRequest, childrenResponse);
        }

        [Fact]
        public async Task GetChildren_WhenRepeatedGetChildrenUsingV2WithContinuationToken_ShouldGetChildren()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var firstChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            var secondChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            var thirdChild = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink, true);
            await _fixture.WithContentItems(new[] { parent, firstChild, secondChild }, async () =>
            {
                var retrievedIds = new List<Guid>();

                //First
                var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink}/children?top=1");
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string)children[0]["contentLink"]["guidValue"]));

                //Second
                var pagingToken = childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{parent.ContentLink}/children");
                httpRequestMessage.Headers.Add(PagingConstants.ContinuationTokenHeaderName, pagingToken);
                childrenResponse = await _fixture.Client.SendAsync(httpRequestMessage);
                children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string)children[0]["contentLink"]["guidValue"]));

                //Third
                pagingToken = childrenResponse.Headers.GetValues(PagingConstants.ContinuationTokenHeaderName);
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, V2Uri + $"/{parent.ContentLink}/children");
                httpRequestMessage.Headers.Add(PagingConstants.ContinuationTokenHeaderName, pagingToken);
                childrenResponse = await _fixture.Client.SendAsync(httpRequestMessage);
                children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                retrievedIds.Add(new Guid((string)children[0]["contentLink"]["guidValue"]));

                Assert.False(childrenResponse.Headers.TryGetValues(PagingConstants.ContinuationTokenHeaderName, out var headervalues));

                Assert.Equal(new[] { thirdChild.ContentGuid, secondChild.ContentGuid, firstChild.ContentGuid }, retrievedIds);
            });
        }

        [Fact]
        public async Task GetChildren_WhenRequestedWithSpecificProperties_ShouldReturnContentWithOnlySelectedProperties()
        {
            var parent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.RootPage, true);
            var child = _fixture.GetWithDefaultName<StandardPage>(parent.ContentLink);
            await _fixture.WithContentItems(new[] { parent, child }, async () =>
            {
                var childrenResponse = await _fixture.Client.GetAsync(V2Uri + $"/{parent.ContentLink}/children?select=name");
                AssertResponse.OK(childrenResponse);
                var children = JArray.Parse(await childrenResponse.Content.ReadAsStringAsync());
                Assert.Single(children);
                Assert.Equal(child.Name, (string)children[0]["name"]);
                Assert.Null(children[0]["url"]);
                Assert.Null(children[0]["category"]);
            });
        }
    }
}
