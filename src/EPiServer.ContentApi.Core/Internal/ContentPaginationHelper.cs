using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// This class helps to paginate through items.
    /// </summary>
    public class ContentPaginationHelper<T>
    {
        /// <summary>
        /// Get a range of items.
        /// </summary>
        /// <param name="startIndex">The startIndex of the current page</param>
        /// <param name="top">Number of items will be taken.</param>
        /// <param name="dataLoadingFunc">Function to get items.</param>
        /// <returns></returns>
        public static ContentDeliveryQueryRange<T> GetRange(int startIndex, int top, Func<int, int, PagedResult<IndexedItem<T>>> dataLoadingFunc)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "StartIndex value must be greater than or equal to zero");
            }

            if (top < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(top), "Top value must be greater than 0");
            }

            if (dataLoadingFunc == null)
            {
                throw new ArgumentNullException(nameof(dataLoadingFunc));
            }

            var currentIndex = startIndex;
            var pagedItems = new List<IndexedItem<T>>();
            int? totalItems = null;
            var canFetchMore = true;
            var numberOfItemsLeft = 0;
            PagedResult<IndexedItem<T>> fetchedData;

            do
            {
                numberOfItemsLeft = top - pagedItems.Count;

                //dataLoadingFunc might returns fewer items than top so we need the while loop here
                fetchedData = dataLoadingFunc(currentIndex, top);

                if (!totalItems.HasValue)
                {
                    totalItems = fetchedData.TotalCount;
                }

                // In case that top is too big, so current index + top may be bigger than totalItems.value
                // so in this case, only acquire totalItems.value items
                currentIndex = totalItems.Value - currentIndex > top ? currentIndex + top : totalItems.Value;
                canFetchMore = currentIndex < totalItems;

                pagedItems.AddRange(fetchedData.PagedItems.Take(numberOfItemsLeft));
            } while (canFetchMore && pagedItems.Count < top);


            // if there still has more data in database (canFetchMore = true), then clients can continue send data request to CD
            // if not, there still a case that we fetch data more than it is required (due to satisfy security related issue), and only a part of that is returned to clients, more data request is allowed.
            var hasMoreContent = canFetchMore ? true : (fetchedData.PagedItems.Count() > numberOfItemsLeft);
            var lastIndex = pagedItems.Any() ? pagedItems.Last().Index : currentIndex;
            return new ContentDeliveryQueryRange<T>(pagedItems.Select(x => x.Item), lastIndex, totalItems ?? 0, hasMoreContent);
        }
    }
}
