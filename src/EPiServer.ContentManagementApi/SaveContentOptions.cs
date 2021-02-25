namespace EPiServer.ContentManagementApi
{
    /// <summary>
    /// Define options used when saving content.
    /// </summary>
    public class SaveContentOptions
    {
        /// <summary>
        /// Expose for test
        /// </summary>
        internal SaveContentOptions()
        {
            ContentValidationMode = ContentValidationMode.Complete;
        }

        public SaveContentOptions(ContentValidationMode contentValidationMode)
        {
            ContentValidationMode = contentValidationMode;
        }

        /// <summary>
        /// The validation mode used when saving content.
        /// </summary>
        public ContentValidationMode ContentValidationMode { get; }
    }
}
