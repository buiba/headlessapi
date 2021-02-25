using EPiServer.ServiceLocation;
using System;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Represents a search request made to the Content Api
    /// </summary>
    public class SearchRequest
    {
        private readonly ContentApiSearchConfiguration _searchConfig;
        private readonly ContentSearchApiOptions _option;

        public SearchRequest() : this(ServiceLocator.Current.GetInstance<ContentApiSearchConfiguration>())
        {

        }

        public SearchRequest(ContentApiSearchConfiguration searchService)
        {
            _searchConfig = searchService;
            _option = _searchConfig.GetSearchOptions();
            Top = _option.MaximumSearchResults;
            Skip = 0;
        }

        /// <summary>
        ///     Optional Free text query string for a search request
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        ///     Number of results to bypass in the response. For use with paginated results.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        ///     Number of results to retrieve in the response. For use with paginated results.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        ///     Indicates whether the response should be personalized when sent back to the user 
        /// </summary>
        public bool Personalize { get; set; } = false;

        /// <summary>
        ///     Filter string based on OData syntax for filtering content
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        ///     Order By string for ordering returned search results
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        ///     Comma-separated list of properties to Expand, fetching content in the response
        /// </summary>
        public string Expand { get; set; } = String.Empty;
    }
}