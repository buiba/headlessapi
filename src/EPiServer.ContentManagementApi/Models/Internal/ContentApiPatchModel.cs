using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentManagementApi.Models.Internal
{
    /// <summary>
    /// Model for endpoint that update a content item.
    /// </summary>
    [ApiDefinition(Name = "patchContentModel")]
    public class ContentApiPatchModel
    {
        /// <summary>
        /// Name of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        [MaxLength(255, ErrorMessage = "'Name' has exceeded its limit of 255 characters.")]
        public string Name { get; set; }

        /// <summary>
        /// Language of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        [ValidateLanguageModel]
        public LanguageModel Language { get; set; }

        /// <summary>
        /// Route segment of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public string RouteSegment { get; set; }

        /// <summary>
        /// The start publish date for the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public DateTimeOffset? StartPublish { get; set; }

        /// <summary>
        /// The stop publish date of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public DateTimeOffset? StopPublish { get; set; }

        /// <summary>
        /// The version status of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        [ValidateVersionStatus]
        public VersionStatus? Status { get; set; }

        /// <summary>
        /// Other properties of the content.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        internal ISet<string> UpdatedMetadata { get; set; } = new HashSet<string>();
    }
}
