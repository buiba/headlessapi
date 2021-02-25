using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// define model builder for CatalogContentBase
    /// </summary>
    internal interface ICatalogContentModelBuilder
    {
        /// <summary>
        /// The builder with higher order is chosen to handle a specific content in different contexts. The Default implementation has Order 100.
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// Whether a builder can handle a specific content
        /// </summary>       
        bool CanHandle(CatalogContentBase content);

        /// <summary>
        /// Create default model corresponding with the content
        /// </summary>
        ContentApiModel CreateDefaultModel(CatalogContentBase content);
    }
}
