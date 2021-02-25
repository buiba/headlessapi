using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.SpecializedProperties;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.PropertyDataTypes
{
    /// <summary>
    /// Defines the type of a content type property.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = "propertyDataType")]
    public readonly struct ExternalPropertyDataType : IEquatable<ExternalPropertyDataType>
    {
        private const string TypeNamePattern = "[a-zA-Z][(\\w\\s*\\w?),\\.]*";

        internal static ExternalPropertyDataType Block(string itemType) => new ExternalPropertyDataType(nameof(PropertyBlock), itemType);

        internal static bool IsBlock(string type) => nameof(PropertyBlock).Equals(type, StringComparison.OrdinalIgnoreCase);

        internal static bool IsBlock(ExternalPropertyDataType dataType) => IsBlock(dataType.Type);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyDataType"/> class.
        /// </summary>
        /// <param name="type">The main type.</param>
        public ExternalPropertyDataType(string type)
            : this(type, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyDataType"/> class.
        /// </summary>
        /// <param name="type">The main type.</param>
        /// <param name="itemType">The item type.</param>
        public ExternalPropertyDataType(string type, string itemType)
        {
            Type = type;
            ItemType = itemType;
        }

        /// <summary>
        /// The main data type name.
        /// </summary>
        [Required]
        [MaxLength(255)]
        [RegularExpression(TypeNamePattern)]
        [JsonProperty("dataType")]
        public string Type { get; }

        /// <summary>
        /// The item type for cases when the 'dataType' is 'PropertyBlock'.
        /// </summary>
        [MaxLength(255)]
        [RegularExpression(TypeNamePattern)]
        public string ItemType { get; }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
            => obj is ExternalPropertyDataType t && Equals(t);

        /// <inheritdoc />
        public bool Equals(ExternalPropertyDataType other)
            => StringComparer.OrdinalIgnoreCase.Equals(Type, other.Type) &&
               StringComparer.OrdinalIgnoreCase.Equals(ItemType, other.ItemType);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCodeCombiner = new HashCodeCombiner();
            hashCodeCombiner.Add(Type, StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(ItemType))
            {
                hashCodeCombiner.Add(ItemType, StringComparer.OrdinalIgnoreCase);
            }

            return hashCodeCombiner.CombinedHash;
        }

        /// <inheritdoc />
        public override string ToString() => string.IsNullOrEmpty(ItemType) ? Type : $"{Type}<{ItemType}>";

        /// <inheritdoc />
        public static bool operator ==(ExternalPropertyDataType left, ExternalPropertyDataType right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(ExternalPropertyDataType left, ExternalPropertyDataType right) => !left.Equals(right);
    }
}
