using System;
using System.Net;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Problems.Internal
{
    internal static class ProblemDetailsApiControllerExtensions
    {
        public static ProblemDetailsActionResult Problem(this ApiController apiController, HttpStatusCode statusCode, string message)
        => apiController.Problem(statusCode, ProblemDetails.ForStatusCode(statusCode, apiController.Request.RequestUri.PathAndQuery, detail: message));

        public static ProblemDetailsActionResult Problem(this ApiController apiController, HttpStatusCode statusCode, ProblemDetails problem)
        {
            if (apiController is null)
            {
                throw new ArgumentNullException(nameof(apiController));
            }

            return new ProblemDetailsActionResult(statusCode, problem, new JsonSerializerSettings(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), apiController);
        }
    }
}
