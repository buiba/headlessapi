using System;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Tracking
{
    /// <summary>
    ///     Metadata for referenced content
    /// </summary>
    public class ReferencedContentMetadata
    {
        /// <summary>
        ///     Expiration time
        /// </summary>
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        ///     Saved time
        /// </summary>
        public DateTime? SavedTime { get; set; }

        /// <summary>
        ///     A list of all personalized properties associated with content
        /// </summary>
        public HashSet<string> PersonalizedProperties { get; } = new HashSet<string>();
    }
}
