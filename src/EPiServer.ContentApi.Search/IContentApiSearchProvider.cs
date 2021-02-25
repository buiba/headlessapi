using EPiServer.ContentApi.Search.Internal;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Search
{
    /// <summary>
    ///     Represents a search provider which can successfully process a SearchRequest and return a SearchResponse for use in the Content Api
    /// </summary>
    public interface IContentApiSearchProvider
    {
        /// <summary>
        ///     Given a OData filter string, parse the filter into a Find's filter
        /// </summary>
        SearchResponse Search(SearchRequest searchRequest, IEnumerable<string> languages);
    }
}
