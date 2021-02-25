using System;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Class for storing constants of Content Delivery Api
    /// </summary>
    public static class RouteConstants
    {
        /// <summary>
        /// Base route prefix for Content Delivery Api endpoints
        /// </summary>
        [Obsolete("Replaced by VersionTwoApiRoute")]
        public const string BaseContentApiRoute = VersionTwoApiRoute;

        /// <summary>
        /// Base route prefix for Content Delivery Api endpoints
        /// </summary>
        public const string VersionTwoApiRoute = "api/episerver/v2.0/";

        /// <summary>
        /// Base route prefix for Content Delivery Api endpoints
        /// </summary>
        public const string VersionThreeApiRoute = "api/episerver/v3/";

        /// <summary>
        /// Accept language header
        /// </summary>
        public static string AcceptLanguage = "Accept-Language";

        /// <summary>
        /// Accept type application/json 
        /// </summary>
        public static string JsonContentType = "application/json";
    }
}
