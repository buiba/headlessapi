using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Markets;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    /// <summary>
    /// Service to manage cart
    /// </summary>
    [ServiceConfiguration(typeof(OrderService))]
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly OrderValidationService _orderValidationService;
        private readonly IPurchaseOrderProvider _purchaseOrderProvider;
        private readonly IMarketService _marketService;
        private readonly Mapper _mapper;

        public OrderService(IOrderRepository orderRepository,
            IPurchaseOrderProvider purchaseOrderProvider,
            IMarketService marketService,
            IPurchaseOrderRepository purchaseOrderRepository,
            IOrderNumberGenerator orderNumberGenerator,
            Mapper mapper,
            OrderValidationService orderValidationService
            )
        {
            _orderRepository = orderRepository;
            _purchaseOrderProvider = purchaseOrderProvider;
            _marketService = marketService;
            _purchaseOrderRepository = purchaseOrderRepository;
            _orderNumberGenerator = orderNumberGenerator;
            _mapper = mapper;
            _orderValidationService = orderValidationService;
        }

        internal OrderApiModel Create(OrderInputModel model)
        {
            if (_marketService.GetMarket(model.Market) == null)
            {
                throw new ApiException(ApiErrors.InvalidOrder, HttpStatusCode.BadRequest);
            }

            var purchaseOrder = _purchaseOrderProvider.Create(model.CustomerId, model.Name);
            _mapper.UpdateOrderFromModel(purchaseOrder, model);

            var validationIssues = _orderValidationService.ValidateOrder(purchaseOrder);
            if (validationIssues.Any())
            {
                throw new ApiException(_mapper.MapToError(validationIssues), HttpStatusCode.BadRequest);
            }
            
            purchaseOrder.OrderNumber = _orderNumberGenerator.GenerateOrderNumber(purchaseOrder);
            var orderReference = _orderRepository.Save(purchaseOrder);
            purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);

            return _mapper.MapToOrderModel(purchaseOrder);
        }

        internal OrderApiModel Get(string orderNumber)
        {
            var purchaseOrder = _purchaseOrderRepository.Load(orderNumber);
            if (purchaseOrder == null)
            {
                throw new ApiException(ApiErrors.OrderNotFound, HttpStatusCode.NotFound);
            }

            return _mapper.MapToOrderModel(purchaseOrder);
        }
    }
}