using System;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.InventoryService;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    public class Mapper
    {
        private readonly ShipmentIdConverter _shipmentIdConverter;
        private readonly LineItemIdConverter _lineItemIdConverter;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IRelationRepository _relationRepository;
        private readonly PricingService _pricingService;
        private readonly ShippingMethodService _shippingMethodService;
        private readonly IInventoryService _inventoryService;

        public Mapper(
            ShipmentIdConverter shipmentIdConverter,
            LineItemIdConverter lineItemIdConverter,
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter,
            IOrderGroupCalculator orderGroupCalculator,
            IOrderGroupFactory orderGroupFactory,
            IRelationRepository relationRepository,
            PricingService pricingService,
            ShippingMethodService shippingMethodService,
            IInventoryService inventoryService)
        {
            _shipmentIdConverter = shipmentIdConverter;
            _lineItemIdConverter = lineItemIdConverter;
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _orderGroupCalculator = orderGroupCalculator;
            _orderGroupFactory = orderGroupFactory;
            _relationRepository = relationRepository;
            _pricingService = pricingService;
            _shippingMethodService = shippingMethodService;
            _inventoryService = inventoryService;
        }

        internal TotalsModel MapToTotalsModel(IOrderGroup orderGroup, OrderGroupTotals totals)
        {
            var formTotals = totals[orderGroup.GetFirstForm()];

            var shippingTotals = orderGroup.GetFirstForm().Shipments
                .Select(shipment => new { shipment, shippingTotals = formTotals[shipment] })
                .Select(t => new
                {
                    ShippingTotalsModel =
                        new ShippingTotalsModel
                        {
                            ShipmentId = _shipmentIdConverter.ConvertToGuid(t.shipment.ShipmentId),
                            ItemsTotal = t.shippingTotals.ItemsTotal,
                            ShippingCost = t.shippingTotals.ShippingCost,
                            ShippingTax = t.shippingTotals.ShippingTax,
                            LineItemPrices = t.shipment.LineItems.Select(x => new LineItemPricesModel
                            {
                                LineItemId = _lineItemIdConverter.ConvertToGuid(x.LineItemId),
                                DiscountedPrice = t.shippingTotals[x].DiscountedPrice,
                                ExtendedPrice = t.shippingTotals[x].ExtendedPrice
                            })
                        }
                }).Select(x => x.ShippingTotalsModel);

            return new TotalsModel
            {
                Total = totals.Total,
                SubTotal = totals.SubTotal,
                HandlingTotal = totals.HandlingTotal,
                ShippingTotal = totals.ShippingTotal,
                TaxTotal = totals.TaxTotal,
                DiscountTotal = formTotals.DiscountTotal,
                ShippingTotals = shippingTotals
            };
        }

        internal AddressModel MapToAddressModel(IOrderAddress address)
        {
            if (address == null) return null;

            return new AddressModel
            {
                FirstName = address.FirstName,
                LastName = address.LastName,
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                CountryName = address.CountryName,
                PostalCode = address.PostalCode,
                RegionName = address.RegionName,
                Email = address.Email,
                PhoneNumber = address.DaytimePhoneNumber
            };
        }

        internal IEnumerable<ShippingMethodModel> MapToShippingMethodModel(IOrderGroup order)
        {
            var shipmentAvailableShippingMethods = new List<ShippingMethodModel>();
            foreach (var shipment in order.GetFirstForm().Shipments)
            {
                shipmentAvailableShippingMethods.AddRange(_shippingMethodService.GetShippingMethods(order.MarketId, order.Currency, shipment));
            }

            return shipmentAvailableShippingMethods;
        }

        internal IEnumerable<LineItemValidationModel> MapToValidationIssuesModel(IDictionary<ILineItem, IList<ValidationIssue>> validationIssues)
        {
            return validationIssues.Select(x => new LineItemValidationModel
            {
                ContentId = _contentLoader.Get<EntryContentBase>(_referenceConverter.GetContentLink(x.Key.Code)).ContentGuid,
                Code = x.Key.Code,
                ValidationIssues = x.Value.Select(validationIssue => validationIssue.ToString()),
            }).ToList();
        }

        internal OrderApiModel MapToOrderModel(IPurchaseOrder order)
        {
            return new OrderApiModel
            {
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                Market = order.MarketId.Value,
                Currency = order.Currency.CurrencyCode,
                Shipments = MapToShipmentModel(order.GetFirstForm()),
                Totals = MapToTotalsModel(order, _orderGroupCalculator.GetOrderGroupTotals(order)),
            };
        }


        internal IEnumerable<ShipmentModel> MapToShipmentModel(IOrderForm form)
        {
            return form.Shipments.Select(s =>
                new ShipmentModel
                {
                    LineItems = s.LineItems.Select(li =>
                        new LineItemModel
                        {
                            Id = _lineItemIdConverter.ConvertToGuid(li.LineItemId),
                            ContentId = _contentLoader
                                .Get<EntryContentBase>(_referenceConverter.GetContentLink(li.Code)).ContentGuid,
                            Code = li.Code,
                            Quantity = li.Quantity,
                            DisplayName = li.DisplayName,
                            PlacedPrice = li.PlacedPrice,
                            IsGift = li.IsGift,
                        }),
                    ShippingAddress = MapToAddressModel(s.ShippingAddress),
                    ShippingMethodId = s.ShippingMethodId,
                    Id = _shipmentIdConverter.ConvertToGuid(s.ShipmentId),
                });
        }

        internal Error MapToError(IDictionary<ILineItem, IList<ValidationIssue>> validationIssues)
        {
            var error = ApiErrors.OrderValidationFailed;
            error.Details = validationIssues.Select(x => new ErrorDetails
            {
                Code = x.Value.Count == 1 ? x.Value.Single().ToString() : ApiErrors.LineItemMultipleValidationIssue.Code,
                Message = x.Value.Count == 1 ? x.Value.Single().ToString() : ApiErrors.LineItemMultipleValidationIssue.Message,
                Target = _contentLoader.Get<EntryContentBase>(_referenceConverter.GetContentLink(x.Key.Code)).ContentGuid.ToString(),
                InnerError = x.Value.Count == 1 ? MapToInnerError(x.Key, x.Value.Single()) : null,
                Details = x.Value.Count == 1 ? null : x.Value.Select(issue => new ErrorDetails
                {
                    Code = issue.ToString(),
                    Message = issue.ToString(),
                    InnerError = MapToInnerError(x.Key, issue),
                })
            });

            return error;
        }

        internal dynamic MapToInnerError(ILineItem lineItem, ValidationIssue validationIssue)
        {
            dynamic innerError = new ExpandoObject();

            switch (validationIssue)
            {
                case ValidationIssue.RemovedDueToCodeMissing:
                case ValidationIssue.RemovedDueToNotAvailableInMarket:
                case ValidationIssue.RemovedDueToInactiveWarehouse:
                case ValidationIssue.RemovedDueToMissingInventoryInformation:
                case ValidationIssue.RemovedDueToUnavailableCatalog:
                case ValidationIssue.RemovedDueToUnavailableItem:
                case ValidationIssue.RemovedDueToInsufficientQuantityInInventory:
                case ValidationIssue.RemovedDueToInvalidMaxQuantitySetting:
                case ValidationIssue.RemovedDueToInvalidPrice:
                    innerError.IsRemoved = true;
                    innerError.RequiredAction = $"Remove line item {lineItem.Code} from the cart";
                    break;
                case ValidationIssue.AdjustedQuantityByMinQuantity:
                case ValidationIssue.AdjustedQuantityByMaxQuantity:
                case ValidationIssue.AdjustedQuantityByBackorderQuantity:
                case ValidationIssue.AdjustedQuantityByPreorderQuantity:
                case ValidationIssue.AdjustedQuantityByAvailableQuantity:
                    innerError.AllowedQuantity = lineItem.Quantity;
                    innerError.RequiredAction = $"Set line item quantity for {lineItem.Code} to {lineItem.Quantity}";
                    break;
                case ValidationIssue.PlacedPricedChanged:
                    innerError.NewPrice = lineItem.PlacedPrice;
                    innerError.RequiredAction = $"Set line item price for {lineItem.Code} to {lineItem.PlacedPrice}";
                    break;
                default:
                    innerError.RequiredAction = "Unknown issue";
                    break;
            }

            return innerError;
        }

        internal void UpdateOrderFromModel(IOrderGroup order, OrderInputModel model)
        {
            order.Name = model.Name;
            order.Currency = model.Currency;
            order.CustomerId = model.CustomerId;
            order.MarketId = model.Market;

            UpdateCouponCodes(order, model.CouponCodes);
            UpdateShipments(order, model.Shipments, model.Market, model.Currency);
        }

        internal void UpdateCartFromModel(IOrderGroup cart, CartApiModel model)
        {
            cart.Name = model.Name;
            cart.Currency = model.Currency;
            cart.CustomerId = model.CustomerId;
            cart.MarketId = model.Market;

            UpdateCouponCodes(cart, model.CouponCodes);
            UpdateShipments(cart, model.Shipments, model.Market, model.Currency);
        }

        internal void UpdateShipments(IOrderGroup orderGroup, IEnumerable<ShipmentModel> shipments, string market, string currency)
        {
            orderGroup.GetFirstForm().Shipments.Clear();

            foreach (var shipmentModel in shipments ?? Enumerable.Empty<ShipmentModel>())
            {
                var shipment = orderGroup.CreateShipment(_orderGroupFactory);
                shipment.ShippingMethodId = shipmentModel.ShippingMethodId;
                shipment.ShippingAddress = CreateShippingAddress(orderGroup, shipmentModel.ShippingAddress);
                orderGroup.GetFirstForm().Shipments.Add(shipment);
                AddLineItems(orderGroup, shipment, shipmentModel.LineItems, market, currency);
            }
        }

        internal void UpdateCouponCodes(IOrderGroup orderGroup, IEnumerable<string> couponCodes)
        {
            orderGroup.GetFirstForm().CouponCodes.Clear();

            if (couponCodes != null)
            {
                foreach (var couponCode in couponCodes)
                {
                    orderGroup.GetFirstForm().CouponCodes.Add(couponCode);
                }
            }
        }

        internal void AddLineItems(IOrderGroup orderGroup, IShipment shipment, IEnumerable<LineItemModel> lineItems, string market,
            string currency)
        {
            foreach (var modelLineItem in lineItems)
            {
                var found = _contentLoader.TryGet<EntryContentBase>(modelLineItem.ContentId, out var content);
                if (!found)
                {
                    throw new ApiException(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
                }

                if (content is BundleContent)
                {
                    foreach (var relation in _relationRepository.GetChildren<BundleEntry>(content.ContentLink))
                    {
                        var entry = _contentLoader.Get<EntryContentBase>(relation.Child);
                        AddLineItems(orderGroup, shipment,
                            new[]
                            {
                                new LineItemModel
                                {
                                    ContentId = entry.ContentGuid, Code = entry.Code, Quantity = relation.Quantity ?? 1
                                }
                            }, market, currency);
                    }
                }

                var orderLineItem = orderGroup.CreateLineItem(content.Code);
                orderLineItem.Quantity = modelLineItem.Quantity.Value;
                orderLineItem.DisplayName = content.DisplayName;
                orderLineItem.IsGift = modelLineItem.IsGift;

                var price = _pricingService
                    .GetCatalogEntryPrices(new[] { content.Code }, market, currency)
                    .OrderBy(x => x.UnitPrice.Amount).FirstOrDefault();

                if (price != null)
                {
                    orderLineItem.PlacedPrice = price.UnitPrice;
                }

                orderGroup.AddLineItem(shipment, orderLineItem);
            }
        }

        internal IOrderAddress CreateShippingAddress(IOrderGroup order, AddressModel model)
        {
            if (model == null) return null;

            var shippingAddress = _orderGroupFactory.CreateOrderAddress(order);
            shippingAddress.Id = model.LastName ?? Guid.NewGuid().ToString();
            shippingAddress.FirstName = model.FirstName;
            shippingAddress.LastName = model.LastName;
            shippingAddress.Line1 = model.Line1;
            shippingAddress.Line2 = model.Line2;
            shippingAddress.City = model.City;
            shippingAddress.CountryName = model.CountryName;
            shippingAddress.PostalCode = model.PostalCode;
            shippingAddress.RegionName = model.RegionName;
            shippingAddress.Email = model.Email;
            shippingAddress.DaytimePhoneNumber = model.PhoneNumber;
            return shippingAddress;
        }
    }
}
