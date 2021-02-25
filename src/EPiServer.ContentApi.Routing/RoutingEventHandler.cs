using EPiServer.ContentApi.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;
using System;
using System.Web;

namespace EPiServer.ContentApi.Routing
{
    /// <summary>
    ///  Routing Handler is called when an incoming request have been routed to a content instance.
    /// </summary>
    [ServiceConfiguration(typeof(RoutingEventHandler))]
    public class RoutingEventHandler : IDisposable
    {
        private readonly ServiceAccessor<HttpContextBase> _httpContextAccessor;
        private readonly IContentRouteEvents _contentRouteEvents;
        private readonly ContentApiRouteService _contentApiRouteService;

        public RoutingEventHandler(IContentRouteEvents routeEvents, ServiceAccessor<HttpContextBase> httpContextAccessor, ContentApiRouteService contentApiRouteService)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentRouteEvents = routeEvents;
            _contentApiRouteService = contentApiRouteService;
        }

        /// <summary>
        /// Attach event handlers to routed event.
        /// </summary>
        public virtual void AttachEventHandler()
        {
            _contentRouteEvents.RoutedContent += RoutedContent;
        }

        /// <summary>
        /// Handle content routed event raised by Cms.Core.
        /// If the request should be routed, the path will be rewritten and the request then is passed to next route (ContentApi WebApi route)
        /// </summary>
        protected virtual void RoutedContent(object sender, RoutingEventArgs e)
        {
            var httpContext = _httpContextAccessor();
            var request = httpContext?.Request;

            var routingContext = e.RoutingSegmentContext;
            if (_contentApiRouteService.ShouldRouteRequest(request))
            {                
                request.Headers[RouteConstants.AcceptLanguage] = GetLanguage(routingContext, request);
                var path = _contentApiRouteService.BuildRewritePath(routingContext);
                httpContext.RewritePath(path);

                // Set RouteData to null to pass the request to next routes (WebApi route)
                e.RoutingSegmentContext.RouteData = null;
            }
        }

        /// <summary>
        /// Get language from request. Prioritize routed language over accept language header:
        /// (1) If the request url contains a specific language (ex: http://localhost:51059/en/alloy-plan/), then this
        /// language, in this case 'en', should be used
        /// (2) If the request url does not contain any specific language (ex: http://localhost:51059/), then which language value specificed 
        /// in the Accept-Language header should be used        
        /// </summary>
        protected virtual string GetLanguage(SegmentContext routingContext, HttpRequestBase request)
        {
            var language = routingContext.Language ?? routingContext.ContentLanguage ?? string.Empty;
            var acceptLanguageHeader = request.Headers[RouteConstants.AcceptLanguage] ?? string.Empty;

            return string.IsNullOrWhiteSpace(language) ? acceptLanguageHeader : language;
        }

        /// <summary>
        /// Dispose events when the object is disposed
        /// </summary>
        public void Dispose()
        {
            if (_contentRouteEvents != null)
                _contentRouteEvents.RoutedContent -= RoutedContent;
        }
    }
}
