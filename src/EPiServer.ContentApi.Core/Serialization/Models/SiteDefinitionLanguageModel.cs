using EPiServer.ContentApi.Core.Serialization.Models;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Define SiteDefinitionLanguageModel
    /// </summary>
    public class SiteDefinitionLanguageModel : LanguageModel
    {
        /// <summary>
        /// Defines if this language model is master language or not
        /// </summary>
        public bool IsMasterLanguage { get; set; }

        /// <summary>
        /// The string uses to represent this language as a URL segment
        /// </summary>
        public string UrlSegment { get; set; }

        /// <summary>
        /// The primary location of the site language.
        /// </summary>
        /// <remarks>
        /// This property is only include when hosts are excluded from the main site information.
        /// </remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }
}
