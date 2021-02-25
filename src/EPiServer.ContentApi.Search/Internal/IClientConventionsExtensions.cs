using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.ClientConventions;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Convenience extensions for configuring IClientConventions associated with enabling Content Api Search
    /// </summary>
    public static class IClientConventionsExtensions
    {
        /// <summary>
        ///     Initialize Content Api Search using a provided ISerializer implementation
        /// </summary>
        /// <param name="conventions">Conventions to add to</param>
        public static void InitializeContentApiSearch(this IClientConventions conventions)
        {
            conventions.ForInstancesOf<IContent>()
                .IncludeField(x => x.ContentApiModel());
        }
    }
}
