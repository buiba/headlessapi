using EPiServer.Core;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Tracking
{
    /// <summary>
    /// A context object that contains information about whats included in current API request
    /// </summary>
    public class ContentApiTrackingContext
    {
        /// <summary>
        /// A list of all referenced content that are included in the output of current request
        /// </summary>
        public IDictionary<LanguageContentReference, ReferencedContentMetadata> ReferencedContent { get; } = new Dictionary<LanguageContentReference, ReferencedContentMetadata>();

        /// <summary>
        /// A list of all referenced content that are not publically available
        /// </summary>
        public HashSet<ContentReference> SecuredContent { get; } = new HashSet<ContentReference>();

        /// <summary>
        /// A list of all referenced sites that are returned to clients
        /// </summary>
        public HashSet<ReferencedSiteMetadata> ReferencedSites { get; } = new HashSet<ReferencedSiteMetadata >();
    }
}
