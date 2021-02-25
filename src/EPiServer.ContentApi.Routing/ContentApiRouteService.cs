using EPiServer.ContentApi.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.ContentApi.Routing
{
    /// <summary>
    /// Handling common logic for content api routing.
    /// E.g: 
    ///     - Check whether a request related to content delivery api
    ///     - Build rewrite path for a given segment.
    /// </summary>
    [ServiceConfiguration(typeof(ContentApiRouteService))]
    public class ContentApiRouteService
    {
        private readonly IEnumerable<string> _partialSegments;

        public ContentApiRouteService()
        {
            _partialSegments = GetRoutableSegments();
        }

        /// <summary>
        /// Get segments which are handled by content api routing.
        /// Default segments are: children, ancestors.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<string> GetRoutableSegments()
        {
            return new string[]
             {
                "children",  // is mapped to endpoint /api/episerver/{apiVersion}/content/{contentReference}/children
                "ancestors"  // is mapped to endpoint /api/episerver/{apiVersion}/content/{contentReference}/ancestors
             };
        }

        /// <summary>
        /// Check whether a segment should be routed by content api routing.
        /// </summary>
        public virtual bool IsRoutableSegment(string segment)
        {
            return !string.IsNullOrEmpty(segment) && _partialSegments.Any(key => string.Equals(key, segment, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Build rewrite path for a given segment.
        /// For example: when clients make a request to http://localhost:3581/en/alloy-track/children with accept-header value is 'application/json'
        /// then the path is rewritten as http://localhost:3581/api/episerver/{apiVersion}/content/8 
        /// </summary>
        public virtual string BuildRewritePath(SegmentContext routingContext)
        {
            // The root virtual path of the application with no trailing slash (/).
            // if not running under virtual app, ApplicationVirtualPath will return a single slash (/)
            var appPath = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            if (!string.Equals(appPath, "/"))
            {
                // running under virtual app, add a trailing slash (/)
                appPath = appPath + "/";
            }

            foreach (var segment in _partialSegments)
            {
                var routeData = routingContext.GetCustomRouteData<string>(segment);
                if (routeData != null)
                {
                    return $"{appPath}{RouteConstants.VersionTwoApiRoute}content/{routingContext.RoutedContentLink}/{segment}";
                }
            }

            return $"{appPath}{RouteConstants.VersionTwoApiRoute}content/{routingContext.RoutedContentLink}";
        }

        /// <summary>
        /// Determine if the request should be routed.
        /// By default, Requests which have accept headers 'application/json' will be routed.
        /// </summary>
        public virtual bool ShouldRouteRequest(HttpRequestBase request)
        {
            if (request != null && request.AcceptTypes?.Contains(RouteConstants.JsonContentType) == true)
            {
                return true;
            }

            return false;
        }
    }
}
