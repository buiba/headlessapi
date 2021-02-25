using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// POCO class for store SiteDefinitionModel
    /// </summary>
    public class SiteDefinitionModel
    {
        public SiteDefinitionModel()
        {
            ContentRoots = new Dictionary<string, ContentModelReference>();
            Languages = new List<SiteDefinitionLanguageModel>();
            Hosts = new List<HostDefinitionModel>();
        }

        public string Name { get; set; }

        public Guid Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EditLocation { get; set; }

        public IDictionary<string, ContentModelReference> ContentRoots { get; set; }

        public IEnumerable<SiteDefinitionLanguageModel> Languages { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<HostDefinitionModel> Hosts { get; set; }
    }
}
