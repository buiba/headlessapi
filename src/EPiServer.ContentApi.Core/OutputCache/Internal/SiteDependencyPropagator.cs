using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    internal class SiteDependencyPropagator
    {
        private readonly ISynchronizedObjectInstanceCache _synchronizedObjectInstanceCache;
        public SiteDependencyPropagator(ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache, IOutputCacheProvider outputCacheProvider)
        {
            _synchronizedObjectInstanceCache = synchronizedObjectInstanceCache;
            OutputCacheProvider = outputCacheProvider;
        }

        //Exposed for test
        internal IOutputCacheProvider OutputCacheProvider { get; set; }

        internal void SiteDefinitionCreated(object sender, SiteDefinitionEventArgs e)
        {
            EvictCommonKey();
        }

        internal void SiteDefinitionUpdated(object sender, SiteDefinitionEventArgs e)
        {
            EvictCommonKey();
            EvictSpecificKey($"{ SiteOutputCacheEvaluator.SiteDependency}{e.Site.Id}");
        }

        internal void SiteDefinitionDeleted(object sender, SiteDefinitionEventArgs e)
        {
            EvictCommonKey();
            EvictSpecificKey($"{ SiteOutputCacheEvaluator.SiteDependency}{e.Site.Id}");
        }

        private void EvictCommonKey() 
        {
            // evict etag for "get list of definition";
            _synchronizedObjectInstanceCache.Remove($"{SiteOutputCacheEvaluator.SiteDependency}{ReferencedSiteMetadata.DefaultInstance.Id}");
            OutputCacheProvider.Remove($"{SiteOutputCacheEvaluator.SiteDependency}{ReferencedSiteMetadata.DefaultInstance.Id}");
        }

        private void EvictSpecificKey(string key)
        {
            // evict etag for a single site;
            _synchronizedObjectInstanceCache.Remove(key);
            OutputCacheProvider.Remove(key);
        }
    }
}
