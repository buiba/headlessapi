using System;
using System.ComponentModel;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.Internal;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    /// <summary>
    /// Describes a difference between a content type definition and the existing version.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    [ApiDefinition(Name = "contentTypeDifference")]
    public readonly struct ExternalContentTypeDifference : IEquatable<ExternalContentTypeDifference>
    {
        public static readonly ExternalContentTypeDifference None = new ExternalContentTypeDifference();

        private readonly bool? _isValid;
        private readonly string _reason;

        private ExternalContentTypeDifference(VersionComponent versionComponent, string reason, bool? isValid)
        {
            VersionComponent = versionComponent;
            _reason = reason;
            _isValid = isValid;
        }

        /// <summary>
        /// Creates a new <see cref="ExternalContentTypeDifference"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ExternalContentTypeDifference(VersionComponent versionComponent, string reason)
            : this(versionComponent, reason, true)
        {
        }

        /// <summary>
        /// Describes the reason for the difference.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("")]
        public string Reason => _reason ?? string.Empty;

        /// <summary>
        /// Indicates if the difference is considered a major, minor or patch level change.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public VersionComponent VersionComponent { get; }

        /// <summary>
        /// Indicates if this difference is a valid change that can be applied.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool IsValid => _isValid ?? true;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is ExternalContentTypeDifference other && Equals(other);

        /// <inheritdoc />
        public bool Equals(ExternalContentTypeDifference other)
        {
            if (IsValid)
            {
                return other.IsValid &&
                    VersionComponent == other.VersionComponent &&
                    StringComparer.InvariantCulture.Equals(Reason, other.Reason);
            }

            return !other.IsValid &&
                StringComparer.InvariantCulture.Equals(Reason, other.Reason);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCodeCombiner();

            hash.Add(IsValid);
            hash.Add(VersionComponent);

            if (Reason is object)
            {
                hash.Add(Reason, StringComparer.InvariantCulture);
            }

            return hash.CombinedHash;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsValid)
            {
                return $"[{VersionComponent}] {Reason}";
            }

            return $"[Invalid] {Reason}";
        }

        /// <inheritdoc />
        public static bool operator ==(ExternalContentTypeDifference left, ExternalContentTypeDifference right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(ExternalContentTypeDifference left, ExternalContentTypeDifference right) => !left.Equals(right);

        /// <summary>
        /// Creates a new <see cref="ExternalContentTypeDifference"/> for an invalid difference.
        /// </summary>
        /// <param name="reason">A description of the reason for the invalid difference.</param>
        /// <returns>A new <see cref="ExternalContentTypeDifference"/> instance.</returns>
        public static ExternalContentTypeDifference Invalid(string reason) => new ExternalContentTypeDifference(VersionComponent.None, reason, false);

        /// <summary>
        /// Creates a new <see cref="ExternalContentTypeDifference"/> for a major difference.
        /// </summary>
        /// <param name="reason">A description of the reason for the difference.</param>
        /// <returns>A new <see cref="ExternalContentTypeDifference"/> instance.</returns>
        public static ExternalContentTypeDifference Major(string reason) => new ExternalContentTypeDifference(VersionComponent.Major, reason);

        /// <summary>
        /// Creates a new <see cref="ExternalContentTypeDifference"/> for a minor difference.
        /// </summary>
        /// <param name="reason">A description of the reason for the difference.</param>
        /// <returns>A new <see cref="ExternalContentTypeDifference"/> instance.</returns>
        public static ExternalContentTypeDifference Minor(string reason) => new ExternalContentTypeDifference(VersionComponent.Minor, reason);

        /// <summary>
        /// Creates a new <see cref="ExternalContentTypeDifference"/> for a patch level difference.
        /// </summary>
        /// <param name="reason">A description of the reason for the difference.</param>
        /// <returns>A new <see cref="ExternalContentTypeDifference"/> instance.</returns>
        public static ExternalContentTypeDifference Patch(string reason) => new ExternalContentTypeDifference(VersionComponent.Patch, reason);
    }
}
