using EPiServer.ContentApi.Core.Serialization.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    internal class MinifiedContentApiModel : IContentApiModel
    {
        public MinifiedContentApiModel(ContentModelReference contentLink, string name, LanguageModel langauge)
        {
            ContentLink = contentLink;
            Name = name;
            Language = langauge;
        }
        public ContentModelReference ContentLink { get; }
        public string Name { get; }
        public LanguageModel Language { get; }

        [JsonExtensionData]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
