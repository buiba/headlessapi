namespace EPiServer.ContentManagementApi.Internal
{
    /// <summary>
    /// Define header constants of Content management.
    /// </summary>
    public static class HeaderConstants
    {
        /// <summary>
        /// The name of the request header that controls a content item is deleted immediately or it's moved to the recycle bin first.
        /// </summary>
        public const string PermanentDeleteHeaderName = "x-epi-permanent-delete";

        /// <summary>
        /// The name of the request header that controls the validation mode when saving content.
        /// If header value is 'minimal' then content will be saved with only validation for broken data.
        /// If header value is 'complete' then all validation rules are asserted when saving content.
        /// </summary>
        public const string ValidationMode = "x-epi-validation-mode";

        /// <summary>
        /// The location of new content just created.
        /// </summary>
        public const string Location = "Location";
    }
}
