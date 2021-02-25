using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Model that contains necessary data for commerce bundle
    /// </summary>
    internal class BundleContentApiModel : ContentApiModel
    {
        public IEnumerable<string> Assets { get; set; }
    }
}
