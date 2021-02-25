namespace EPiServer.DefinitionsApi.PropertyGroups.Internal
{
    /// <summary>
    /// List of known problems that are returned by the PropertyGroups API.
    /// </summary>
    internal static class ProblemCode
    {
        /// <summary>
        /// Indicates that an operation illegal for system groups was attempted
        /// </summary>
        public const string SystemGroup = "SystemGroup";
    }
}
