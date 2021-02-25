using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using System;
using System.Globalization;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using System.Collections.Generic;

namespace EPiServer.ContentApi.IntegrationTests.Commerce
{
    public abstract class CommerceIntegrationTestBase
    {
        protected static ContentReference CatalogContentLink;
        protected const string DefaultMarketId = "DEFAULT";
        protected readonly Currency DefaultCurrency = Currency.USD;

        protected CommerceIntegrationTestBase()
        {
            CatalogContentLink = GetInstance<IContentRepository>().GetChildren<CatalogContent>(GetInstance<ReferenceConverter>().GetRootLink()).First().ContentLink;
        }

        protected T GetWithDefaultName<T>(ContentReference parentLink, Action<T> init = null, string language = "en") where T : CatalogContentBase
        {
            var content = GetInstance<IContentRepository>().GetDefault<T>(parentLink, CultureInfo.GetCultureInfo(language));
            content.ContentGuid = Guid.NewGuid();
            content.Name = content.ContentGuid.ToString("N");

            switch (content)
            {
                case NodeContent node:
                    node.Code = Guid.NewGuid().ToString();
                    break;
                case EntryContentBase entry:
                    entry.Code = Guid.NewGuid().ToString();
                    break;
            }

            init?.Invoke(content);

            GetInstance<IContentRepository>().Save(content, SaveAction.Publish, AccessLevel.NoAccess);

            return content;
        }

        protected void SavePrice(
            string code, 
            decimal quantity, 
            decimal price, 
            string marketId = DefaultMarketId, 
            string currency = "USD")
        {
            var priceDetailValue = new PriceDetailValue
            {
                CatalogKey = new CatalogKey(code),
                MarketId = marketId,
                CustomerPricing = CustomerPricing.AllCustomers,
                MinQuantity = quantity,
                UnitPrice = new Money(price, currency),
                ValidFrom = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddDays(1),
            };
            GetInstance<IPriceDetailService>().Save(priceDetailValue);
        }

        protected CartApiModel CreateCart(EntryContentBase entry, AddressModel addressModel = null)
        {
            return new CartApiModel
            {
                Currency = DefaultCurrency,
                CustomerId = Guid.NewGuid(),
                Market = DefaultMarketId,
                Name = "Default",
                Shipments = new []
                {
                    new ShipmentModel
                    {
                        ShippingMethodId = Guid.Empty,
                        ShippingAddress = addressModel,
                        LineItems = new []
                        {
                            new LineItemModel
                            {
                                ContentId = entry.ContentGuid,
                                Code = entry.Code,
                                Quantity = 1
                            }
                        }
                    }, 
                },
                CouponCodes = Enumerable.Empty<string>()
            };
        }

        protected OrderInputModel CreateOrder(
            EntryContentBase entry,
            decimal quantity = 1)
        {
            return new OrderInputModel
            {
                Currency = DefaultCurrency,
                CustomerId = Guid.NewGuid(),
                Market = DefaultMarketId,
                Name = "Default",
                Shipments = new[]
                {
                    new ShipmentModel
                    {
                        ShippingMethodId = Guid.Empty,
                        ShippingAddress = null,
                        LineItems = new []
                        {
                            new LineItemModel
                            {
                                ContentId = entry.ContentGuid,
                                Code = entry.Code,
                                Quantity = quantity,
                            }
                        }
                    },
                },
                CouponCodes = new[]
                {
                    "CODE01",
                    "CODE02"
                }
            };
        }

        protected static T GetInstance<T>() => ServiceLocator.Current.GetInstance<T>();
    }
}