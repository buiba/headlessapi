using EPiServer.ContentApi.Core.Internal;
using Newtonsoft.Json;
using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Serializable model class for ContentReference.
    /// </summary>
    [ApiDefinition(Name = "contentReference")]
    public class ContentModelReference
    {
        /// <summary>
        /// Id number of the content.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The version id of the content.
        /// </summary>
        public int? WorkId { get; set; }

        /// <summary>
        /// The unique identifier of the content.
        /// </summary>
        public Guid? GuidValue { get; set; }

        /// <summary>
        /// The provider name that serves the content.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Url of the content.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Language of the content.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LanguageModel Language { get; set; }

        /// <summary>
        /// The expanded content of the content.
        /// </summary>
        public ContentApiModel Expanded { get; set; }

        public static bool IsNullOrEmpty(ContentModelReference contentModelReference) => contentModelReference is null || contentModelReference.Id is null;
    }
}
