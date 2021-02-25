using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Cart
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class ConvertToOrder : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;

        public ConvertToOrder(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ConvertToOrder_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, Constants.CartsApiBaseUrl + $"{Guid.NewGuid()}/ConvertToOrder");

            var ressponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(ressponse);
        }

        [Fact]
        public async Task ConvertToOrder_WhenEmptyCartId_ShouldReturnBadRequest()
        {
            var (response, _) = await _fixture.ConvertToOrderAsync(Guid.Empty);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ConvertToOrder_WhenCartExists_ShouldConvertToOrder()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
               entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 0, 1);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (response, order) = await _fixture.ConvertToOrderAsync(savedCart.Id);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(response.Headers.Location.OriginalString.Replace("http://localhost/", ""),
                Constants.OrdersApiBaseUrl + ((OrderApiModel)order).OrderNumber);
        }

        [Fact]
        public async Task ConvertToOrder_WhenSuccessfull_NewOrderIsReachable()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
               entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 0, 1);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (response, _) = await _fixture.ConvertToOrderAsync(savedCart.Id);
            var newCreatedOrder = await _fixture.Client.GetAsync(response.Headers.Location.ToString());
            newCreatedOrder.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ConvertToOrder_WhenInvalidCart_ShouldReturnValidationErrors_LineItemRemoved()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);

            SavePrice(variant.Code, 0, 1);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (response, errors) = await _fixture.ConvertToOrderAsync(savedCart.Id);
            var errorDetails = ((Error) errors).Details;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(errorDetails);
            Assert.True((bool)(errorDetails.FirstOrDefault().InnerError as dynamic).isRemoved);
        }

        [Fact]
        public async Task ConvertToOrder_WhenInvalidCart_ShouldReturnValidationErrors_AdjustedQuantity()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 2;
            });

            SavePrice(variant.Code, 0, 1);
            var cartModel = CreateCart(variant);
            cartModel.Shipments.Single().LineItems.Single().Quantity = 3;

            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (response, errors) = await _fixture.ConvertToOrderAsync(savedCart.Id);
            var errorDetails = ((Error)errors).Details;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(errorDetails);
            Assert.Equal((decimal?)(errorDetails.FirstOrDefault().InnerError as dynamic).allowedQuantity, variant.MaxQuantity);
        }

        [Fact]
        public async Task ConvertToOrder_WhenSuccessfull_ShouldReturnShippingAddress()
        {
            var variant =
                GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry => { entry.MaxQuantity = 10; });

            SavePrice(variant.Code, 0, 1);
            var shippingAddress = new AddressModel()
                {FirstName = "aa", LastName = "bb", City = "cc", CountryName = "dd"};
            var cartModel = CreateCart(variant, shippingAddress);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (response, savedOrder) = await _fixture.ConvertToOrderAsync(savedCart.Id);
            (savedOrder as OrderApiModel).Shipments.First().ShippingAddress.Should().BeEquivalentTo(shippingAddress,
                options =>
                    options.Using<string>(ctx =>
                            (ctx.Subject ?? string.Empty).Should().BeEquivalentTo(ctx.Expectation ?? string.Empty))
                        .WhenTypeIs<string>());
        }
    }
}
