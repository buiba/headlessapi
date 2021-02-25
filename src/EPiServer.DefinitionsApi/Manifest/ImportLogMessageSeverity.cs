namespace EPiServer.DefinitionsApi.Manifest
{
    /// <summary>
    /// Represent the severity level of an import log message.
    /// </summary>
    public enum ImportLogMessageSeverity
    {
        /// <summary>
        ///  Designates to information messages.
        /// </summary>
        Information = 0,

        /// <summary>
        ///  Designates to successful messages.
        /// </summary>
        Success,

        /// <summary>
        ///  Designates warning messages.
        /// </summary>
        Warning,

        /// <summary>
        ///  Designates error messages.
        /// </summary>
        Error
    }
}
