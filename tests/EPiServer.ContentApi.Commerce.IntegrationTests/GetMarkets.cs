using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal.Models.Markets;
using Mediachase.Commerce;
using Xunit;
using Mediachase.Commerce.Markets;
using System.Net.Http;

namespace EPiServer.ContentApi.IntegrationTests.Commerce
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class GetMarkets : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;

        public GetMarkets(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, Constants.MarketsApiBaseUrl + "some_market");
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.OK(response);

            var getAllRequest = new HttpRequestMessage(HttpMethod.Options, Constants.MarketsApiBaseUrl);
            response = await _fixture.Client.SendAsync(getAllRequest);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task Get_WhenMarketExists_ShouldReturnMarket()
        {
            var market = new SampleMarket();
            GetInstance<IMarketService>().CreateMarket(market);

            var response = await _fixture.Client.GetAsync(Constants.MarketsApiBaseUrl + market.MarketId);
            AssertResponse.OK(response);
            
            var responsePayload = JsonConvert.DeserializeObject<MarketApiModel>(await response.Content.ReadAsStringAsync());

            var expected = new MarketApiModel
            {
                Id = market.MarketId.Value,
                Name = market.MarketName,
                DefaultCurrency = market.DefaultCurrency,
                DefaultLanguage = market.DefaultLanguage,
                Currencies = market.Currencies.Select(x => x.CurrencyCode),
                Countries = market.Countries,
                Languages = market.Languages
            };

            responsePayload.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Get_WhenMarketDoesNotExists_ShouldReturn404()
        {
            var response = await _fixture.Client.GetAsync(Constants.MarketsApiBaseUrl + "unknownmarket");
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task Get_WhenMarketIsInactive_ShouldReturnNotFound()
        {
            var market = new SampleMarket {IsEnabled = false};
            GetInstance<IMarketService>().CreateMarket(market);

            var response = await _fixture.Client.GetAsync(Constants.MarketsApiBaseUrl + market.MarketId);
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEnabledMarkets()
        {
            var disabledMarket = new SampleMarket{ IsEnabled = false};
            GetInstance<IMarketService>().CreateMarket(disabledMarket);
            
            var market = new SampleMarket();
            GetInstance<IMarketService>().CreateMarket(market);

            var response = await _fixture.Client.GetAsync(Constants.MarketsApiBaseUrl);
            AssertResponse.OK(response);

            var responsePayload = JsonConvert.DeserializeObject<IEnumerable<MarketApiModel>>(await response.Content.ReadAsStringAsync()).ToList();

            var expected = new MarketApiModel
            {
                Id = market.MarketId.Value,
                Name = market.MarketName,
                DefaultCurrency = market.DefaultCurrency,
                DefaultLanguage = market.DefaultLanguage,
                Currencies = market.Currencies.Select(x => x.CurrencyCode),
                Countries = market.Countries,
                Languages = market.Languages
            };

            var actual = responsePayload.Single(x => x.Id == market.MarketId.Value);
            actual.Should().BeEquivalentTo(expected);
            responsePayload.Should().NotContain(x => x.Id == disabledMarket.MarketId.Value);
        }

        internal class SampleMarket : IMarket
        {
            public MarketId MarketId { get; set; } = string.Concat(Guid.NewGuid().ToString().Take(8)); //trim to 8 chars because thats the max length
            public bool IsEnabled { get; set; } = true;
            public string MarketName { get; set; } = Guid.NewGuid().ToString();
            public string MarketDescription { get; set; } = "description";
            public CultureInfo DefaultLanguage { get; set; } = CultureInfo.CreateSpecificCulture("en");
            public Currency DefaultCurrency { get; set; } = Currency.USD;
            public IEnumerable<CultureInfo> Languages { get; set; } = new [] {CultureInfo.CreateSpecificCulture("en"), CultureInfo.CreateSpecificCulture("sv")};
            public IEnumerable<Currency> Currencies { get; set; } = new[] { Currency.USD, Currency.SEK };
            public IEnumerable<string> Countries { get; set; } = new[] { "us", "se" };
            public bool PricesIncludeTax { get; set; } = false;
        }
    }
}
