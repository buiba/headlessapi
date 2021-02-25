using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;
using System.Web;
using System.Web.Routing;

namespace EPiServer.ContentApi.Routing
{
    /// <summary>
    /// Content Api's partial router for handling extend routing on IContent.
    /// An example would be an url like 'http://localhost/en/alloy-plan/{extendedPart}'.
    /// If 'http://localhost/en/alloy-plan/' url can be matched to a page (IContent), then this router 
    /// will be called to handle the {extendedPart} segment     
    /// </summary>
    [ServiceConfiguration(typeof(ContentApiPartialRouter))]
    public class ContentApiPartialRouter : IPartialRouter<IContent, IContent>
    {
        private readonly ServiceAccessor<HttpContextBase> _httpContextAccessor;
        private readonly ContentApiRouteService _contentApiRouteService;

        public ContentApiPartialRouter(ServiceAccessor<HttpContextBase> httpContextAccessor, ContentApiRouteService contentApiRouteService)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentApiRouteService = contentApiRouteService;
        }

        /// <summary>
        /// Handle routing of partial segments. 
        /// For example, when ContentApiPartialRouter is called to handle an url like 'http://localhost/en/alloy-plan/children',
        /// it will call this method to route the 'children' segment
        /// </summary>        
        public virtual object RoutePartial(IContent content, SegmentContext segmentContext)
        {
            var context = _httpContextAccessor();
            var request = context?.Request;

            if (_contentApiRouteService.ShouldRouteRequest(request))
            {
                var nextSegment = segmentContext.GetNextValue(segmentContext.RemainingPath);
                if (_contentApiRouteService.IsRoutableSegment(nextSegment.Next))
                {
                    segmentContext.SetCustomRouteData(nextSegment.Next, bool.TrueString);
                    segmentContext.RemainingPath = string.Empty; // We already handle the segment, so remove it from remaining path
                    return content;
                }                
            }

            return null;
        }

        /// <summary>
        /// Gets a partial virtual path for a content item during routing. In Content Api, we dont need to use this method so
        /// we just return null by default
        /// </summary>
        public virtual PartialRouteData GetPartialVirtualPath(IContent content, string language, RouteValueDictionary routeValues, RequestContext requestContext)
        {
            return null;
        }
    }
}