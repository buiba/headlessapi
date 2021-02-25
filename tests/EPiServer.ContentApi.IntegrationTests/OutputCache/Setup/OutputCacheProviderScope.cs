using EPiServer.ContentApi.Core.OutputCache;
using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.IntegrationTests.OutputCache
{
    internal class OutputCacheProviderScope : IDisposable
    {
        IOutputCacheProvider _defaultProvider;
        public OutputCacheProviderScope(IOutputCacheProvider outputCacheProvider)
        {
            _defaultProvider = ServiceLocator.Current.GetInstance<ContentDependencyPropagator>().OutputCacheProvider;
            ServiceLocator.Current.GetInstance<ContentDependencyPropagator>().OutputCacheProvider = 
                ServiceLocator.Current.GetInstance<SiteDependencyPropagator>().OutputCacheProvider = 
                outputCacheProvider;
        }

        public void Dispose() => ServiceLocator.Current.GetInstance<ContentDependencyPropagator>().OutputCacheProvider =
            ServiceLocator.Current.GetInstance<SiteDependencyPropagator>().OutputCacheProvider =
            _defaultProvider;
    }
}
