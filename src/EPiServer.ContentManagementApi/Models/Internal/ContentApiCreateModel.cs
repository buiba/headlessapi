using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using Newtonsoft.Json;

namespace EPiServer.ContentManagementApi.Models.Internal
{
    /// <summary>
    /// Contains information of the new content to be created.
    /// </summary>
    public class ContentApiCreateModel
    {
        /// <summary>
        /// Content link of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public ContentReferenceInputModel ContentLink { get; set; }

        /// <summary>
        /// Name of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        [Required]
        [MaxLength(255, ErrorMessage = "'Name' has exceeded its limit of 255 characters.")]
        public string Name { get; set; }

        /// <summary>
        /// Language of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        [ValidateLanguageModel]
        public LanguageModel Language { get; set; }

        /// <summary>
        /// Content type of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        [ValidateContentType]
        [Required(ErrorMessage = "Content Type is required.")]
        public List<string> ContentType { get; set; }

        /// <summary>
        /// Parent link of the content.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        [Required]
        public ContentReferenceInputModel ParentLink { get; set; }

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
    }
}

