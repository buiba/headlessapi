using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.Core;
using System;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.Commerce.Internal.Infrastructure
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            context.Response = ToActionResult(context.Exception).ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            base.OnException(context);
        }

        internal static IHttpActionResult ToActionResult(Exception exception)
        {
            switch (exception)
            {
                case ApiException apiException:
                    return new ContentApiErrorResult(apiException.Error, apiException.StatusCode);
                case TypeMismatchException _:
                case ContentNotFoundException _:
                case ResourceNotFoundException _:
                    return new ContentApiErrorResult(ApiErrors.NotFound, HttpStatusCode.NotFound);
                case AccessDeniedException _:
                    return new ContentApiErrorResult(ApiErrors.Forbidden, HttpStatusCode.Forbidden);
                case ArgumentNullException _:
                case ArgumentException _:
                    return new ContentApiErrorResult(ApiErrors.InvalidHeaderValue, HttpStatusCode.BadRequest);
                default:
                    return new ContentApiErrorResult(ApiErrors.InternalServerError, HttpStatusCode.InternalServerError);
            }
        }
    }
}
