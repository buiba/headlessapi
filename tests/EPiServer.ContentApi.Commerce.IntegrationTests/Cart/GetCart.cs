using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using EPiServer.ContentApi.Commerce.Internal.Services;
using System.Linq;
using FluentAssertions;
using EPiServer.ContentApi.Core.Internal;
using System.Net.Http;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Cart
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class GetCart : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;
        private readonly CartIdConverter _cartIdConverter;

        public GetCart(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
            _cartIdConverter = new CartIdConverter(new GuidEncoder());
        }

        [Fact]
        public async Task GetCart_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, Constants.CartsApiBaseUrl + $"{Guid.NewGuid()}");

            var ressponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(ressponse);
        }

        [Fact]
        public async Task GetCart_WhenItDoesNotExist_ShouldReturn404()
        {
            var cartId = _cartIdConverter.ConvertToGuid(666);
            var response = await _fixture.Client.GetAsync(Constants.CartsApiBaseUrl + cartId);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task GetCart_WhenCartIdNotGuid_ShouldReturn404()
        {
            var response = await _fixture.Client.GetAsync(Constants.CartsApiBaseUrl + "123");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCart_WhenIdIsEmpty_ShouldReturn400()
        {
            var response = await _fixture.Client.GetAsync(Constants.CartsApiBaseUrl + Guid.Empty);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCart_WhenExists_ShouldReturnCart()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);

            SavePrice(variant.Code, 1, 1);

            var (_, savedCart) = await _fixture.PostCartAsync(CreateCart(variant));
            var (response, loadedCart) = await _fixture.GetCartAsync(savedCart.Id);
           
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            loadedCart.Should().BeEquivalentTo(savedCart, o => o.Excluding(x => x.LastUpdated)
                .Excluding(x => x.SelectedMemberPath.EndsWith("Id")));
        }
    }
}
