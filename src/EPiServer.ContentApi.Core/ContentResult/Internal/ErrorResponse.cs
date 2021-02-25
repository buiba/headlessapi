namespace EPiServer.ContentApi.Core.ContentResult.Internal
{
    /// <summary>
    ///     Wrapper class for <see cref="Error"/> instances returned via <see cref="ContentApiErrorResult"/>.
    ///     Implemented to align with Microsoft REST API Guidelines: https://github.com/Microsoft/api-guidelines/blob/vNext/Guidelines.md
    /// </summary>
    public class ErrorResponse
    {
        public ErrorResponse(Error error)
        {
            Error = error;
        }

        public Error Error { get; set; }
    }
}