using EPiServer.ContentApi.Core.ContentResult.Internal;
using System;
using System.Net;

namespace EPiServer.ContentApi.Commerce.Internal.Infrastructure
{
    public class ApiException : Exception
    {
        public Error Error { get; }
        public HttpStatusCode StatusCode { get; }

        public ApiException(string message)
            : base(message)
        {
            Error = new Error { Message = message };
        }

        public ApiException(string message, Exception inner)
            : base(message, inner)
        {
            Error = new Error { Message = message };
        }

        public ApiException(Error error, HttpStatusCode statusCode)
            : base($"{error.Code}-{error.Message}")
        {
            Error = error;
            StatusCode = statusCode;
        }
    }
}
