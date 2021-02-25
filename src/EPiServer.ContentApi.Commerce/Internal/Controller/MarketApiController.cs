using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Markets;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal.Controller
{
    [RoutePrefix(RouteConstants.VersionTwoApiRoute + "markets")]
    [PreviewFeatureFilter]
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [ApiExceptionFilter]
    public class MarketApiController : ApiController
    {
        private readonly MarketsService _marketService;

        public MarketApiController() : this(
            ServiceLocator.Current.GetInstance<MarketsService>())
        { }

        public MarketApiController(MarketsService marketService)
        {
            _marketService = marketService;
        }

        /// <summary>
        /// Gets a single market.
        /// </summary>
        /// <param name="id">The market id.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("{id}")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(MarketApiModel))]
        public IHttpActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new ContentApiErrorResult(ApiErrors.InvalidMarketId, HttpStatusCode.BadRequest);
            }

            var market = _marketService.Get(id);
            if (market == null)
            {
                return new ContentApiErrorResult(ApiErrors.MarketNotFound, HttpStatusCode.NotFound);
            }

            return new ContentApiResult<MarketApiModel>(market, HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets all markets.
        /// </summary>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(IEnumerable<MarketApiModel>))]
        public IHttpActionResult GetAll()
        {
            return new ContentApiResult<IEnumerable<MarketApiModel>>(_marketService.GetAll(), HttpStatusCode.OK);
        }
    }
}
