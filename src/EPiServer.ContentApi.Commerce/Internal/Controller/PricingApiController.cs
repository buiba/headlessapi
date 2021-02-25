using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Pricing;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;

namespace EPiServer.ContentApi.Commerce.Internal.Controller
{
    [RoutePrefix(RouteConstants.VersionTwoApiRoute + "pricing")]
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [ApiExceptionFilter]
    public class PricingApiController : ApiController
    {
        private readonly PricingService _pricingService;
        private readonly ReferenceConverter _referenceConverter;

        public PricingApiController() : this(
            ServiceLocator.Current.GetInstance<PricingService>(),
            ServiceLocator.Current.GetInstance<ReferenceConverter>())
        {}

        public PricingApiController(PricingService pricingService, ReferenceConverter referenceConverter)
        {
            _pricingService = pricingService;
            _referenceConverter = referenceConverter;
        }

        /// <summary>
        /// Gets sku price information.
        /// </summary>
        /// <param name="contentIds">The content ids.</param>
        /// <param name="marketId">The market id.</param>
        /// <param name="currencyCode">The currency code.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(IEnumerable<PricingApiModel>))]
        public IHttpActionResult GetPricings([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] Guid[] contentIds, string marketId, string currencyCode)
        {
            if (!ModelState.IsValid)
            {
                return new ContentApiErrorResult(ApiErrors.InvalidHeaderValue, HttpStatusCode.BadRequest);
            }

            if (!contentIds.Any())
            {
                return new ContentApiErrorResult(ApiErrors.InvalidHeaderValue, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(marketId))
            {
                return new ContentApiErrorResult(ApiErrors.InvalidHeaderValue, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return new ContentApiErrorResult(ApiErrors.InvalidHeaderValue, HttpStatusCode.BadRequest);
            }

            return new ContentApiResult<IEnumerable<PricingApiModel>>(_pricingService.GetPricings(_referenceConverter.GetCodes(contentIds), marketId, currencyCode), HttpStatusCode.OK);
        }
    }
}
