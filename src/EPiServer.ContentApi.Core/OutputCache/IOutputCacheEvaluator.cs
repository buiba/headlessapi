using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.OutputCache
{
    /// <summary>
    /// Defines signature for a component that evaluates if current request can be cached
    /// </summary>
    internal interface IOutputCacheEvaluator
    {
        /// <summary>
        /// The type of item (e.g. Content or Site) that this collector handles
        /// </summary>
        string HandledType { get; }

        /// <summary>
        /// Evaluates if current <paramref name="responseMessage"/> can be cached
        /// </summary>
        /// <param name="requestMessage">The current request</param>
        /// <param name="responseMessage">The current response</param>
        /// <param name="identity">The current user</param>
        /// <returns>An <see cref="OutputCacheEvaluateResult"/> that specifies if the response can be cached</returns>
        OutputCacheEvaluateResult EvaluateRequest(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, ClaimsIdentity identity);
    }
}
