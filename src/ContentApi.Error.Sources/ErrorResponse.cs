using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace EPiServer.ContentApi.Error.Internal
{
    /// <summary>
    ///     Implemented to align with Microsoft REST API Guidelines: https://github.com/Microsoft/api-guidelines/blob/vNext/Guidelines.md
    /// </summary>
    internal class ErrorResponse
    {
        /// <summary>
        /// Content type used for error responses.
        /// </summary>
        internal const string MediaType = "application/json";

        public ErrorResponse(Error error)
        {
            Error = error;
        }

        public Error Error { get; set; }

        /// <summary>
        /// Gets or sets the status code
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Kept for backward compatibility")]
        internal static ErrorResponse ForStatusCode(HttpStatusCode statusCode, string instance = null, string title = null, string detail = null, string code = null)
        {
            return new ErrorResponse(
                new Error
                {
                    Message = title ?? detail ?? statusCode.ToString(),
                    Code = code
                })
            {
                StatusCode = statusCode
            };
        }
    }
}
