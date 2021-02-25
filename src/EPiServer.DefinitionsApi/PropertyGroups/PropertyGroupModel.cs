using System.ComponentModel.DataAnnotations;
using EPiServer.DefinitionsApi.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.PropertyGroups
{
    /// <summary>
    /// Defines a groups for content type properties.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = "propertyGroup")]
    public class PropertyGroupModel
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        [Required]
        [RegularExpression("[a-zA-Z0-9][\\w]*")]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// The display name of the group.
        /// </summary>
        [MaxLength(100)]
        public string DisplayName { get; set; }

        /// <summary>
        ///  The relative sort index.
        /// </summary>
        [Range(0, 10000)]
        public int SortIndex { get; set; }

        /// <summary>
        /// Indicates if the group is managed by the system or not.
        /// </summary>
        public bool? SystemGroup { get; set; }
    }
}
