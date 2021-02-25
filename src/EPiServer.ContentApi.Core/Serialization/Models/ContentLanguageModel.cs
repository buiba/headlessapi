using System;
namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Serializable model class for store IContent language-related information  
    /// </summary>
    public class ContentLanguageModel : LanguageModel
    {
        /// <summary>
        /// Url of content in this language. Ex: /en/alloy-plan
        /// </summary>
        public string Link { get; set; }
    }
}
