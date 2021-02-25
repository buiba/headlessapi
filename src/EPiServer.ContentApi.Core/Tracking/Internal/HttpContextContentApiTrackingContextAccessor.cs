using EPiServer.ServiceLocation;
using System.Web;

namespace EPiServer.ContentApi.Core.Tracking.Internal
{
    internal class HttpContextContentApiTrackingContextAccessor : IContentApiTrackingContextAccessor
    {
        internal const string ContextKey = "epi-contentapi-tracking";
        private readonly ServiceAccessor<HttpContextBase> _httpContextAccessor;

        public HttpContextContentApiTrackingContextAccessor(ServiceAccessor<HttpContextBase> httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ContentApiTrackingContext Current
        {
            get
            {
                var httpContext = _httpContextAccessor();
                if (httpContext != null)
                {
                    if (!(httpContext.Items[ContextKey] is ContentApiTrackingContext context))
                    {
                        context = new ContentApiTrackingContext();
                        httpContext.Items[ContextKey] = context;
                    }
                    return context;
                }
                //we return new instance here to avoid null checks by callers
                return new ContentApiTrackingContext();
            }
        }
    }
}
