using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.Problems.Internal
{
    /// <exclude />
    internal sealed class ProblemDetailsAttribute : ActionFilterAttribute, IExceptionFilter
    {
        // Problem code returned when the ModelState is invalid.
        internal const string InvalidModelProblemCode = "InvalidModel";

        /// <exclude />
        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken = default)
        {
            if (actionExecutedContext.Exception is ProblemDetailsException problemDetailsException)
            {
                var problem = problemDetailsException.Problem;
                if (problem.Instance is null)
                {
                    problem.Instance = actionExecutedContext.Request.RequestUri.PathAndQuery;
                }

                actionExecutedContext.Response = new HttpResponseMessage(problemDetailsException.StatusCode)
                {
                    Content = new ProblemDetailsContent(problem)
                };
            }
            else if (actionExecutedContext.Exception is HttpResponseException responseException)
            {
                actionExecutedContext.Response = responseException.Response;
                OnActionExecuted(actionExecutedContext);
            }
            else
            {
                var problem = ProblemDetails.ForStatusCode(
                    HttpStatusCode.InternalServerError,
                    actionExecutedContext.Request.RequestUri.PathAndQuery,
                    detail: (actionExecutedContext.ActionContext?.RequestContext?.IncludeErrorDetail == true) ? actionExecutedContext.Exception?.Message : "");

                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new ProblemDetailsContent(problem)
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
                var problem = TranslateError(actionExecutedContext.Response.StatusCode, actionExecutedContext.Request.RequestUri.PathAndQuery, (HttpError)errorContent.Value);
                actionExecutedContext.Response.Content = new ProblemDetailsContent(problem);
            }
            else if (actionExecutedContext.Response.Content is null && actionExecutedContext.Response.StatusCode >= HttpStatusCode.BadRequest)
            {
                var problem = ProblemDetails.ForStatusCode(actionExecutedContext.Response.StatusCode, actionExecutedContext.Request.RequestUri.PathAndQuery);
                actionExecutedContext.Response.Content = new ProblemDetailsContent(problem);
            }
        }

        private static ProblemDetails TranslateError(HttpStatusCode status, string path, HttpError httpError)
        {
            var problem = ProblemDetails.ForStatusCode(status, path, httpError.Message, httpError.MessageDetail);

            if (httpError.ModelState != null)
            {
                problem.Code = InvalidModelProblemCode;

                foreach (var entry in httpError.ModelState)
                {
                    if (entry.Value is string[] list)
                    {
                        problem.Errors.Add(entry.Key, list);
                    }
                }
            }

            foreach (var entry in httpError.Where(e => e.Key != nameof(HttpError.Message) && e.Key != nameof(HttpError.ModelState)))
            {
                problem.Extensions.Add(entry);
            }

            return problem;
        }
    }
}
