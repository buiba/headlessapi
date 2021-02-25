using System;
using EPiServer.Commerce.Catalog.ContentTypes;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal.Models.Pricing;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Newtonsoft.Json;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class GetPricings : CommerceIntegrationTestBase
    {
        private readonly DateTime _validFrom = DateTime.UtcNow;
        private readonly DateTime _validUntil = DateTime.UtcNow.AddDays(1);
        private readonly CommerceServiceFixture _fixture;
        
        public GetPricings(CommerceServiceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task GetPricings_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, $"{Constants.PricesApiBaseUrl}?contentIds=some_ids&marketId=market&currencyCode=currency");
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task GetPricings_WhenPricesExistsForSingleEntry_ShouldReturnPrices()
        {
            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);
            SavePrice(variant.Code, 0, 1, DefaultMarketId, DefaultCurrency);
            
            var response = await SendRequest(variant.ContentGuid.ToString(), DefaultMarketId, DefaultCurrency);

            AssertResponse.OK(response);

            var responsePayload = JsonConvert.DeserializeObject<List<PricingApiModel>>(await response.Content.ReadAsStringAsync());

            var expectedModel = new PricingApiModel
            {
                EntryCode = variant.Code,
                Prices = new List<PriceModel>
                {
                    new PriceModel
                    {
                        Price = 1,
                        ValidFrom = _validFrom,
                        ValidUntil = _validUntil,
                        PriceType = "AllCustomers",
                        PriceCode = ""
                    }
                },
                DiscountedPrices = Enumerable.Empty<DiscountedPriceModel>()
            };

            responsePayload.Should().BeEquivalentTo(new[] { expectedModel }, o =>
                o.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>());
        }

        [Fact]
        public async Task GetPricings_WhenEntryDoesNotExist_ShouldReturnEmptyList()
        {
            var response = await SendRequest(Guid.NewGuid().ToString(), DefaultMarketId, DefaultCurrency);

            AssertResponse.OK(response);
            var priceModels = JsonConvert.DeserializeObject<List<PricingApiModel>>(await response.Content.ReadAsStringAsync());
            Assert.Empty(priceModels);
        }

        [Fact]
        public async Task GetPricings_WhenMarketDoesNotExist_ShouldReturnEmptyList()
        {
            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);
            SavePrice(variant.Code, 0, 1, DefaultMarketId, DefaultCurrency);

            var response = await SendRequest(Guid.NewGuid().ToString(), "UNKNOWNMARKET", DefaultCurrency);

            AssertResponse.OK(response);
            var priceModels = JsonConvert.DeserializeObject<List<PricingApiModel>>(await response.Content.ReadAsStringAsync());
            Assert.Empty(priceModels);
        }

        [Fact]
        public async Task GetPricings_WhenPricesExistsForMultipleEntries_ShouldReturnMultiplePrices()
        {
            var variant1 = GetWithDefaultName<VariationContent>(CatalogContentLink);
            var variant2 = GetWithDefaultName<VariationContent>(CatalogContentLink);
            SavePrice(variant1.Code, 0, 1, DefaultMarketId, DefaultCurrency);
            SavePrice(variant2.Code, 0, 2, DefaultMarketId, DefaultCurrency);
           
            var response = await SendRequest($"{variant1.ContentGuid},{variant2.ContentGuid}", DefaultMarketId, DefaultCurrency);
            AssertResponse.OK(response);

            var responsePayload = JsonConvert.DeserializeObject<List<PricingApiModel>>(await response.Content.ReadAsStringAsync());

            var expected = new[]
            {
                new PricingApiModel
                {
                    EntryCode = variant1.Code,
                    Prices = new List<PriceModel>
                    {
                        new PriceModel
                        {
                            Price = 1,
                            ValidFrom = _validFrom,
                            ValidUntil = _validUntil,
                            PriceType = "AllCustomers",
                            PriceCode = ""
                        }
                    },
                    DiscountedPrices = Enumerable.Empty<DiscountedPriceModel>()
                },
                new PricingApiModel
                {
                    EntryCode = variant2.Code,
                    Prices = new List<PriceModel>
                    {
                        new PriceModel
                        {
                            Price = 2,
                            ValidFrom = _validFrom,
                            ValidUntil = _validUntil,
                            PriceType = "AllCustomers",
                            PriceCode = ""
                        }
                    },
                    DiscountedPrices = Enumerable.Empty<DiscountedPriceModel>()
                }
            };

            responsePayload.Should().BeEquivalentTo(expected, o =>
                o.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>());
        }

        [Fact]
        public async void GetPricings_WhenEntryHasNoPrices_ShouldReturnEmptyPrices()
        {
            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);

            var response = await SendRequest(variant.ContentGuid.ToString(), DefaultMarketId, DefaultCurrency);
            AssertResponse.OK(response);
            
            var responsePayload = JsonConvert.DeserializeObject<List<PricingApiModel>>(await response.Content.ReadAsStringAsync());
            var expectedModel = new PricingApiModel
            {
                EntryCode = variant.Code,
                Prices = Enumerable.Empty<PriceModel>(),
                DiscountedPrices = Enumerable.Empty<DiscountedPriceModel>()
            };

            responsePayload.Should().BeEquivalentTo(new[] { expectedModel }, o =>
                o.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>());
        }

        [Fact]
        public async void WhenPricesExistInMultipleCurrencies_ShouldOnlyReturnPricesInSpecifiedCurrency()
        {
            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);
            SavePrice(variant.Code, 0, 1, DefaultMarketId, DefaultCurrency);
            SavePrice(variant.Code, 0, 2, DefaultMarketId, Currency.SEK);

            var response = await SendRequest(variant.ContentGuid.ToString(), DefaultMarketId, Currency.SEK);
            AssertResponse.OK(response);

            var responsePayload = JsonConvert.DeserializeObject<List<PricingApiModel>>(await response.Content.ReadAsStringAsync());
            var expectedModel = new PricingApiModel
            {
                EntryCode = variant.Code,
                Prices = new List<PriceModel>
                {
                    new PriceModel
                    {
                        Price = 2,
                        ValidFrom = _validFrom,
                        ValidUntil = _validUntil,
                        PriceType = "AllCustomers",
                        PriceCode = ""
                    }
                },
                DiscountedPrices = Enumerable.Empty<DiscountedPriceModel>()
            };

            responsePayload.Should().BeEquivalentTo(new[] { expectedModel }, o =>
                o.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>());
        }

        [Fact]
        public async void WhenPricesExistInMultipleMarkets_ShouldOnlyReturnPricesInSpecifiedMarket()
        {
            var australiaMarket = new AustraliaMarket();
            GetInstance<IMarketService>().CreateMarket(australiaMarket);
            GetInstance<IMarketService>().CreateMarket(new NorwayMarket());

            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);
            SavePrice(variant.Code, 0, 1, australiaMarket.MarketId.Value, australiaMarket.DefaultCurrency);
            SavePrice(variant.Code, 0, 2, "NOR", "NOK");

            var response = await SendRequest(variant.ContentGuid.ToString(), australiaMarket.MarketId.Value, Currency.AUD);
            AssertResponse.OK(response);

            var responsePayload = JsonConvert.DeserializeObject<List<PricingApiModel>>(await response.Content.ReadAsStringAsync());
            var expectedModel = new PricingApiModel
            {
                EntryCode = variant.Code,
                Prices = new List<PriceModel>
                {
                    new PriceModel
                    {
                        Price = 1,
                        ValidFrom = _validFrom,
                        ValidUntil = _validUntil,
                        PriceType = "AllCustomers",
                        PriceCode = ""
                    }
                },
                DiscountedPrices = Enumerable.Empty<DiscountedPriceModel>()
            };

            responsePayload.Should().BeEquivalentTo(new[] { expectedModel }, o =>
                o.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>());
        }

        async Task<HttpResponseMessage> SendRequest(string ids, string market, string currency) =>
            await _fixture.Client.GetAsync($"{Constants.PricesApiBaseUrl}?contentIds={ids}&marketId={market}&currencyCode={currency}");
    }

    internal class AustraliaMarket : IMarket
    {
        public MarketId MarketId { get; } = "AUS";
        public bool IsEnabled { get; } = true;
        public string MarketName { get; } = "Australia";
        public string MarketDescription { get; } = "Test Australia market";
        public CultureInfo DefaultLanguage { get; } = CultureInfo.CreateSpecificCulture("en");
        public IEnumerable<CultureInfo> Languages { get; } = new List<CultureInfo>
        {
            CultureInfo.CreateSpecificCulture("en")
        };
        public Currency DefaultCurrency { get; } = Currency.AUD;
        public IEnumerable<Currency> Currencies { get; } = new[] { Currency.AUD };
        public IEnumerable<string> Countries { get; } = new[] { "aus" };
        public bool PricesIncludeTax { get; } = false;
    }

    internal class NorwayMarket : IMarket
    {
        public MarketId MarketId { get; } = "NOR";
        public bool IsEnabled { get; } = true;
        public string MarketName { get; } = "Norway";
        public string MarketDescription { get; } = "Test Norway market";
        public CultureInfo DefaultLanguage { get; } = CultureInfo.CreateSpecificCulture("en");
        public IEnumerable<CultureInfo> Languages { get; } = new List<CultureInfo>
        {
            CultureInfo.CreateSpecificCulture("en")
        };
        public Currency DefaultCurrency { get; } = Currency.NOK;
        public IEnumerable<Currency> Currencies { get; } = new[] { Currency.NOK };
        public IEnumerable<string> Countries { get; } = new[] { "Nor" };
        public bool PricesIncludeTax { get; } = false;
    }
}
