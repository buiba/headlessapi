using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core;

namespace EPiServer.ContentApi.Commerce.Internal
{
    internal static class AssetContainerExtensions
    {
        /// <summary>
        /// Get all assets of item
        /// </summary>       
        public static IEnumerable<string> GetAssets(this IAssetContainer assetContainer, UrlResolverService urlResolverService)
        {
            return assetContainer.CommerceMediaCollection?.Select(asset => urlResolverService.ResolveUrl(asset.AssetLink, null));
        }
    }
}