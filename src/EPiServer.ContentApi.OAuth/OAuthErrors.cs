namespace EPiServer.ContentApi.OAuth
{
    /// <summary>
    /// Class for storing OAuth error messages
    /// </summary>
    public static class OAuthErrors
    {
        /// <summary>
        /// When the client id of request is invalid. eq: missing client_id in request
        /// </summary>
        public static string InvalidClientId = "invalid_client_id";

        /// <summary>
        /// When origin of request is invalid. eg: CORS not enable for current origin
        /// </summary>
        public static string InvalidOrigin = "invalid_origin";

        /// <summary>
        /// When grant type is not valid. (grant_type could be password or refresh_token)
        /// </summary>
        public static string InvalidGrant = "invalid_grant";

        /// <summary>
        /// When credential is not valid (e.g: wrong username/password or user account is inactive/locked out)
        /// </summary>
        public static string InvalidCredentials = "invalid_credentials";

        /// <summary>
        /// Internal server error. eg: cannot connect to database
        /// </summary>
        public static string ServerError = "server_error";

        /// <summary>
        /// when refresh_token param is missing
        /// </summary>
        public static string InvalidRefreshToken = "invalid_refresh_token";
    }
}
