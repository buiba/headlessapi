using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.OutputCache
{
    /// <summary>
    /// Signature for component that provides the output caching feature
    /// </summary>
    internal interface IOutputCacheProvider
    {       
        /// <summary>
        /// Get the <see cref="OutputCacheResult"/> given the request and identity
        /// </summary>
        /// <param name="requestMessage">The current request</param>
        /// <param name="identity">The current identity</param>        
        Task<OutputCacheResult> GetAsync(HttpRequestMessage requestMessage, ClaimsIdentity identity);

        /// <summary>
        /// Set the cache entry 
        /// </summary>
        /// <param name="requestMessage">The current request</param>
        /// <param name="responseMessage">The current response</param>
        /// <param name="outputCacheEvaluateResult">The result of the evaluation from <see cref="IOutputCacheEvaluator"/> instances</param>
        /// <param name="identity">The current identity</param>        
        Task SetAsync(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, OutputCacheEvaluateResult outputCacheEvaluateResult, ClaimsIdentity identity);
        
        /// <summary>
        /// Removes all entries that has any dependency in <paramref name="dependencyKeys"/>
        /// </summary>
        /// <param name="dependencyKeys">Cache keys for the effected content items</param>        
        void Remove(IEnumerable<string> dependencyKeys);
    }   

    /// <summary>
    /// Extends <see cref="IOutputCacheProvider"/> with convenient methods
    /// </summary>
    internal static class OutputCacheProviderExtensions
    {
        /// <summary>
        /// Removes all entries that has a dependency to <paramref name="dependencyKey"/>
        /// </summary>
        /// <param name="outputCacheProvider"></param>
        /// <param name="dependencyKey">Cache key for the effected content item</param>         
        public static void Remove(this IOutputCacheProvider outputCacheProvider, string dependencyKey) => outputCacheProvider.Remove(new[] { dependencyKey });
    }
}
