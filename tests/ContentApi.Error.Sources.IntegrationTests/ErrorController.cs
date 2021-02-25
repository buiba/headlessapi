using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EPiServer.ContentApi.Error.Internal;

namespace EPiServer.ContentApi.Error
{
    [Error]
    public class ErrorController : ApiController
    {
        private const string RoutePrefix = "api/episerver/v2.0/problems/";

        internal const string ThrowBadRequestPath = RoutePrefix + "throwbadrequest";
        internal const string ConflictPath = RoutePrefix + "conflict";
        internal const string UnhandledPath = RoutePrefix + "unhandled";
        internal const string ValidationPath = RoutePrefix + "validation";
        internal const string FilterErrorPath = RoutePrefix + "outside";
        internal const string InvalidParameter = "parameter1";
        internal const string InvalidParameterValue = "parameter1Value";

        [Route(ThrowBadRequestPath)]
        [HttpGet]
        public IHttpActionResult BadRequestMethod()
        {
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Crap input"));
        }

        [Route(ConflictPath)]
        [HttpGet]
        public IHttpActionResult ConflictMethod()
        {
            return Conflict();
        }

        [Route(UnhandledPath)]
        [HttpGet]
        public IHttpActionResult Unhandled()
        {
            throw new InvalidOperationException("kaboom");
        }

        [Route(ValidationPath)]
        [HttpGet]
        public IHttpActionResult Validate()
        {
            ModelState.AddModelError(InvalidParameter, InvalidParameterValue);
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
        }

        [Route(FilterErrorPath)]
        [HttpGet]
        [ExceptionInFilter]
        public IHttpActionResult FilterError()
        {
            return Ok();
        }

        private class ExceptionInFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(HttpActionContext actionContext)
            {
                throw new HttpResponseException(actionContext.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "This filter didn't like this"));
            }
        }
    }
}
