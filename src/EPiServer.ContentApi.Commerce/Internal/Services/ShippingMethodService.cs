using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    [ServiceConfiguration(typeof(ShippingMethodService))]
    public class ShippingMethodService
    {
        private readonly IMarketService _marketService;
        private readonly ShipmentIdConverter _shipmentIdConverter;
        private readonly ServiceCollectionAccessor<IShippingPlugin> _shippingPluginsAccessor;
        private readonly ServiceCollectionAccessor<IShippingGateway> _shippingGatewaysAccessor;

        public ShippingMethodService(
            IMarketService marketService,
            ShipmentIdConverter shipmentIdConverter,
            ServiceCollectionAccessor<IShippingPlugin> shippingPluginsAccessor, 
            ServiceCollectionAccessor<IShippingGateway> shippingGatewaysAccessor)
        {
            _marketService = marketService;
            _shipmentIdConverter = shipmentIdConverter;
            _shippingPluginsAccessor = shippingPluginsAccessor;
            _shippingGatewaysAccessor = shippingGatewaysAccessor;
        }

        internal IEnumerable<ShippingMethodModel> GetShippingMethods(MarketId marketId, Currency currency, IShipment shipment)
        {
            var market = _marketService.GetMarket(marketId);
            var shippingRates = GetShippingRates(market, currency, shipment);
            return shippingRates.Select(r => new ShippingMethodModel
            {
                ShipmentId = _shipmentIdConverter.ConvertToGuid(shipment.ShipmentId), 
                Id = r.Id, 
                DisplayName = r.Name, 
                Price = r.Money
            });
        }

        private IEnumerable<ShippingRate> GetShippingRates(IMarket market, Currency currency, IShipment shipment)
        {
            var methods = GetShippingMethodsByMarket(market.MarketId.Value);
            var currentLanguage = market.DefaultLanguage.TwoLetterISOLanguageName;

            return methods.Where(shippingMethodRow => currentLanguage.Equals(shippingMethodRow.LanguageId, StringComparison.OrdinalIgnoreCase)
                                                      && string.Equals(currency, shippingMethodRow.Currency, StringComparison.OrdinalIgnoreCase))
                .OrderBy(shippingMethodRow => shippingMethodRow.Ordering)
                .Select(shippingMethodRow => GetRate(shipment, shippingMethodRow, market))
                .Where(rate => rate != null);
        }

        private IEnumerable<ShippingMethodInfoModel> GetShippingMethodsByMarket(string marketId)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(marketId, false);
            return methods.ShippingMethod.Select(method => new ShippingMethodInfoModel
            {
                MethodId = method.ShippingMethodId,
                Currency = method.Currency,
                LanguageId = method.LanguageId,
                Ordering = method.Ordering,
                ClassName = methods.ShippingOption.FindByShippingOptionId(method.ShippingOptionId).ClassName
            });
        }

        private ShippingRate GetRate(IShipment shipment, ShippingMethodInfoModel shippingMethodInfoModel, IMarket currentMarket)
        {
            var type = Type.GetType(shippingMethodInfoModel.ClassName);
            if (type == null)
            {
                throw new TypeInitializationException(shippingMethodInfoModel.ClassName, null);
            }
            string message = string.Empty;
            var shippingPlugin = _shippingPluginsAccessor().FirstOrDefault(s => s.GetType() == type);
            if (shippingPlugin != null)
            {
                return shippingPlugin.GetRate(currentMarket, shippingMethodInfoModel.MethodId, shipment, ref message);
            }

            var shippingGateway = _shippingGatewaysAccessor().FirstOrDefault(s => s.GetType() == type);
            if (shippingGateway != null)
            {
                return shippingGateway.GetRate(currentMarket, shippingMethodInfoModel.MethodId, (Shipment)shipment, ref message);
            }
            throw new InvalidOperationException($"There is no registered {nameof(IShippingPlugin)} or {nameof(IShippingGateway)} instance.");
        }

        class ShippingMethodInfoModel
        {
            public Guid MethodId { get; set; }
            public string ClassName { get; set; }
            public string LanguageId { get; set; }
            public string Currency { get; set; }
            public int Ordering { get; set; }
        }
    }
}
