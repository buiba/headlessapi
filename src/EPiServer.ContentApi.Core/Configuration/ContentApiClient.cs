namespace EPiServer.ContentApi.Core.Configuration
{
    /// <summary>
    ///     Represents a Content Api Client which calls the Api to access information. Only valid registered clients may access the application, from the provided origin.
    /// </summary>
    public class ContentApiClient
    {
		/// <summary>
		/// Client ID
		/// </summary>
        public string ClientId { get; set; }

		/// <summary>
		/// Access control allow origin
		/// </summary>
        public string AccessControlAllowOrigin { get; set; }
    }
}
