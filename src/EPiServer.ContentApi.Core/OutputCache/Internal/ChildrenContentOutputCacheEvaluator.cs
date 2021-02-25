using EPiServer.Core;
using EPiServer.Web;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    internal class ChildrenContentOutputCacheEvaluator : IOutputCacheEvaluator
    {
        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;
        private readonly IPermanentLinkMapper _permanentLinkMapper;
        private readonly ContentETagGenerator _contentETagGenerator;
        private readonly ContentLoaderService _contentLoaderService;

        public ChildrenContentOutputCacheEvaluator(
            IContentCacheKeyCreator contentCacheKeyCreator,
            IPermanentLinkMapper permanentLinkMapper,
            ContentETagGenerator contentETagGenerator,
            ContentLoaderService contentLoaderService)
        {
            _contentCacheKeyCreator = contentCacheKeyCreator;
            _permanentLinkMapper = permanentLinkMapper;
            _contentETagGenerator = contentETagGenerator;
            _contentLoaderService = contentLoaderService;
        }

        public string HandledType => DependencyTypes.Children;

        public OutputCacheEvaluateResult EvaluateRequest(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, ClaimsIdentity identity)
        {
            if (ParseUriHelper.TryParseContentLinkForRequest(requestMessage.RequestUri, _permanentLinkMapper, "children", out var requestContentLink))
            {
                var language = requestMessage.Headers.AcceptLanguage?.FirstOrDefault()?.Value;
                var parentContent = _contentLoaderService.Get(requestContentLink, language, true);

                var dependencies = new HashSet<string> { _contentCacheKeyCreator.CreateChildrenCacheKey(requestContentLink, null) };
                return OutputCacheEvaluateResult.IsCachable(_contentETagGenerator.Generate(parentContent), dependencies, null);
            }

            return OutputCacheEvaluateResult.NotCachable();
        }
    }
}
