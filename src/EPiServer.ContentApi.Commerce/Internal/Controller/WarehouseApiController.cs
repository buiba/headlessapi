using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Commerce.Internal.Infrastructure;
using EPiServer.ContentApi.Commerce.Internal.Models.Warehouse;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal.Controller
{
    [RoutePrefix(RouteConstants.VersionTwoApiRoute + "warehouse")]
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [ApiExceptionFilter]
    public class WarehouseApiController : ApiController
    {
        private readonly WarehouseService _warehouseService;

        public WarehouseApiController() : this(ServiceLocator.Current.GetInstance<WarehouseService>())
        {
        }

        public WarehouseApiController(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        /// <summary>
        /// Gets warehouse by code.
        /// </summary>
        /// <param name="warehouseCode">The warehouse code.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route("{warehouseCode}")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(WarehouseApiModel))]
        public IHttpActionResult Get(string warehouseCode)
        {
            if (string.IsNullOrWhiteSpace(warehouseCode))
            {
                return new ContentApiErrorResult(ApiErrors.InvalidHeaderValue, HttpStatusCode.BadRequest);
            }

            var warehouse = _warehouseService.GetWarehouse(warehouseCode);

            return warehouse == null ?
                new ContentApiErrorResult(ApiErrors.NotFound, HttpStatusCode.NotFound)
                : (IHttpActionResult) new ContentApiResult<WarehouseApiModel>(warehouse, HttpStatusCode.OK);
        }
    }
}
