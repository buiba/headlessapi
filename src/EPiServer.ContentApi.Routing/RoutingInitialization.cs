using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Routing
{
    /// <summary>
    /// Initialize routing for Content Delivery Api
    /// </summary>
    [ModuleDependency(typeof(Web.InitializationModule))]
    public class RoutingInitialization : IInitializableModule
    {
        private RouteInitializationService _routeInitializationService;

        /// <summary>
        /// Initialize routing
        /// </summary>        
        public void Initialize(InitializationEngine context)
        {
            _routeInitializationService = ServiceLocator.Current.GetInstance<RouteInitializationService>();
            _routeInitializationService.Initialize();
        }

        /// <summary>
        /// Dispose the resources used by Content Api's routing
        /// </summary>        
        public void Uninitialize(InitializationEngine context)
        {
            if (_routeInitializationService != null)
            {
                _routeInitializationService.Dispose();
                _routeInitializationService = null;
            }
        }
    }
}
