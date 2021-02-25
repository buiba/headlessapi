using System.ComponentModel.DataAnnotations;
using EPiServer.DefinitionsApi.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    /// <summary>
    /// Specifies the visibility status of a property in edit view.
    /// </summary>
    public enum VisibilityStatus
    {
        /// <summary>
        /// Visible
        /// </summary>
        Default = 0,

        /// <summary>
        /// Hidden
        /// </summary>
        Hidden = 1
    }

    /// <summary>
    /// Defines settings for content type properties that affects how the property is displayed when edited.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = "propertyEditSettings")]
    public class ExternalPropertyEditSettings
    {
        /// <summary>
        /// Creates a new <see cref="ExternalPropertyEditSettings"/>.
        /// </summary>
        public ExternalPropertyEditSettings(
            VisibilityStatus visibility,
            string displayName,
            string groupName,
            int order,
            string helpText,
            string hint)
        {
            Visibility = visibility;
            DisplayName = displayName;
            GroupName = groupName;
            Order = order;
            HelpText = helpText;
            Hint = hint;
        }

        /// <summary>
        /// Indicates if we should display an edit user interface for the property.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter), true)]
        public VisibilityStatus Visibility { get; set; }

        /// <summary>
        /// The name that should be used when editing the property.
        /// </summary>
        [MaxLength(255)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Name of the property group that this property should be displayed in.
        /// </summary>
        [MaxLength(100)]
        [RegularExpression("[a-zA-Z][\\w]*")]
        public string GroupName { get; set; }

        /// <summary>
        /// The field order used for ordering the properties when editing.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        /// <summary>
        /// The text that should be used as help when editing the property.
        /// </summary>
        [MaxLength(2000)]
        public string HelpText { get; set; }

        /// <summary>
        /// A hint that will be used when resolving which editor that should be used when editing this property.
        /// </summary>
        [MaxLength(255)]
        public string Hint { get; set; }
    }
}
