using EPiServer.ContentApi.Core.Internal;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Serializable model class for store language information
    /// </summary>
    [ApiDefinition(Name = "language")]
    public class LanguageModel
    {
        /// <summary>
        /// Display name of language. Ex: English, Svenska
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Name of language. Ex: en, sv
        /// </summary>
        public string Name { get; set; }
    }
}
