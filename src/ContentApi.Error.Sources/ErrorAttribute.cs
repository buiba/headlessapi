using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.Error.Internal
{
    /// <exclude />
    internal sealed class ErrorAttribute : ActionFilterAttribute, IExceptionFilter
    {
        // Problem code returned when the ModelState is invalid.
        internal const string InvalidModelProblemCode = "InvalidModel";

        /// <exclude />
        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken = default)
        {
            if (actionExecutedContext.Exception is ErrorException exception)
            {
                actionExecutedContext.Response = new HttpResponseMessage(exception.StatusCode)
                {
                    Content = new ErrorContent(exception.ErrorResponse)
                };
            }
            else if (actionExecutedContext.Exception is HttpResponseException responseException)
            {
                actionExecutedContext.Response = responseException.Response;
                OnActionExecuted(actionExecutedContext);
            }
            else
            {
                var error = ErrorResponse.ForStatusCode(
                    HttpStatusCode.InternalServerError,
                    actionExecutedContext.Request.RequestUri.PathAndQuery,
                    detail: (actionExecutedContext.ActionContext?.RequestContext?.IncludeErrorDetail == true) ? actionExecutedContext.Exception?.Message : "");

                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new ErrorContent(error)
                };
            }

            return Task.CompletedTask;
        }

        /// <exclude />
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            // Happens if exception is thrown before a Response is created
            if (actionExecutedContext.Response is null)
            {
                return;
            }

            if (actionExecutedContext.Response.Content is ObjectContent<HttpError> errorContent)
            {
                var error = TranslateError(actionExecutedContext.Response.StatusCode, actionExecutedContext.Request.RequestUri.PathAndQuery, (HttpError)errorContent.Value);
                actionExecutedContext.Response.Content = new ErrorContent(error);
            }
            else if (actionExecutedContext.Response.Content is null && actionExecutedContext.Response.StatusCode >= HttpStatusCode.BadRequest)
            {
                var error = ErrorResponse.ForStatusCode(actionExecutedContext.Response.StatusCode, actionExecutedContext.Request.RequestUri.PathAndQuery);
                actionExecutedContext.Response.Content = new ErrorContent(error);
            }
        }

        private static ErrorResponse TranslateError(HttpStatusCode status, string path, HttpError httpError)
        {
            var error = ErrorResponse.ForStatusCode(status, path, httpError.Message, httpError.MessageDetail);

            if (httpError.ModelState != null)
            {
                error.Error.Code = InvalidModelProblemCode;

                var details = new List<ErrorDetails>();

                foreach (var entry in httpError.ModelState)
                {
                    if (entry.Value is string[] list)
                    {
                        details.Add(new ErrorDetails
                        {
                            Target = entry.Key,
                            InnerError = list
                        });
                    }
                }

                error.Error.Details = details;
            }

            return error;
        }
    }
}
