using System.Collections.Generic;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Security
{
    /// <summary>
    ///     Interface for a filter which ensures that the Content Api Required Role setting is enforced
    /// </summary>
    public interface IContentApiRequiredRoleFilter
    {
        /// <summary>
        /// Based on the provided IEnumerable of IContent, return an List of IContent 
        /// that includes only instances of IContent that have the required role attached
        /// </summary>
        /// <param name="content">IEnumable of IContent to filter</param>
        /// <returns>List of content that were not filtered out</returns>
        IEnumerable<IContent> FilterContents(IEnumerable<IContent> content);
        
        /// <summary>
        /// Based on the provided IContent instance, return a bool indicating if the provided content should be filtered out.
        /// </summary>
        /// <param name="content">IContent filter</param>
        /// <returns>true if the Content should be filtered, false if it should not</returns>
        bool ShouldFilterContent(IContent content);
    }
}
