using EPiServer.ContentApi.Search.Internal;
using System;

namespace EPiServer.ContentApi.Search
{
    /// <summary>
    ///     Options to control behavior of the Content Search Api
    /// </summary>
    public class ContentSearchApiOptions : ICloneable
    {
        /// <summary>
        /// Initialize a new instance of <see cref="ContentSearchApiOptions"/>
        /// </summary>
        public ContentSearchApiOptions()
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="ContentSearchApiOptions"/>
        /// </summary>
        /// <param name="searchCacheDuration">Length of time that queries to the Search endpoint will be cached via StaticallyCacheFor.</param>
        /// <param name="maximumSearchResults">Maximum number of search results to return in a single request. </param>
        public ContentSearchApiOptions (TimeSpan searchCacheDuration, int maximumSearchResults)
        {
            SearchCacheDuration = searchCacheDuration;
            MaximumSearchResults = maximumSearchResults;
        }

        /// <summary>
        ///     Length of time that queries to the Search endpoint will be cached via StaticallyCacheFor.
        /// </summary>
        public virtual TimeSpan SearchCacheDuration
        {
            get; protected set;
        } 

        /// <summary>
        ///     Maximum number of search results to return in a single request. Enforced on the Top parameter of <see cref="SearchRequest"/>
        /// </summary>
        public virtual int MaximumSearchResults
        {
            get; protected set;
        }

        /// <summary>
        /// Set search cache duration
        /// </summary>
        public virtual ContentSearchApiOptions SetSearchCacheDuration(TimeSpan searchCacheDuration)
        {
            SearchCacheDuration = searchCacheDuration;
            return this;
        }

        /// <summary>
        /// Set maximum search results
        /// </summary>
        public virtual ContentSearchApiOptions SetMaximumSearchResults(int maximumSearchResults)
        {
            MaximumSearchResults = maximumSearchResults;
            return this;
        }

        /// <summary>
        /// Clone object
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
