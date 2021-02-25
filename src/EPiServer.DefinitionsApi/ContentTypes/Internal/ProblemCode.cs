namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    /// <summary>
    /// List of known problems that are returned by the ContentType API.
    /// </summary>
    public static class ProblemCode
    {
        /// <summary>
        /// Indicates that an operation illegal for system types was attempted
        /// </summary>
        public const string SystemType = "SystemType";

        /// <summary>
        /// Indicates that there was an issue with the version on a content type
        /// </summary>
        public const string Version = "Version";

        /// <summary>
        /// Indicates that the provided base was invalid
        /// </summary>
        public const string InvalidBase = "InvalidBase";

        /// <summary>
        /// Indicates that the content type is being used by content or
        /// another content type.
        /// </summary>
        public const string InUse = "InUse";
    }
}
