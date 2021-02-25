using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    internal class ContentOutputCacheEvaluator : IOutputCacheEvaluator
    {
        private readonly IContentApiTrackingContextAccessor _contentApiTrackingContextAccessor;
        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;
        private readonly ContentETagGenerator _contentETagGenerator;

        public ContentOutputCacheEvaluator(IContentApiTrackingContextAccessor contentApiTrackingContextAccessor, IContentCacheKeyCreator contentCacheKeyCreator, ContentETagGenerator contentETagGenerator)
        {
            _contentApiTrackingContextAccessor = contentApiTrackingContextAccessor;
            _contentCacheKeyCreator = contentCacheKeyCreator;
            _contentETagGenerator = contentETagGenerator;
        }

        public string HandledType => DependencyTypes.Content;

        public OutputCacheEvaluateResult EvaluateRequest(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, ClaimsIdentity identity)
        {
            var currentContext = _contentApiTrackingContextAccessor.Current;
            if (currentContext.SecuredContent.Any() 
                || currentContext.ReferencedContent.Values.Any(metadata => metadata != null && metadata.PersonalizedProperties.Any()))
            {
                return OutputCacheEvaluateResult.NotCachable();
            }

            var extractedData = ExtractData(currentContext);
            return OutputCacheEvaluateResult.IsCachable(_contentETagGenerator.Generate(requestMessage, currentContext), extractedData.dependencies, extractedData.expires);
        }

        private (IEnumerable<string> dependencies, DateTime? expires) ExtractData(ContentApiTrackingContext context)
        {
            DateTime? expires = null;
            var dependencyKeys = new HashSet<string>();
            foreach (var entry in context.ReferencedContent)
            {
                var languageReference = entry.Key;
                //We always add common dependency so it gets evicted for operations that effect all languages, like Move, Delete etc
                dependencyKeys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(languageReference.ContentLink));
                if (languageReference.Language is object && !CultureInfo.InvariantCulture.Equals(languageReference.Language))
                {
                    //for specifc languages we also add a dependency for that specific language so that when that language is re-published
                    //we can invalidate only that language version and not other languages
                    dependencyKeys.Add(_contentCacheKeyCreator.CreateLanguageCacheKey(languageReference.ContentLink, languageReference.Language.Name));
                }

                var metadata = entry.Value;
                if (metadata != null && metadata.ExpirationTime.HasValue && (!expires.HasValue || metadata.ExpirationTime.Value < expires.Value))
                {
                    expires = metadata.ExpirationTime;
                }
            }

            return (dependencyKeys, expires);
        }
    }
}
