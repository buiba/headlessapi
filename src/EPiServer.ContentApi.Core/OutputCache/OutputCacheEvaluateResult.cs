using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace EPiServer.ContentApi.Core.OutputCache
{
    /// <summary>
    /// Represents a result from 
    /// </summary>
    internal class OutputCacheEvaluateResult
    {
        /// <summary>
        /// Creates a <see cref="OutputCacheEvaluateResult"/> for a cachable request
        /// </summary>
        /// <param name="etag">A generated ETag for the <see cref="IOutputCacheEvaluator"/></param>
        /// <param name="dependencyKeys">The dependecy keys for the <see cref="IOutputCacheEvaluator"/></param>
        /// <param name="expires">The expire date for the cached result, if any</param>
        /// <returns>A cachable <see cref="OutputCacheEvaluateResult"/></returns>
        public static OutputCacheEvaluateResult IsCachable(string etag, IEnumerable<string> dependencyKeys, DateTime? expires = null)
            => new OutputCacheEvaluateResult(true, etag, expires, dependencyKeys);

        /// <summary>
        /// Creates a <see cref="OutputCacheEvaluateResult"/> for a none cachable request
        /// </summary>
        public static OutputCacheEvaluateResult NotCachable()
            => new OutputCacheEvaluateResult(false, null, null, Enumerable.Empty<string>());

        /// <summary>
        /// Creates a new instance of <see cref="OutputCacheEvaluateResult"/>
        /// </summary>
        private OutputCacheEvaluateResult(bool isCachable, string eTag, DateTime? expires, IEnumerable<string> dependencyKeys)
        {
            IsCacheable = isCachable;
            ETag = eTag;
            Expires = expires;
            DependencyKeys = dependencyKeys;
        }

        /// <summary>
        /// A calculated ETag for this <see cref="IOutputCacheEvaluator"/>
        /// </summary>
        public string ETag { get; }

        /// <summary>
        /// Indicate whether the request can be cached or not. Returns true if request can be cached else false.
        /// </summary>
        public bool IsCacheable { get; }

        /// <summary>
        /// Returns the time when the result expires (if any)
        /// </summary>
        public DateTime? Expires { get; }

        /// <summary>
        /// A list of all dependencies for this request
        /// </summary>
        public IEnumerable<string> DependencyKeys { get; }
    }   
}
