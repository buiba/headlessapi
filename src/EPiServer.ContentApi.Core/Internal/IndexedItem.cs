namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Represent item in a list with its index.
    /// </summary>
    public class IndexedItem<T>
    {
        /// <summary>
        /// Index of the item.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The item in list.
        /// </summary>
        public T Item { get; set; }
    }
}
