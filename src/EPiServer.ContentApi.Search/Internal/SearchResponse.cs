using EPiServer.ContentApi.Core.Serialization.Models;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Represents a search response sent back from the Content Api
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        ///     Total number of matching results from the search request.  For use in calculating pagination on the client, this number may be bigger than the returned total of <see cref="Results"/>
        ///     in a single request based on the provider <see cref="SearchRequest.Top"/> value, as well as the <see cref="ContentSearchApiOptions.MaximumSearchResults"/> configuration value.
        /// </summary>
        public int TotalMatching { get; set; }

        /// <summary>
        ///     Search Results in the form of <see cref="ContentApiModel"/> instances
        /// </summary>
        public IEnumerable<ContentApiModel> Results { get; set; }
    }
}
