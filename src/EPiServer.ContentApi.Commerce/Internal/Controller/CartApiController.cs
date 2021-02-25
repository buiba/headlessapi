using System;
using System.Collections.Generic;
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
    [RoutePrefix(RouteConstants.VersionTwoApiRoute + "carts")]
    [PreviewFeatureFilter]
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [ApiExceptionFilter]
    public class CartApiController : ApiController
    {
        private readonly CartService _cartService;

        public CartApiController() :
            this(ServiceLocator.Current.GetInstance<CartService>())
        { }

        public CartApiController(CartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Gets a cart.
        /// </summary>
        /// <param name="cartId">The cart id.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("{cartId:guid}")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(CartApiModel))]
        public IHttpActionResult Get(Guid cartId)
        {
            if (cartId == Guid.Empty)
            {
                return new ContentApiErrorResult(ApiErrors.CartIdMissing, HttpStatusCode.BadRequest);
            }

            return new ContentApiResult<CartApiModel>(_cartService.Get(cartId), HttpStatusCode.OK);
        }

        /// <summary>
        /// Updates an existing cart or creates new cart as per provided model.
        /// </summary>
        /// <param name="cartId">The cart id.</param>
        /// <param name="model">The cart model.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        [Route("{cartId:guid}")]
        [HttpPut]        
        [ResponseType(typeof(CartApiModel))]
        public IHttpActionResult Put(Guid cartId, CartApiModel model)
        {
            if (cartId == Guid.Empty)
            {
                return new ContentApiErrorResult(ApiErrors.CartIdMissing, HttpStatusCode.BadRequest);
            }

            if (model == null)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            return new ContentApiResult<CartApiModel>(_cartService.Update(cartId, model), HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates new cart as per provided model.
        /// </summary>
        /// <param name="model">The cart model.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        [Route("")]
        [HttpPost]
        [HttpOptions]
        [ResponseType(typeof(CartApiModel))]
        public IHttpActionResult Post(CartApiModel model)
        {
            if (model == null)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidCart, HttpStatusCode.BadRequest);
            }

            return new ContentApiResult<CartApiModel>(_cartService.Create(model), HttpStatusCode.OK);
        }

        /// <summary>
        /// Deletes a cart as per given cartId
        /// </summary>
        /// <param name="cartId">The unique cart id.</param>
        /// <response code="204">NoContent</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        [Route("{cartId:guid}")]
        [HttpDelete]        
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(Guid cartId)
        {
            if (cartId == Guid.Empty)
            {
                return new ContentApiErrorResult(ApiErrors.CartIdMissing, HttpStatusCode.BadRequest);
            }

            _cartService.Delete(cartId);
            return new ContentApiResult<string>(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Creates an order, returns an order model and location for the resource created in the header.
        /// </summary>
        /// <param name="cartId">The cart id.</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("{cartId:guid}/converttoorder")]
        [HttpPost]
        [HttpOptions]
        [ResponseType(typeof(OrderApiModel))]
        public IHttpActionResult ConvertToOrder(Guid cartId)
        {
            if (cartId == Guid.Empty)
            {
                return new ContentApiErrorResult(ApiErrors.CartIdMissing, HttpStatusCode.BadRequest);
            }

            var order = _cartService.SaveAsPurchaseOrder(cartId);

            var uri = new Uri(
                $"{Request.RequestUri.GetLeftPart(UriPartial.Authority)}/{RouteConstants.VersionTwoApiRoute}orders/{order.OrderNumber}");
            var headers = new Dictionary<string, string>() { { "Location", uri.ToString() } };

            return new ContentApiResult<OrderApiModel>(order, HttpStatusCode.Created, headers);
        }

        /// <summary>
        /// Validates cart and returns information related to cart totals, validation messages.
        /// </summary>
        /// <param name="cartId">The cart id.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("{cartId:guid}/preparecheckout")]
        [HttpPost]
        [HttpOptions]
        [ResponseType(typeof(CheckoutApiModel))]
        public IHttpActionResult PrepareCheckout(Guid cartId)
        {
            if (cartId == Guid.Empty)
            {
                return new ContentApiErrorResult(ApiErrors.CartIdMissing, HttpStatusCode.BadRequest);
            }

            return new ContentApiResult<CheckoutApiModel>(_cartService.PrepareCheckout(cartId), HttpStatusCode.OK);
        }
    }
}
