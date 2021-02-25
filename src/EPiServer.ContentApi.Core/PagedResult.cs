using System.Collections.Generic;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Represent a page of items and the total items.
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// Initializes a new instacne of the <see cref="PagedResult{T}"/>
        /// </summary>
        public PagedResult(IEnumerable<T> pagedItems, int totalCount)
        {
            PagedItems = pagedItems;
            TotalCount = totalCount;
        }

        /// <summary>
        /// Paged items in the source list.
        /// </summary>
        public IEnumerable<T> PagedItems { get; }

        /// <summary>
        /// Total item count in the source list.
        /// </summary>
        public int TotalCount { get; }
    }
}
