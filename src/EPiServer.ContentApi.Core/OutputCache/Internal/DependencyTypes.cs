namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    /// <summary>
    /// Contains constants for the default dependency types
    /// </summary>
    public static class DependencyTypes
    {
        /// <summary>
        /// Represents a dependency to Site
        /// </summary>
        public const string Site = "Site";

        /// <summary>
        /// Represents a dependency to Content
        /// </summary>
        public const string Content = "Content";

        /// <summary>
        /// Represents a dependency to Children
        /// </summary>
        public const string Children = "Children";

        /// <summary>
        ///  Represents a dependency to Ancestors
        /// </summary>
        public const string Ancestors = "Ancestors";
    }
}
