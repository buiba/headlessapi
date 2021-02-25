using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Web;

namespace EPiServer.ContentApi.Core.Security
{
    /// <summary>
    /// Inferface for Site Filter. Determines if content should be filtered based on the site definition
    /// </summary>
    public interface IContentApiSiteFilter
    {
        /// <summary>
        /// Based on the provided IEnumerable of IContent and SiteDefinition, return an List of IContent 
        /// that includes only instances of IContent that should be included
        /// </summary>
        /// <param name="content">IContent</param>
        /// <param name="siteDefinition">SiteDefinition</param>
        /// <returns></returns>
        IEnumerable<IContent> FilterContents(IEnumerable<IContent> content, SiteDefinition siteDefinition);

        /// <summary>
        /// Based on the provided IContent and SiteDefinition, return a bool indicating if the provided content should be filtered.
        /// </summary>
        /// <param name="content">IContent</param>
        /// <param name="siteDefinition">SiteDefinition</param>
        /// <returns></returns>
        bool ShouldFilterContent(IContent content, SiteDefinition siteDefinition);
    }
}
