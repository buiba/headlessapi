using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using FluentAssertions;
using System.Linq;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Commerce.Internal.Services;
using System.Net.Http;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Cart
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class Checkout : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;
        private readonly CartIdConverter _cartIdConverter;

        public Checkout(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
            _cartIdConverter = new CartIdConverter(new Core.Internal.GuidEncoder());
        }

        [Fact]
        public async Task PrepareCheckoutAsync_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, Constants.CartsApiBaseUrl + $"{Guid.NewGuid()}/preparecheckout");

            var ressponse = await _fixture.Client.SendAsync(request);
            AssertResponse.OK(ressponse);
        }

        [Fact]
        public async Task PrepareCheckoutAsync_WhenCartIdIsEmpty_ShouldReturn400()
        {
            var (checkoutResponse, _) = await _fixture.PrepareCheckoutAsync(Guid.Empty);

            Assert.Equal(HttpStatusCode.BadRequest, checkoutResponse.StatusCode);
        }

        [Fact]
        public async Task PrepareCheckoutAsync_WhenCartIdIsGuidButInvalid_ShouldReturn400()
        {
            var cartId = Guid.NewGuid();
            var (checkoutResponse, _) = await _fixture.PrepareCheckoutAsync(cartId);

            Assert.Equal(HttpStatusCode.BadRequest, checkoutResponse.StatusCode);
        }

        [Fact]
        public async Task PrepareCheckoutAsync_WhenCartIdIsNotGuid_ShouldReturn404()
        {
            var cartId = "123";
            var (checkoutResponse, _) = await _fixture.PrepareCheckoutAsync(cartId);

            Assert.Equal(HttpStatusCode.NotFound, checkoutResponse.StatusCode);
        }

        [Fact]
        public async Task PrepareCheckoutAsync_WhenCartDoesNotExist_ShouldReturn404()
        {
            var cartId = _cartIdConverter.ConvertToGuid(666);
            var (checkoutResponse, _) = await _fixture.PrepareCheckoutAsync(cartId);

            Assert.Equal(HttpStatusCode.NotFound, checkoutResponse.StatusCode);
        }

        [Fact]
        public async Task PrepareCheckoutAsync_ShouldReturnCart()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 1, 1);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (checkoutResponse, checkoutData) = await _fixture.PrepareCheckoutAsync(savedCart.Id);

            checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            checkoutData.Cart.Should().BeEquivalentTo(savedCart, o => o
                .Excluding(x => x.LastUpdated)
                .Excluding(x => x.SelectedMemberPath.EndsWith("Id")));

            checkoutData.ValidationIssues.Should().BeEmpty();
            checkoutData.Totals.Total.Should().Be(1);
            checkoutData.AvailableShippingMethods.Should().NotBeEmpty();
        }

        [Fact]
        public async Task PrepareCheckoutAsync_WhenCartContainsInvalidItems_ShouldReturnValidationIssues()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);

            SavePrice(variant.Code, 1, 1);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (checkoutResponse, checkoutData) = await _fixture.PrepareCheckoutAsync(savedCart.Id);

            Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);
            checkoutData.ValidationIssues.Should().HaveCount(1);
        }

        [Fact]
        public async Task PrepareCheckoutAsync_ShouldReturnAvailableMethods()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);

            SavePrice(variant.Code, 1, 1);
            var cartModel = CreateCart(variant);

            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (checkoutResponse, checkoutData) = await _fixture.PrepareCheckoutAsync(savedCart.Id);

            Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);

            checkoutData.AvailableShippingMethods.Where(x => x.ShipmentId == checkoutData.Cart.Shipments.Single().Id).Should().HaveCount(3);
        }

        [Fact]
        public async Task PrepareCheckoutAsync_WithUpdatedShippingDetails_ShouldReturnUpdatedState()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 1, 1);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (_, checkoutData) = await _fixture.PrepareCheckoutAsync(savedCart.Id);

            checkoutData.Cart.Shipments.Single().ShippingMethodId = Guid.NewGuid();
            checkoutData.Cart.Shipments.Single().ShippingAddress = CreateShippingAddress();

            await _fixture.PutCartAsync(checkoutData.Cart.Id.Value, checkoutData.Cart);
            var (_, updatedCheckoutData) = await _fixture.PrepareCheckoutAsync(savedCart.Id);

            updatedCheckoutData.Cart.Should().BeEquivalentTo(checkoutData.Cart, options => options
                .Excluding(x => x.SelectedMemberPath.EndsWith("Id"))
                .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTimeOffset>());
        }

        [Fact]
        public async Task PrepareCheckoutAsync_InvalidCouponCodes_ShouldBeRemovedFromCart()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 1, 1);
            var cartModel = CreateCart(variant);
            cartModel.CouponCodes = new[] { "CODE1" };

            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (checkoutResponse, checkoutData) = await _fixture.PrepareCheckoutAsync(savedCart.Id);

            Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);
            Assert.DoesNotContain(checkoutData.Cart.CouponCodes, c => c.Equals("CODE1", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task PrepareCheckoutAsync_ShouldReturnTotals()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink, entry =>
            {
                entry.MaxQuantity = 10;
            });

            SavePrice(variant.Code, 1, 1);
            var cartModel = CreateCart(variant);

            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (checkoutResponse, checkoutData) = await _fixture.PrepareCheckoutAsync(savedCart.Id);

            Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);

            var expectation = new TotalsModel
            {
                DiscountTotal = 0,
                HandlingTotal = 0,
                ShippingTotal = 0,
                TaxTotal = 0,
                SubTotal = 1,
                Total = 1,
                ShippingTotals = new[]
                {
                    new ShippingTotalsModel
                    {
                        ItemsTotal = 1,
                        ShippingCost = 0,
                        ShippingTax = 0,
                        ShipmentId = checkoutData.Cart.Shipments.Single().Id,
                        LineItemPrices = new[]
                        {
                            new LineItemPricesModel
                            {
                                DiscountedPrice = 1,
                                ExtendedPrice = 1,
                                LineItemId = checkoutData.Cart.Shipments.Single().LineItems.Single().Id
                            }
                        }
                    }
                }
            };

            checkoutData.Totals.Should().BeEquivalentTo(expectation);
        }

        private AddressModel CreateShippingAddress()
        {
            return new AddressModel
            {
                FirstName = "FName",
                LastName = "LName",
                Line1 = "Line1",
                Line2 = "Line2",
                City = "City",
                CountryName = "USA",
                PostalCode = "1234",
                RegionName = "NY",
                Email = "example@email.com",
                PhoneNumber = "123456789"
            };
        }
    }
}
