using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Core.Security.Internal
{
    /// <summary>
    /// Interface for a filter which ensures that the MultiSiteFilteringEnabled setting is enforced 
    /// </summary>
    [ServiceConfiguration(typeof(IContentApiSiteFilter), Lifecycle = ServiceInstanceScope.Singleton)]

    public class ContentApiSiteFilter : IContentApiSiteFilter
    {
        protected readonly ISiteDefinitionResolver _siteDefinitionResolver;
        protected readonly IContentLoader _contentLoader;
        protected readonly ContentApiConfiguration _apiConfig;
        protected static readonly ILogger _logger = LogManager.GetLogger(typeof(ContentApiSiteFilter));

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentApiSiteFilter"/> class.
        /// </summary>
        public ContentApiSiteFilter(ISiteDefinitionResolver siteDefinitionResolver, ContentApiConfiguration apiConfig, IContentLoader contentLoader)
        {
            _siteDefinitionResolver = siteDefinitionResolver;
            _apiConfig = apiConfig;
            _contentLoader = contentLoader;
        }

        /// <summary>
        /// Determines if each IContent instance should be filtered by checking it against the ShouldFilterContent method.
        /// </summary>
        public virtual IEnumerable<IContent> FilterContents(IEnumerable<IContent> content, SiteDefinition siteDefinition)
        {
            return content.Where(x => x != null && !ShouldFilterContent(x, siteDefinition));
        }

        /// <summary>
        /// Compares the SiteDefinition provided with the SiteDefinition retrieved based on the provided content. If those
        /// values are not equal, and MultiSiteFilteringEnabled is enabled in configuration, we return true, indicating we 
        /// should filter the IContent
        /// </summary>
        /// <param name="content">IContent</param>
        /// <param name="siteDefinition">SiteDefinition</param>
        /// <returns>bool</returns>
        /// <exception cref="T:System.ArgumentException">
        /// Thrown if the provided IContent is null.
        /// </exception>
        public virtual bool ShouldFilterContent(IContent content, SiteDefinition siteDefinition)
        {
            var options = _apiConfig.GetOptions();

            if (content == null)
            {
                var exception = new ArgumentException("Provided content cannot be null", nameof(content));
                _logger.Error("Provided content cannot be null", exception);
                throw exception;
            }

            //If content is in For All Sites folder, should not filter content
            var ancestors = _contentLoader.GetAncestors(content.ContentLink);
            if (ancestors != null && ancestors.Any(an => an.ContentLink.CompareToIgnoreWorkID(SiteDefinition.Current.GlobalAssetsRoot)))
            {
                return false;
            }

            return options.MultiSiteFilteringEnabled &&
                   _siteDefinitionResolver.GetByContent(content.ContentLink, true) != siteDefinition;
        }
    }
}
