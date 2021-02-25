using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Model that contains necessary data for product variant
    /// </summary>
    internal class VariationContentApiModel : ContentApiModel
    {
        public IEnumerable<string> Assets { get; set; }
    }
}

