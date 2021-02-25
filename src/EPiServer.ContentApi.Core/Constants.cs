namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Define constants for the paging
    /// </summary>
    public static class PagingConstants
    {
        /// <summary>
        /// Default value for the start index parameter while getting content with paging options
        /// </summary>
        public const int DefaultStartIndex = -1;

        /// <summary>
        /// Default value for the max rows parameter while getting content with paging options
        /// </summary>
        public const int DefaultMaxRows = -1;

        /// <summary>
        /// The continuation token header name
        /// </summary>
        public const string ContinuationTokenHeaderName = "x-epi-continuation";

        /// <summary>
        /// Default value for how many children that are returned when no top parameter is used
        /// </summary>
        public const int DefaultChildrenBatchSize = 100;
    }

    /// <summary>
    /// Define constants for metadata headers
    /// </summary>
    public static class MetadataHeaderConstants
    {
        /// <summary>
        /// The continuation token header name
        /// </summary>
        public const string ContentGUIDMetadataHeaderName = "x-epi-contentguid";

        /// <summary>
        /// The branch header name
        /// </summary>
        public const string BranchMetadataHeaderName = "x-epi-branch";

        /// <summary>
        /// The name of the response header that holds the site id.
        /// </summary>
        public const string SiteIdMetadataHeaderName = "x-epi-siteid";

        /// <summary>
        /// The name of the response header that holds the start page of the current site.
        /// </summary>
        public const string StartPageMetadataHeaderName = "x-epi-startpageguid";

        /// <summary>
        /// The name of the response header that holds part of a route that wasn't matched.
        /// </summary>
        public const string RemainingRouteMetadataHeaderName = "x-epi-remainingroute";

        /// <summary>
        /// The name of the response header returning the current context mode.
        /// </summary>
        public const string ContextModeMetadataHeaderName = "x-epi-contextmode";
    }
}
