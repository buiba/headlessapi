using EPiServer.Core;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// The next token contains the information that needs to get the next page
    /// </summary>
    public class PagingToken
    {
        /// <summary>
        /// The last index of the current batch
        /// </summary>
        public int? LastIndex { get; set; }

        /// <summary>
        /// The amount of content is requested
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Total item count of the data
        /// </summary>
        public int? TotalCount { get; set; }
    }
}
