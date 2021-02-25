using EPiServer.Commerce.Order;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ServiceLocation;
using System;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using Mediachase.Commerce.Markets;
using System.Collections.Generic;
using EPiServer.Commerce.Marketing;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    /// <summary>
    /// Service to manage cart
    /// </summary>
    [ServiceConfiguration(typeof(CartService))]
    public class CartService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly CartIdConverter _cartIdConverter;
        private readonly IMarketService _marketService;
        private readonly OrderValidationService _orderValidationService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ICartProvider _cartProvider;
        private readonly Mapper _mapper;

        public CartService(
            IOrderRepository orderRepository,
            CartIdConverter cartIdConverter,
            IMarketService marketService,
            OrderValidationService orderValidationService,
            IOrderGroupCalculator orderGroupCalculator,
            ICartProvider cartProvider,
            Mapper mapper)
        {
            _orderRepository = orderRepository;
            _cartIdConverter = cartIdConverter;
            _marketService = marketService;
            _orderValidationService = orderValidationService;
            _orderGroupCalculator = orderGroupCalculator;
            _cartProvider = cartProvider;
            _mapper = mapper;
        }

        internal CartApiModel Get(Guid cartId)
        {
            if (!_cartIdConverter.TryConvertToInt(cartId, out int cartOrderGroupId))
            {
                throw new ApiException(ApiErrors.CartNotFound, HttpStatusCode.BadRequest);
            }

            var cart = _orderRepository.Load<ICart>(cartOrderGroupId);
            if (cart == null)
            {
                throw new ApiException(ApiErrors.CartNotFound, HttpStatusCode.NotFound);
            }

            return MapToCartModel(cart);
        }

        internal void Delete(Guid cartId)
        {
            if (!_cartIdConverter.TryConvertToInt(cartId, out int cartOrderGroupId))
            {
                throw new ApiException(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            var cart = _orderRepository.Load<ICart>(cartOrderGroupId);
            if (cart == null)
            {
                throw new ApiException(ApiErrors.CartNotFound, HttpStatusCode.NotFound);
            }

            _orderRepository.Delete(cart.OrderLink);
        }

        internal CartApiModel Update(Guid cartId, CartApiModel model)
        {
            if (_marketService.GetMarket(model.Market) == null)
            {
                throw new ApiException(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            if (!_cartIdConverter.TryConvertToInt(cartId, out int cartOrderGroupId))
            {
                throw new ApiException(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            var cart = _orderRepository.Load<ICart>(cartOrderGroupId);
            if (cart == null)
            {
                throw new ApiException(ApiErrors.CartNotFound, HttpStatusCode.NotFound);
            }

            _mapper.UpdateCartFromModel(cart, model);

            _orderRepository.Save(cart);

            return MapToCartModel(cart);
        }

        internal CartApiModel Create(CartApiModel model)
        {
            if (_marketService.GetMarket(model.Market) == null)
            {
                throw new ApiException(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            var cart = _orderRepository.Create<ICart>(model.CustomerId, model.Name);

            _mapper.UpdateCartFromModel(cart, model);
            _orderRepository.Save(cart);

            return MapToCartModel(cart);
        }

        internal CheckoutApiModel PrepareCheckout(Guid cartId)
        {
            if (!_cartIdConverter.TryConvertToInt(cartId, out int cartOrderGroupId))
            {
                throw new ApiException(ApiErrors.CartNotFound, HttpStatusCode.BadRequest);
            }

            var cart = _orderRepository.Load<ICart>(cartOrderGroupId);
            if (cart == null)
            {
                throw new ApiException(ApiErrors.CartNotFound, HttpStatusCode.NotFound);
            }

            var validationIssues = _orderValidationService.ValidateOrder(cart);
            ValidateAndRemoveCouponCodesFromCart(cart, cart.GetFirstForm().Promotions);
            
            return new CheckoutApiModel
            {
                Cart = MapToCartModel(cart),
                ValidationIssues = _mapper.MapToValidationIssuesModel(validationIssues),
                Totals = _mapper.MapToTotalsModel(cart, _orderGroupCalculator.GetOrderGroupTotals(cart)),
                AvailableShippingMethods = _mapper.MapToShippingMethodModel(cart)
            };
        }

        internal OrderApiModel SaveAsPurchaseOrder(Guid cartId)
        {
            if (!_cartIdConverter.TryConvertToInt(cartId, out int cartOrderGroupId))
            {
                throw new ApiException(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            var cart = _orderRepository.Load<ICart>(cartOrderGroupId);
            if (cart == null)
            {
                throw new ApiException(ApiErrors.CartNotFound, HttpStatusCode.NotFound);
            }

            var validationIssues = _orderValidationService.ValidateOrder(cart);
            if (validationIssues.Any())
            {
                throw new ApiException(_mapper.MapToError(validationIssues), HttpStatusCode.BadRequest);
            }

            var order = _cartProvider.SaveAsPurchaseOrder(cart);
            _orderRepository.Delete(cart.OrderLink);

            return _mapper.MapToOrderModel(order);
        }

        private CartApiModel MapToCartModel(ICart cart)
        {
            return new CartApiModel
            {
                Id = _cartIdConverter.ConvertToGuid(cart.OrderLink.OrderGroupId),
                Name = cart.Name,
                CustomerId = cart.CustomerId,
                Market = cart.MarketId.Value,
                Currency = cart.Currency.CurrencyCode,
                LastUpdated = (cart.Modified ?? cart.Created).ToUniversalTime(),
                Shipments = _mapper.MapToShipmentModel(cart.GetFirstForm()),
                CouponCodes = cart.GetFirstForm().CouponCodes
            };
        }

        private void ValidateAndRemoveCouponCodesFromCart(ICart cart, IEnumerable<PromotionInformation> promotions)
        {
            var invalidCouponCodes = new List<string>();
            foreach (var couponCode in cart.GetFirstForm().CouponCodes)
            {
                if (!promotions.Any(x => x.CouponCode != null && x.CouponCode.Equals(couponCode, StringComparison.OrdinalIgnoreCase)))
                    invalidCouponCodes.Add(couponCode);
            }

            foreach (var invalidCouponCode in invalidCouponCodes)
            {
                cart.GetFirstForm().CouponCodes.Remove(invalidCouponCode);
            }
        }
    }
}
