using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Commerce.Internal.Models.Markets;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    [ServiceConfiguration(typeof(MarketsService))]
    public class MarketsService
    {
        private readonly IMarketService _marketService;

        public MarketsService(IMarketService marketService)
        {
            _marketService = marketService;
        }

        internal MarketApiModel Get(string id)
        {
            var market = _marketService.GetMarket(new MarketId(id));
            if (market is null || !market.IsEnabled) return null;

            return Convert(market);
        }

        internal IEnumerable<MarketApiModel> GetAll()
        {
            return _marketService.GetAllMarkets().Where(x => x.IsEnabled).Select(Convert);
        }

        private MarketApiModel Convert(IMarket market)
        {
            return new MarketApiModel
            {
                Id = market.MarketId.Value,
                Name = market.MarketName,
                DefaultCurrency = market.DefaultCurrency,
                DefaultLanguage = market.DefaultLanguage,
                Currencies = market.Currencies.Select(x => x.CurrencyCode),
                Languages = market.Languages,
                Countries = market.Countries
            };
        }
    }
}
