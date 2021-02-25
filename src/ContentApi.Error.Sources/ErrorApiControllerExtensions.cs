using System;
using System.Net;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Error.Internal
{
    internal static class ErrorApiControllerExtensions
    {
        public static ErrorActionResult Problem(this ApiController apiController, HttpStatusCode statusCode, string message)
            => apiController.Problem(statusCode, ErrorResponse.ForStatusCode(statusCode, apiController.Request.RequestUri.PathAndQuery, detail: message));

        public static ErrorActionResult Problem(this ApiController apiController, HttpStatusCode statusCode, ErrorResponse error)
        {
            if (apiController is null)
            {
                throw new ArgumentNullException(nameof(apiController));
            }

            return new ErrorActionResult(statusCode, error, new JsonSerializerSettings(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), apiController);
        }
    }
}
