using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace EPiServer.ContentApi.Problems.Internal
{
    [SuppressMessage("Design", "CA1064:Exceptions should be public", Justification = "This exception is always caught internally")]
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "This exception is internal")]
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "This exception is internal")]
    internal class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(HttpStatusCode statusCode, string message, string code = null)
            : this(ProblemDetails.ForStatusCode(statusCode, detail: message, code: code)) { }

        public ProblemDetailsException(ProblemDetails problem)
        {
            if (problem is null)
            {
                throw new ArgumentNullException(nameof(problem));
            }

            if (!problem.Status.HasValue)
            {
                throw new ArgumentException("Problem detail must have a status code.", nameof(problem));
            }

            Problem = problem;
            StatusCode = (HttpStatusCode)problem.Status;
        }

        public ProblemDetails Problem { get; }

        public HttpStatusCode StatusCode { get; }
    }
}
