using EPiServer.Core;
using EPiServer.Web;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    internal class AncestorsContentOutputCacheEvaluator : IOutputCacheEvaluator
    {
        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;
        private readonly IPermanentLinkMapper _permanentLinkMapper;

        public AncestorsContentOutputCacheEvaluator(
            IContentCacheKeyCreator contentCacheKeyCreator,
            IPermanentLinkMapper permanentLinkMapper)
        {
            _contentCacheKeyCreator = contentCacheKeyCreator;
            _permanentLinkMapper = permanentLinkMapper;
        }

        public string HandledType => DependencyTypes.Ancestors;

        public OutputCacheEvaluateResult EvaluateRequest(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, ClaimsIdentity identity)
        {
            if (ParseUriHelper.TryParseContentLinkForRequest(requestMessage.RequestUri, _permanentLinkMapper, "ancestors", out var requestContentLink))
            {
                // Since the contentLink itself is not included in ContentApiTrackingContext.ReferencedContent, we should create common cache key for it
                var dependencies = new HashSet<string> { _contentCacheKeyCreator.CreateCommonCacheKey(requestContentLink) };
                return OutputCacheEvaluateResult.IsCachable(string.Empty, dependencies, null);
            }

            return OutputCacheEvaluateResult.NotCachable();
        }
    }
}
