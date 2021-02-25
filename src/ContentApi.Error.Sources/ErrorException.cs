using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace EPiServer.ContentApi.Error.Internal
{
    [SuppressMessage("Design", "CA1064:Exceptions should be public", Justification = "This exception is always caught internally")]
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "This exception is internal")]
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "This exception is internal")]
    internal class ErrorException : Exception
    {
        public ErrorException(HttpStatusCode statusCode, string message, string code = null)
            : this(ErrorResponse.ForStatusCode(statusCode, detail: message, code: code)) { }

        public ErrorException(ErrorResponse errorResponse)
        {
            if (errorResponse is null)
            {
                throw new ArgumentNullException(nameof(errorResponse));
            }

            if (!errorResponse.StatusCode.HasValue)
            {
                throw new ArgumentException("Problem detail must have a status code.", nameof(errorResponse));
            }

            ErrorResponse = errorResponse;
            StatusCode = errorResponse.StatusCode.GetValueOrDefault();
        }

        public ErrorResponse ErrorResponse { get; }

        public HttpStatusCode StatusCode { get; }

        public override string Message => ErrorResponse.Error.Message;
    }
}
