using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Model that contains necessary data for commerce package
    /// </summary>
    internal class PackageContentApiModel: ContentApiModel
    {      
        public IEnumerable<string> Assets { get; set; }
    }
}
