using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    /// <summary>
    /// Defines validation settings for content type properties that are checked when content is saved.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = "propertyValidationSettings")]
    public class ExternalPropertyValidationSettings
    {
        /// <summary>
        /// The name of the validation type that should be performed.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Indicates the severity that an validation error based on these settings should have.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter), true)]
        public ValidationErrorSeverity Severity { get; set; }

        /// <summary>
        /// The error message to use for any validation error.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Settings specific to the validation settings type.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Settings { get; set; } = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
    }
}
