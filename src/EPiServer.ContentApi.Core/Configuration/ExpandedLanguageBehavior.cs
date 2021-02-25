namespace EPiServer.ContentApi.Core.Configuration
{
    /// <summary>
    /// Defines the language behavior for expanded properties when content is loaded in a fallback language.
    /// If<see cref="RequestedLanguage" />, then expanded properties are loaded in the requested language, otherwise the content's fallback language.
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public enum ExpandedLanguageBehavior
    {
        /// <summary>
        /// Returns expanded properties in RequestedLanguage.
        /// </summary>
        RequestedLanguage,
        /// <summary>
        /// Returns expanded properties in ContentLanguage
        /// </summary>
        ContentLanguage
    }
}
