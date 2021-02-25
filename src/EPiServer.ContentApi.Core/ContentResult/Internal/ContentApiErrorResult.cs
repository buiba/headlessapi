using System.Net;
using System.Net.Http;


namespace EPiServer.ContentApi.Core.ContentResult.Internal
{
    /// <summary>
    ///     Convenience class that extends <see cref="ContentApiResult{T}" /> for use when returning errors in the Content Api
    /// </summary>
    public class ContentApiErrorResult : ContentApiResult<ErrorResponse>
    {
        public ContentApiErrorResult(Error value, HttpStatusCode statusCode) : base(new ErrorResponse(value), statusCode) {  }
    }
}
 