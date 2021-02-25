using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    /// <summary>
    /// A content type definition describes the format of content items.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), NamingStrategyParameters = new object[] { true, false }, ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = DefinitionName)]
    public class ExternalContentType
    {
        internal const string DefinitionName = "contentType";

        /// <summary>
        /// The unique identifier of the content type.
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the content type.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [RegularExpression("[a-zA-Z][\\w]*")]
        public string Name { get; set; }

        /// <summary>
        /// The base type for this content type.
        /// </summary>
        [Required]
        [ValidateContentTypeBase]
        public string BaseType { get; set; }

        /// <summary>
        /// The version of the content type.
        /// </summary>
        /// <example>2.1.0</example>
        [ValidateVersion]
        public string Version { get; set; }

        /// <summary>
        /// The edit settings for the content type.
        /// </summary>
        public ExternalContentTypeEditSettings EditSettings { get; set; }

        /// <summary>
        /// A list with the properties of this content type.
        /// </summary>
        public IEnumerable<ExternalProperty> Properties { get; internal set; } = new List<ExternalProperty>();
    }
}
