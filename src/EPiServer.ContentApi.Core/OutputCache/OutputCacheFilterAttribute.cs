using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.Core.OutputCache
{
    /// <summary>
    /// Action Filter reponsible for handling output cache functionality
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>    
    public class OutputCacheFilterAttribute : FilterAttribute, IActionFilter
    {        
        private const string Stale_While_Revalidate = "stale-while-revalidate";
        private readonly Injected<IOutputCacheProvider> _outputCacheProvider;        
        private readonly Injected<IEnumerable<IOutputCacheEvaluator>> _outputCacheEvaluators;
        private readonly Injected<ISynchronizedObjectInstanceCache> _synchronizedObjectInstanceCache;
        private readonly Injected<ContentApiConfiguration> _apiConfiguration;
        private readonly Injected<ContentOptions> _contentOptions;

        public OutputCacheFilterAttribute(string[] Types)
        {
            this.Types = Types;
        }

        /// <summary>
        /// The type of item (e.g. <see cref="DependencyTypes.Content"/>) that will be handled by this filter. 
        /// </summary>
        public string[] Types { get; }

        public override bool AllowMultiple => false;

        public virtual async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext.Request.Method != HttpMethod.Get)
            {
                return await continuation();
            }

            var contentApiOption = _apiConfiguration.Service.Default();

            // Try to get matched etag from If-None-Match header sent in the request
            var etag = TryGetMatchedETag(actionContext.Request);
            if (!string.IsNullOrEmpty(etag))
            {
                return BuildNotModifiedReposnse(actionContext, etag, contentApiOption.HttpResponseExpireTime);
            }            

            var claimsIdentity = actionContext.RequestContext.Principal?.Identity as ClaimsIdentity;

            var cachedResult = await _outputCacheProvider.Service.GetAsync(actionContext.Request, claimsIdentity);
            if (cachedResult.Success)
            {
                return cachedResult.Response;  
            }

            var response = await continuation().ConfigureAwait(true);

            // In case response is not 200 OK, we should early return without handling ETag or cache
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return response;
            }

            var outputCacheEvaluators = _outputCacheEvaluators.Service.Where(s => Types.Contains(s.HandledType)).OrderBy(s => s.GetType().Name);
            if (!outputCacheEvaluators.Any())
            {
                return response;
            }

            var evaluateResults = outputCacheEvaluators.Select(m => m.EvaluateRequest(actionContext.Request, response, claimsIdentity));            
            var combinedEvaluateResult = CombineEvaluateResult(evaluateResults);

            if (combinedEvaluateResult.IsCacheable)
            {
                AddETagAndCacheHeaders(response, actionContext, combinedEvaluateResult, contentApiOption.HttpResponseExpireTime);
            }

            await _outputCacheProvider.Service.SetAsync(actionContext.Request, response, combinedEvaluateResult, claimsIdentity);            

            return response;
        }    

        private HttpResponseMessage BuildNotModifiedReposnse(HttpActionContext actionContext, string etag, TimeSpan httpReponseExpireTime)
        {
            var response = actionContext.Request.CreateResponse(HttpStatusCode.NotModified);            
            response.Headers.ETag = new EntityTagHeaderValue(string.Format("\"{0}\"", etag));
            response.Headers.CacheControl = new CacheControlHeaderValue() { SharedMaxAge = httpReponseExpireTime, Public = true };
            response.Headers.CacheControl.Extensions.Add(new NameValueHeaderValue(Stale_While_Revalidate, "2"));

            return response;
        }
        
        private void AddETagAndCacheHeaders(HttpResponseMessage response, HttpActionContext actionContext, OutputCacheEvaluateResult evaluateResult, TimeSpan httpReponseExpireTime)
        {
            response.Headers.ETag = new EntityTagHeaderValue(string.Format("\"{0}\"", evaluateResult.ETag));
            response.Headers.CacheControl = new CacheControlHeaderValue() { SharedMaxAge = httpReponseExpireTime, Public = true };
            response.Headers.CacheControl.Extensions.Add(new NameValueHeaderValue(Stale_While_Revalidate, "2"));

            if (_synchronizedObjectInstanceCache.Service.Get(evaluateResult.ETag) is null)
            {
                if (evaluateResult.Expires.HasValue && evaluateResult.Expires != null)
                {
                    _synchronizedObjectInstanceCache.Service.Insert(evaluateResult.ETag, new object(), new CacheEvictionPolicy(evaluateResult.Expires.Value - DateTime.UtcNow, CacheTimeoutType.Absolute, evaluateResult.DependencyKeys));
                }
                else
                {
                    _synchronizedObjectInstanceCache.Service.Insert(evaluateResult.ETag, new object(), new CacheEvictionPolicy(_contentOptions.Service.ContentCacheSlidingExpiration, CacheTimeoutType.Sliding, evaluateResult.DependencyKeys));
                }
            }
        }

        private OutputCacheEvaluateResult CombineEvaluateResult(IEnumerable<OutputCacheEvaluateResult> evaluateResults)
        {
            var isCacheable = true;
            DateTime? expires = null;
            var dependencies = new List<string>();

            var etagHashCodeCombiner = HashCodeCombiner.Start();

            foreach (var outputCacheEvaluateResult in evaluateResults)
            {
                isCacheable = isCacheable && outputCacheEvaluateResult.IsCacheable;                 
                etagHashCodeCombiner.Add(outputCacheEvaluateResult.ETag);
                expires = outputCacheEvaluateResult.Expires < expires ? outputCacheEvaluateResult.Expires : expires;
                dependencies.AddRange(outputCacheEvaluateResult.DependencyKeys);
            }

            return isCacheable ? OutputCacheEvaluateResult.IsCachable(etagHashCodeCombiner.CombinedHash.ToString(), dependencies, expires) : OutputCacheEvaluateResult.NotCachable();                
        }     

        private string TryGetMatchedETag(HttpRequestMessage request)
        {
            if (request.Headers.IfNoneMatch.Any())
            {
                // Since etag header must sent with double quote, we should strip the quote before try to get from local cache
                foreach (var etag in request.Headers.IfNoneMatch.Select(h => h.Tag.Replace("\"", "")))
                {                    
                    if (_synchronizedObjectInstanceCache.Service.Get(etag) is object)
                    {
                        return etag;
                    }
                }                  
            }

            return string.Empty;
        }
    }
}
