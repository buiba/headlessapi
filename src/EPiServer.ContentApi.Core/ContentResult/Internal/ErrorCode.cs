namespace EPiServer.ContentApi.Core.ContentResult.Internal
{
    /// <summary>
    ///     Class containing error codes that are returned in the API for consumers. For use with <see cref="ContentApiErrorResult"/> in conjunction with an <see cref="Error"/> instance
    /// </summary>
    public static class ErrorCode
    {
        public static string InternalServerError = "InternalServerError";

        public static string InvalidHeaderValue = "InvalidHeaderValue";

        public static string InputOutOfRange = "InputOutOfRange";

        public static string Unauthorized = "Unauthorized";

        public static string Forbidden = "Forbidden";

        public static string NotFound = "NotFound";

        public static string InvalidFilterClause = "InvalidFilterClause";

        public static string InvalidOrderByClause = "InvalidOrderByClause";

        public static string InvalidParameter = "InvalidParameter";
    }
}
