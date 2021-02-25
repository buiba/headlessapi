using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Framework.Cache;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    internal class SiteOutputCacheEvaluator : IOutputCacheEvaluator
    {
        internal const string SiteDependency = "ep:o:site:";
        private readonly ISynchronizedObjectInstanceCache _synchronizedObjectInstanceCache;
        private readonly SiteETagGenerator _sitesETagGenerator;
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;
        private readonly IContentApiTrackingContextAccessor _contentApiTrackingContextAccessor;

        public SiteOutputCacheEvaluator(ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache, SiteETagGenerator sitesETagGenerator, ISiteDefinitionRepository siteDefinitionRepository, IContentApiTrackingContextAccessor contentApiContextAccessor)
        {
            _synchronizedObjectInstanceCache = synchronizedObjectInstanceCache;
            _sitesETagGenerator = sitesETagGenerator;
            _siteDefinitionRepository = siteDefinitionRepository;
            _contentApiTrackingContextAccessor = contentApiContextAccessor;
        }

        public string HandledType => DependencyTypes.Site;

        public OutputCacheEvaluateResult EvaluateRequest(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, ClaimsIdentity identity)
        {
            var siteMetaDataList = _contentApiTrackingContextAccessor.Current.ReferencedSites;
            if (siteMetaDataList.Count == 0) 
            {
                return OutputCacheEvaluateResult.NotCachable();
            }

            var siteKeys = siteMetaDataList.Select(meta => $"{SiteDependency}{meta.Id}");
            EnsureDependencies(siteKeys);

            return OutputCacheEvaluateResult.IsCachable(_sitesETagGenerator.Generate(siteMetaDataList), siteKeys);
        }

        private void EnsureDependencies(IEnumerable<string> siteKeys)
        {
            foreach (var key in siteKeys)
            {
                if (_synchronizedObjectInstanceCache.Get(key) is null)
                {
                    _synchronizedObjectInstanceCache.Insert(key, new object(), CacheEvictionPolicy.Empty);
                }
            }
        }
    }
}
