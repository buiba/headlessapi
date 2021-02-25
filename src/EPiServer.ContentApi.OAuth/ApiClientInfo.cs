namespace EPiServer.ContentApi.OAuth
{
	/// <summary>
	///     Represents a Api Client which calls the authorisation server to acquire access token. 
	///     Only valid registered clients may access the application, from the provided origin.
	/// </summary>
	public class ApiClientInfo
	{
		/// <summary>
		/// Client ID
		/// </summary>
		public string ClientId { get; set; }

		/// <summary>
		/// Access Control Allow Origin
		/// </summary>
		public string AccessControlAllowOrigin { get; set; }
	}
}
