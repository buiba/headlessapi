using System;
using System.Net;

// Use the Xunit.Sdk namespace for improved xunit output formatting
namespace Xunit.Sdk
{
    public class StatusCodeMismatchException : XunitException
    {
        public StatusCodeMismatchException()
            : base("AssertResponse.StatusCode() Failure") { }

        public HttpStatusCode ExpectedStatusCode { get; set; }

        public HttpStatusCode ActualStatusCode { get; set; }

        public string ResponseContent { get; set; }

        public override string Message => $"{base.Message}{Environment.NewLine}Excepted: {ExpectedStatusCode}{Environment.NewLine}Actual:   {ActualStatusCode}{Environment.NewLine}" + (string.IsNullOrEmpty(ResponseContent) ? "" : $"Response Content:{Environment.NewLine}{ResponseContent}");
    }
}
