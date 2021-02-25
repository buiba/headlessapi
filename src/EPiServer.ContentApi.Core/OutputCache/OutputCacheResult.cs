using System.Net.Http;

namespace EPiServer.ContentApi.Core.OutputCache
{
    /// <summary>
    /// Represents a output cache result of <see cref="IOutputCacheProvider"/>
    /// </summary>
    internal class OutputCacheResult
    {
        /// <summary>
        /// Creates a new instance of <see cref="OutputCacheResult"/>
        /// </summary>
        public OutputCacheResult(bool success, HttpResponseMessage response)
        {
            Success = success;
            Response = response;
        }

        /// <summary>
        /// Indicate whether the cache result is hit or not. True if the cache hit, otherwise false.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// The Http response message that will be returned in case the cache is hit.
        /// </summary>
        public HttpResponseMessage Response { get; }
    }   
}
