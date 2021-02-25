using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Marketing;
using EPiServer.ContentApi.Commerce.Internal.Models.Pricing;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    /// <summary>
    /// Service to retrieve sku prices.
    /// </summary>
    [ServiceConfiguration(typeof(PricingService))]
    public class PricingService
    {
        private readonly IPriceService _priceService;
        private readonly PromotionService _promotionService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IMarketService _marketService;
        private readonly ContentApiAuthorizationService _authorizationService;

        //For tests
        protected PricingService() { }

        public PricingService(
            IPriceService priceService,
            PromotionService promotionService,
            ReferenceConverter referenceConverter,
            IMarketService marketService,
            ContentApiAuthorizationService authorizationService)
        {
            _priceService = priceService;
            _promotionService = promotionService;
            _referenceConverter = referenceConverter;
            _marketService = marketService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Get catalog entry prices by given array of entry codes, marketId and currency. 
        /// </summary>      
        public virtual IEnumerable<IPriceValue> GetCatalogEntryPrices(IEnumerable<string> catalogEntryCodes, MarketId marketId, Currency currency)
        {
            return _priceService.GetPrices(marketId, DateTime.Now,
                        catalogEntryCodes.Select(ec => new CatalogKey(ec)),
                        new PriceFilter { Currencies = new[] { currency } });
        }

        /// <summary>
        /// Get pricing information by given array of entry codes, marketId and currencyCode.
        /// </summary>        
        public virtual IEnumerable<PricingApiModel> GetPricings(IEnumerable<string> catalogEntryCodes, string marketId, string currencyCode)
        {
            var market = _marketService.GetMarket(marketId);
            if (market == null)
            {
                return Enumerable.Empty<PricingApiModel>();
            }

            var entryPrices = GetCatalogEntryPrices(catalogEntryCodes, new MarketId(marketId), new Currency(currencyCode));

            return catalogEntryCodes
                .Select(x => new { EntryCode = x, ContentReference = _referenceConverter.GetContentLink(x) })
                .Where(x => _authorizationService.CanUserAccessContent(x.ContentReference))
                .Select(x => new PricingApiModel
                {
                    EntryCode = x.EntryCode,
                    Prices = entryPrices.Where(e => e.CatalogKey.CatalogEntryCode.Equals(x.EntryCode, StringComparison.OrdinalIgnoreCase))
                                .Select(ConvertToPriceValueModel),
                    DiscountedPrices = _promotionService.GetDiscountPrices(x.ContentReference, market, currencyCode)
                                        .SelectMany(dp => dp.DiscountPrices)
                                          .Select(ConvertToDiscountedPriceModel)
                });
        }

        private PriceModel ConvertToPriceValueModel(IPriceValue priceValue) =>
            new PriceModel
            {
                MinQuantity = priceValue.MinQuantity,
                Price = priceValue.UnitPrice,
                ValidFrom = priceValue.ValidFrom,
                ValidUntil = priceValue.ValidUntil,
                PriceType = priceValue.CustomerPricing.PriceTypeId.ToString(),
                PriceCode = priceValue.CustomerPricing.PriceCode
            };
     
        private DiscountedPriceModel ConvertToDiscountedPriceModel(DiscountPrice discountPrice) =>
            new DiscountedPriceModel
            {
                DefaultPrice = discountPrice.DefaultPrice,
                DiscountedPrice = discountPrice.Price,
                Description = discountPrice.Promotion.Description
            };
    }
}
