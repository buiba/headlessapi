using System.ComponentModel.DataAnnotations;
using EPiServer.DefinitionsApi.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    /// <summary>
    /// Defines settings for content type that affects how it is displayed when edited.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = "editSettings")]
    public class ExternalContentTypeEditSettings
    {
        /// <summary>
        /// Creates a new <see cref="ExternalContentTypeEditSettings"/>.
        /// </summary>
        public ExternalContentTypeEditSettings(
            string displayName,
            string description,
            bool available,
            int order)
        {
            DisplayName = displayName;
            Description = description;
            Available = available;
            Order = order;
        }

        /// <summary>
        /// The name that should be used when editing the content type.
        /// </summary>
        [MaxLength(50)]
        public string DisplayName { get; set; }

        /// <summary>
        /// The description that should be used when editing the content type.
        /// </summary>
        [MaxLength(255)]
        public string Description { get; set; }

        /// <summary>
        /// Determines if the content type is available when editing.
        /// </summary>
        public bool Available { get; set; }

        /// <summary>
        /// The field order used for ordering the content types when editing.
        /// </summary>
        public int Order { get; set; }
    }
}
