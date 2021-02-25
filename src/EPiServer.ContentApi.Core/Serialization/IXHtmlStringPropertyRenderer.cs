using EPiServer.SpecializedProperties;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Interface for the implementation of rendering PropertyXhtmlStrings
    /// </summary>
    public interface IXHtmlStringPropertyRenderer
    {
        /// <summary>
        /// Renders an PropertyXHtmlString and returns it as a string. Takes a boolean to determine 
        /// if personalization should be included when rendering.
        /// </summary>
        /// <param name="xhtmlString">PropertyXhtmlString</param>
        /// /// <param name="excludePersonalizedContent">excludePersonalizedContent</param>
        /// <returns>string</returns>
        string Render(PropertyXhtmlString xhtmlString, bool excludePersonalizedContent);
    }
}
