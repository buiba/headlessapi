using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Order
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class PostOrder : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;

        public PostOrder(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PostOrder_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, Constants.OrdersApiBaseUrl);
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task PostOrder_WithNoPayload_ShouldReturn400()
        {
            var (response, _) = await _fixture.PostOrderAsync<OrderApiModel>(null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostOrder_WithInvalidPayload_ShouldReturn400()
        {
            
            var orderInputModel = CreateOrder(new SampleVariationContent { Code = "skuCode", ContentGuid = Guid.NewGuid() });
            orderInputModel.CustomerId = Guid.Empty;

            var (response, _) = await _fixture.PostOrderAsync<OrderApiModel>(orderInputModel);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostOrder_WhenMarketIsInvalid_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            var orderInputModel = CreateOrder(variant);
            orderInputModel.Market = "unknownmarket";

            var (response, _) = await _fixture.PostOrderAsync<OrderApiModel>(orderInputModel);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostOrder_WhenOrderWithSameInputValuesHasBeenCreated_ShouldReturn200AndCreateNewOrder()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            var orderInputModel = CreateOrder(variant);

            var (_, savedPurchaseOrder) = await _fixture.PostOrderAsync<OrderApiModel>(orderInputModel);

            var (secondResponse, secondSavedOrder) = await _fixture.PostOrderAsync<OrderApiModel>(orderInputModel);

            Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
            Assert.NotEqual(secondSavedOrder.OrderNumber, savedPurchaseOrder.OrderNumber);
        }

        [Fact]
        public async Task PostOrder_ShouldReturnLocationHeader()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            var orderInputModel = CreateOrder(variant);

            var (response, savedPurchaseOrder) = await _fixture.PostOrderAsync<OrderApiModel>(orderInputModel);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(response.Headers.Location.OriginalString.Replace("http://localhost/", ""),
                Constants.OrdersApiBaseUrl + savedPurchaseOrder.OrderNumber);
        }

        [Fact]
        public async Task PostOrder_ShouldReturnOrder()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 1, 1);

            var orderInputModel = CreateOrder(variant);

            var (response, savedOrder) = await _fixture.PostOrderAsync<OrderApiModel>(orderInputModel);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var expectation = new OrderApiModel
            {
                CustomerId = orderInputModel.CustomerId,
                Currency = orderInputModel.Currency,
                Market = orderInputModel.Market,
                Shipments = orderInputModel.Shipments,
            };

            savedOrder.Should().BeEquivalentTo(expectation, options => options
                .Excluding(x => x.OrderNumber)
                .Excluding(x => x.Totals)
                .Excluding(x => x.SelectedMemberPath.EndsWith("Id"))
                .Excluding(x => x.SelectedMemberPath.EndsWith("PlacedPrice"))
                .Excluding(x => x.SelectedMemberPath.EndsWith("DisplayName")));
        }

        [Fact]
        public async Task PostOrder_WithMaxQuantityNotValid_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 1;
            });
            
            var orderInputModel = CreateOrder(variant, 2);

            var (response, error) = await _fixture.PostOrderAsync<ErrorResponse>(orderInputModel);
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(error.Error.Details);
        }
    }
}
