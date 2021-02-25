using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IOutputCacheProvider"/>
    /// </summary>
    internal class DefaultOutputCacheProvider : IOutputCacheProvider
    {
        public Task<OutputCacheResult> GetAsync(HttpRequestMessage requestMessage, ClaimsIdentity identity)
        {
            return Task.FromResult(new OutputCacheResult(false, null));
        }

        public Task SetAsync(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, OutputCacheEvaluateResult outputCacheEvaluateResult, ClaimsIdentity identity)
        {
            return Task.CompletedTask;
        }

        public void Remove(IEnumerable<string> dependencyKeys)
        {
            // For this provider, we don't need to remove anythings. CMS cache automatically 
            // clean cache for us in case it 's no longer valid due to changes (e.g. security changes, or moving)
        }

    }
}
