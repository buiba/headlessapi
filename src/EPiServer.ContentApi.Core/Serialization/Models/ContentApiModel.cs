using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Internal;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Serializable model class for IContent exposed in the Content API.
    /// </summary>
    [ApiDefinition(Name = "content")]
    public class ContentApiModel : IContentApiModel
    {
        /// <summary>
        /// Content link of the content.
        /// </summary>
        public ContentModelReference ContentLink { get; set; }

        /// <summary>
        /// Name of the content.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Language of the content.
        /// </summary>
        public LanguageModel Language { get; set; }

        /// <summary>
        /// The existing languages of the content.
        /// </summary>
        public List<LanguageModel> ExistingLanguages { get; set; }

        /// <summary>
        /// Master language of the content.
        /// </summary>
        public LanguageModel MasterLanguage { get; set; }

        /// <summary>
        /// Content type of the content.
        /// </summary>
        public List<string> ContentType { get; set; }

        /// <summary>
        /// Parent link of the content.
        /// </summary>
        public ContentModelReference ParentLink { get; set; }

        /// <summary>
        /// Route segment of the content.
        /// </summary>
        public string RouteSegment { get; set; }

        /// <summary>
        /// Url of the content.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The date when the content was last marked as changed.
        /// </summary>
        public DateTime? Changed { get; set; }

        /// <summary>
        /// The datetime when the content was created.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// The start publish date of the content.
        /// </summary>
        public DateTime? StartPublish { get; set; }

        /// <summary>
        /// The stop publish date of the content.
        /// </summary>
        public DateTime? StopPublish { get; set; }

        /// <summary>
        /// The datetime when the content was last saved.
        /// </summary>
        public DateTime? Saved { get; set; }

        /// <summary>
        /// Status of the content.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Other properties of the content.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
