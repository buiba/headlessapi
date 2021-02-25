using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal.Controller
{
    [RoutePrefix(RouteConstants.VersionTwoApiRoute + "orders")]
    [PreviewFeatureFilter]
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [ApiExceptionFilter]
    public class OrderApiController : ApiController
    {
        private readonly OrderService _orderService;

        public OrderApiController() :
            this(ServiceLocator.Current.GetInstance<OrderService>())
        { }

        public OrderApiController(OrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates new order as per provided model.
        /// </summary>
        /// <param name="model">The order model.</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        [Route("")]
        [HttpPost]
        [HttpOptions]
        [ResponseType(typeof(OrderApiModel))]
        public IHttpActionResult Post(OrderInputModel model)
        {
            if (model == null)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidOrder, HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidOrder, HttpStatusCode.BadRequest);
            }
            
            var orderApiModel = _orderService.Create(model);

            var uri = new Uri(
                $"{Request.RequestUri.GetLeftPart(UriPartial.Authority)}/{RouteConstants.VersionTwoApiRoute}orders/{orderApiModel.OrderNumber}");
            var headers = new Dictionary<string, string>() {{"Location", uri.ToString()}};

            return new ContentApiResult<OrderApiModel>(orderApiModel, HttpStatusCode.Created, headers);
        }

        /// <summary>
        /// Gets a order.
        /// </summary>
        /// <param name="orderNumber">The order number.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("{orderNumber}")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(OrderApiModel))]
        public IHttpActionResult Get(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return new ContentApiErrorResult(ApiErrors.OrderNumberMissing, HttpStatusCode.BadRequest);
            }

            return new ContentApiResult<OrderApiModel>(_orderService.Get(orderNumber), HttpStatusCode.OK);
        }
    }
}
