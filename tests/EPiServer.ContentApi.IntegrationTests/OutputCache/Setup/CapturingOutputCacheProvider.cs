using EPiServer.ContentApi.Core.OutputCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache
{
    internal class CapturingOutputCacheProvider : IOutputCacheProvider
    {
        public List<string> CapturedDependencyKeys { get; } = new List<string>();

        public Task<OutputCacheResult> GetAsync(HttpRequestMessage requestMessage, ClaimsIdentity identity)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, OutputCacheEvaluateResult outputCacheEvaluateResult, ClaimsIdentity identity)
        {
            throw new NotImplementedException();
        }

        public void Remove(IEnumerable<string> dependencyKeys) => CapturedDependencyKeys.AddRange(dependencyKeys);


    }
}
