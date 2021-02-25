using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Model that contains necessary data for commerce catalog node
    /// </summary>
    internal class NodeContentApiModel : ContentApiModel
    {
        public IEnumerable<string> Assets { get; set; }
    }
}
