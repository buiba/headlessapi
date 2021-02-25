using System.Collections.Generic;
using System.Linq;
using EPiServer.DefinitionsApi.Manifest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.PropertyGroups.Manifest
{
    /// <summary>
    /// Defines property groups to be imported.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class PropertyGroupsSection : IManifestSection
    {
        /// <summary>
        /// List of property groups that should be imported.
        /// </summary>
        public IEnumerable<PropertyGroupModel> Items { get; set; } = Enumerable.Empty<PropertyGroupModel>();
    }
}
