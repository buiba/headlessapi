using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Routing
{
    /// <summary>
    /// Responsible for initialization of Content Api routing
    /// </summary>
    [ServiceConfiguration(typeof(RouteInitializationService))]
    public class RouteInitializationService
    {
        private RoutingEventHandler _routingEventHandler;

        /// <summary>
        /// Attach routing event handler and register Content Api's partial router
        /// </summary>
        public virtual void Initialize()
        {
            _routingEventHandler = ServiceLocator.Current.GetInstance<RoutingEventHandler>();
            _routingEventHandler.AttachEventHandler();

            Global.RoutesRegistered += (o, e) =>
            {
                e.Routes.RegisterPartialRouter(ServiceLocator.Current.GetInstance<ContentApiPartialRouter>());
            };
        }

        /// <summary>
        /// Dispose of the resources that used by this service
        /// </summary>
        public virtual void Dispose()
        {
            if (_routingEventHandler != null)
            {
                _routingEventHandler.Dispose();
                _routingEventHandler = null;
            }
        }
    }
}
