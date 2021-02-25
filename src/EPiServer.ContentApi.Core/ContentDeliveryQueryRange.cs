using System.Collections.Generic;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Represent a list of items in a query range
    /// </summary>
    public class ContentDeliveryQueryRange<T>
    {
        public ContentDeliveryQueryRange(IEnumerable<T> contents, int lastIndex, int totalCount, bool hasMoreContent)
        {
            LastIndex = lastIndex;
            HasMoreContent = hasMoreContent;
            PagedResult = new PagedResult<T>(contents, totalCount);
        }

        /// <summary>
        /// Indicates whether the source list has more content to fetch or not.
        /// </summary>
        public bool HasMoreContent { get; }

        /// <summary>
        /// The last index of the current batch.
        /// </summary>
        public int LastIndex { get; }

        /// <summary>
        /// The paged result of the query.
        /// </summary>
        public PagedResult<T> PagedResult { get; }
    }
}
