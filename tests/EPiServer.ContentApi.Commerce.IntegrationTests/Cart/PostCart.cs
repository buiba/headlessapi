using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using FluentAssertions;
using System.Linq;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using System.Net.Http;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Cart
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class PostCart : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;

        public PostCart(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PostCart_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, Constants.CartsApiBaseUrl);

            var ressponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(ressponse);
        }

        [Fact]
        public async Task PostCart_WithNoPayload_ShouldReturn400()
        {
            var (response, _) = await _fixture.PostCartAsync(null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostCart_WithInvalidPayload_ShouldReturn400()
        {
            var cartModel = CreateCart(new SampleVariationContent { Code = "skuCode", ContentGuid = Guid.NewGuid() });
            cartModel.CustomerId = Guid.Empty;

            var (response, _) = await _fixture.PostCartAsync(cartModel);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostCart_WhenCartDoesNotExist_ShouldReturn200()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
         
            SavePrice(variant.Code, 0, 1);
            var cartModel = CreateCart(variant);

            var (response, savedCart) = await _fixture.PostCartAsync(cartModel);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            savedCart.Should().BeEquivalentTo(cartModel, options => options
               .Excluding(x => x.LastUpdated)
               .Excluding(x => x.Id)
               .Excluding(x => x.Shipments));
            var savedLineItem = savedCart.Shipments.Single().LineItems.Single();
            savedLineItem.Should().BeEquivalentTo(new LineItemModel
            {
                ContentId = variant.ContentGuid,
                Code = variant.Code,
                Quantity = 1,
                PlacedPrice = 1,
                DisplayName = variant.DisplayName
            }, options => options
                .Excluding(x => x.Id));
        }

        [Fact]
        public async Task PostCart_WhenMarketIsInvalid_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);

            var cartModel = CreateCart(variant);
            cartModel.Market = "unknownmarket";

            var (response, _) = await _fixture.PostCartAsync(cartModel);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostCart_ShouldIgnoreLineItemPropertiesThatAreReadonly()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            SavePrice(variant.Code, 0, 1);

            cartModel.Shipments.Single().LineItems.Single().DisplayName = "other";
            cartModel.Shipments.Single().LineItems.Single().PlacedPrice = 666;
            cartModel.Shipments.Single().LineItems.Single().Code = "unknownskucode";
            var (response, savedCart) = await _fixture.PostCartAsync(cartModel);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedLineItem = new LineItemModel
            {
                Code = variant.Code,
                ContentId = variant.ContentGuid,
                DisplayName = variant.DisplayName,
                PlacedPrice = 1,
                Quantity = 1
            };
            savedCart.Shipments.Single().LineItems.Single().Should()
                .BeEquivalentTo(expectedLineItem, o => o.Excluding(x => x.Id));
        }

        [Fact]
        public async Task PostCart_WhenCartWithSameIdExists_ShouldReturn200AndCreateNewCart()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);

            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);
           
            savedCart.CustomerId = Guid.NewGuid(); //change  random property

            var (secondResponse, secondSavedCart) = await _fixture.PostCartAsync(savedCart);

            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.NotEqual(secondSavedCart.Id, savedCart.Id);
        }

        [Fact]
        public async Task PostCart_WithSameCustomerIdAndMarketAndName_ShouldReturn500()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);

            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);
           
            var (secondResponse, _) = await _fixture.PostCartAsync(savedCart);
            Assert.Equal(HttpStatusCode.InternalServerError, secondResponse.StatusCode);
        }

        [Fact]
        public async Task PostCart_WhenContentIdIsInvalid_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            variant.ContentGuid = Guid.NewGuid();
            var cartModel = CreateCart(variant);
            var (response, _) = await _fixture.PostCartAsync(cartModel);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
