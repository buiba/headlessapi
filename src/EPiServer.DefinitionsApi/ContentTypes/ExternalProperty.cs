using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    /// <summary>
    /// A content type property definition describes a property of a content items.
    /// </summary>
    [JsonConverter(typeof(ExternalPropertyJsonConverter))]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = "contentTypeProperty")]
    public class ExternalProperty
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [RegularExpression("[a-zA-Z][\\w]*")]
        public string Name { get; set; }

        /// <summary>
        /// The data type of the property.
        /// </summary>
        [Required]
        public ExternalPropertyDataType DataType { get; set; }

        /// <summary>
        /// Indicates if the property has specific values for each content branch or if values are shared between all branches.
        /// </summary>
        public bool BranchSpecific { get; set; }

        /// <summary>
        /// The edit settings of the property.
        /// </summary>
        public ExternalPropertyEditSettings EditSettings { get; set; }

        /// <summary>
        /// The validation settings of the property.
        /// </summary>
        public IList<ExternalPropertyValidationSettings> Validation { get; } = new List<ExternalPropertyValidationSettings>();
    }
}
