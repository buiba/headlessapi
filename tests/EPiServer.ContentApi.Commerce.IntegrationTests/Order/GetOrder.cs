using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Order
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class GetOrder : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;

        public GetOrder(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetOrder_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, Constants.OrdersApiBaseUrl + "some_order");
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task GetOrder_WhenItDoesNotExist_ShouldReturn404()
        {
            var response = await _fixture.Client.GetAsync(Constants.OrdersApiBaseUrl + "abcd");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetOrder_WhenOrderNumberIsEmpty_ShouldReturnMethodNotAllowed()
        {
            var response = await _fixture.Client.GetAsync(Constants.OrdersApiBaseUrl + string.Empty);

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task GetOrder_WhenExists_ShouldReturnOrder()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 1, 1);

            var (_, savedCart) = await _fixture.PostCartAsync(CreateCart(variant));

            var (_, savedOrder) = await _fixture.ConvertToOrderAsync(savedCart.Id);
            var (response, loadedOrder) = await _fixture.GetOrderAsync(((OrderApiModel) savedOrder).OrderNumber);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            loadedOrder.Should().BeEquivalentTo(savedOrder);
            loadedOrder.Totals.Total.Should().Be(((OrderApiModel) savedOrder).Totals.Total);
        }
    }
}
