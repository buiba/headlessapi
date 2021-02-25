using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.Manifest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes.Manifest
{
    /// <summary>
    /// Defines content types to be imported.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class ContentTypesSection : IManifestSection
    {
        /// <summary>
        /// Indicates which types of content type downgrades that are allowed during the import.
        /// </summary>
        public VersionComponent? AllowedDowngrades { get; set; }

        /// <summary>
        /// Indicates which types of content type upgrades that are allowed during the import.
        /// </summary>
        public VersionComponent? AllowedUpgrades { get; set; }

        /// <summary>
        /// List of content types that should be imported.
        /// </summary>
        public ValidatableExternalContentTypes Items { get; set; } = new ValidatableExternalContentTypes();
    }
}
