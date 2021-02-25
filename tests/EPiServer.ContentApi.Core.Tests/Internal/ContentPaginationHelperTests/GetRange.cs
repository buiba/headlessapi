using EPiServer.ContentApi.Core.Internal;
using System;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Internal.ContentPaginationHelperTests
{
    public class ContentPaginationHelperTests
    {

    }

    public class GetRange : ContentPaginationHelperTests
    {
        private readonly PagedResult<IndexedItem<string>> _pagedResult = new PagedResult<IndexedItem<string>>(new[] { new IndexedItem<string> { Index = 0, Item = "a" }, new IndexedItem<string> { Index = 1, Item = "b" } }, 2);

        private readonly int[] _originalItems = new[] { 1, 2, 3, 4, 5, 6 };
        private readonly Func<int, int, PagedResult<IndexedItem<int>>> _dataLoadingFunc;

        private ContentDeliveryQueryRange<int> _query;

        public GetRange()
        {
            _dataLoadingFunc = (startIndex, top) =>
            {
                var indexedItems = _originalItems
                                    .Skip(startIndex)
                                    .Take(top)
                                    .Select((c, i) => new IndexedItem<int> { Item = c, Index = i + startIndex });

                indexedItems = indexedItems.Where(x => x.Item % 2 != 0);

                return new PagedResult<IndexedItem<int>>(indexedItems, _originalItems.Count());
            };
        }

        [Fact]
        void ItShouldThrowArgumentOutOfRangeException_WhenNegativeStartIndex()
        {
            Assert.Throws<ArgumentOutOfRangeException>("startIndex", () => ContentPaginationHelper<string>.GetRange(-1, 0, (s, t) => _pagedResult));
        }

        [Fact]
        void ItShouldThrowArgumentOutOfRangeException_WhenTopLessThanOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>("top", () => ContentPaginationHelper<string>.GetRange(3, 0, (s, t) => _pagedResult));
        }

        [Fact]
        void ItShouldThrowArgumentOutOfRangeException_WhenNullOfDataLoadingFunc()
        {
            Assert.Throws<ArgumentNullException>("dataLoadingFunc", () => ContentPaginationHelper<string>.GetRange(3, 4, null));
        }

        [Fact]
        void ItShouldFetchAllItemsAtOnce_WhenRequestingMoreItemsThanTotal()
        {
            //This is the last item that does not divided by 2
            var expectedLastItem = 5;
            _query = ContentPaginationHelper<int>.GetRange(0, 30, _dataLoadingFunc);
            Assert.Equal(_originalItems.Where(x => x % 2 != 0).Count(), _query.PagedResult.PagedItems.Count());
            Assert.False(_query.HasMoreContent);
            Assert.Equal(expectedLastItem, _query.PagedResult.PagedItems.Last());
        }

        [Fact]
        void ItShouldExcludeTheInsufficientItems_WhenFunctionRemovesItems()
        {
            _query = ContentPaginationHelper<int>.GetRange(0, 30, _dataLoadingFunc);
            Assert.True(_query.PagedResult.PagedItems.All(x => x % 2 != 0));
        }

        [Fact]
        void ItShouldReturnItemsInDifferentLoadingTimes_WhenDataIsLoadedMultipleTimes_AndEnoughTopItems_ButCanFetchMore()
        {
            var originalItems = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            Func<int, int, PagedResult<IndexedItem<int>>> dataLoadingFunc = (startIndex, top) =>
            {
                var indexedItems = originalItems
                                    .Skip(startIndex)
                                    .Take(top)
                                    .Select((c, i) => new IndexedItem<int> { Item = c, Index = i + startIndex });
                indexedItems = indexedItems.Where(i => i.Index < 2 || i.Index > 7);

                return new PagedResult<IndexedItem<int>>(indexedItems, originalItems.Count());
            };

            _query = ContentPaginationHelper<int>.GetRange(0, 4, dataLoadingFunc);

            Assert.Equal(11, _query.PagedResult.TotalCount);
            Assert.Equal(4, _query.PagedResult.PagedItems.Count());
            Assert.Equal(9, _query.LastIndex);
            Assert.True(_query.HasMoreContent);
        }

        [Fact]
        void ItShouldReturnItemsInDifferentLoadingTimes_WhenDataIsLoadedMultipleTimes_AndNotEnoughTopItems()
        {
            var originalItems = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            Func<int, int, PagedResult<IndexedItem<int>>> dataLoadingFunc = (startIndex, top) =>
            {
                var indexedItems = originalItems
                                    .Skip(startIndex)
                                    .Take(top)
                                    .Select((c, i) => new IndexedItem<int> { Item = c, Index = i + startIndex });
                indexedItems = indexedItems.Where(i => i.Index < 2 || i.Index > 9);

                return new PagedResult<IndexedItem<int>>(indexedItems, originalItems.Count());
            };

            _query = ContentPaginationHelper<int>.GetRange(0, 5, dataLoadingFunc);

            Assert.Equal(11, _query.PagedResult.TotalCount);
            Assert.Equal(3, _query.PagedResult.PagedItems.Count());
            Assert.False(_query.HasMoreContent);
            Assert.Equal(10, _query.LastIndex);
        }

        [Fact]
        void ItShouldReturnItemsInDifferentLoadingTimes_WhenDataIsLoadedMultipleTimes_AndEnoughTopItems_ButCannotFetchMore()
        {
            var originalItems = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            Func<int, int, PagedResult<IndexedItem<int>>> dataLoadingFunc = (startIndex, top) =>
            {
                var indexedItems = originalItems
                                    .Skip(startIndex)
                                    .Take(top)
                                    .Select((c, i) => new IndexedItem<int> { Item = c, Index = i + startIndex });
                indexedItems = indexedItems.Where(i => i.Index < 2 || i.Index > 8);

                return new PagedResult<IndexedItem<int>>(indexedItems, originalItems.Count());
            };

            _query = ContentPaginationHelper<int>.GetRange(0, 3, dataLoadingFunc);

            Assert.Equal(11, _query.PagedResult.TotalCount);
            Assert.Equal(3, _query.PagedResult.PagedItems.Count());
            Assert.True(_query.HasMoreContent);
            Assert.Equal(9, _query.LastIndex);
        }
    }
}
