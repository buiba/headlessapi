namespace EPiServer.ContentManagementApi
{
    /// <summary>
    /// The validation mode used when saving content.
    /// </summary>
    public enum ContentValidationMode
    {        
        /// <summary>
        /// Assert all validation rules.
        /// </summary>
        Complete,
        /// <summary>
        /// Only validate for broken data and not trigger any additional validation rules.
        /// </summary>
        Minimal
    }
}
