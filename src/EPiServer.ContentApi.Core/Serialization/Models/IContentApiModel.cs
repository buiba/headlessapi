using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Signature for models that represents a content item being serialized
    /// </summary>
    public interface IContentApiModel
    {
        /// <summary>
        /// The reference for the content item
        /// </summary>
        ContentModelReference ContentLink { get; }

        /// <summary>
        /// The language for the content item
        /// </summary>
        LanguageModel Language { get; }

        /// <summary>
        /// The name of the content item
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Other properties that should be serialized in the content representation
        /// </summary>
        IDictionary<string, object> Properties { get; }
    }
}