using EPiServer.ServiceLocation;
using System;

namespace EPiServer.ContentApi.Search
{
    /// <summary>
    /// Configuration for content search api
    /// </summary>
    [Options]
    public class ContentApiSearchConfiguration
    {
        private readonly ContentSearchApiOptions _default;

        /// <summary>
        /// Initialize a new instance of <see cref="ContentApiSearchConfiguration"/>
        /// </summary>
        public ContentApiSearchConfiguration()
        {
            _default = new ContentSearchApiOptions(
                              searchCacheDuration: TimeSpan.FromMinutes(30),
                              maximumSearchResults: 100);
        }

        /// <summary>
        /// Get default options for search. Default() should be used at initialization time to customize/config default value
        /// </summary>
        public ContentSearchApiOptions Default()
        {
            return _default;
        }

        /// <summary>
        /// Get options for search. GetSearchOptions() should be used in application for api calling 
        /// By using this function, Default value will not be changed if clients using api change some values
        /// </summary>
        public ContentSearchApiOptions GetSearchOptions()
        {
            return _default.Clone() as ContentSearchApiOptions;
        }
    }
}
