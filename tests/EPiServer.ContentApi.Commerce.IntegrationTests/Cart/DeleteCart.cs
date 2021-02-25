using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog.ContentTypes;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Cart
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class DeleteCart : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;
        private readonly CartIdConverter _cartIdConverter;

        public DeleteCart(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
            _cartIdConverter = new CartIdConverter(new Core.Internal.GuidEncoder());
        }

        [Fact]
        public async Task DeleteCart_CartIdArgumentNotValid_ShouldReturn400()
        {
            var response1 = await _fixture.Client.DeleteAsync(Constants.CartsApiBaseUrl + "123");
            var response2 = await _fixture.Client.DeleteAsync(Constants.CartsApiBaseUrl + Guid.Empty);

            Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }

        [Fact]
        public async Task DeleteCart_WhenItDoesNotExist_ShouldReturn404()
        {
            var cartId = _cartIdConverter.ConvertToGuid(666);
            var response = await _fixture.Client.DeleteAsync(Constants.CartsApiBaseUrl + cartId);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCart_WheExist_ShouldReturn200()
        {
            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var deleteResponse = await _fixture.Client.DeleteAsync(Constants.CartsApiBaseUrl + savedCart.Id);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var secondResponse = await _fixture.Client.DeleteAsync(Constants.CartsApiBaseUrl + savedCart.Id);
            Assert.Equal(HttpStatusCode.NotFound, secondResponse.StatusCode);
        }
    }
}