using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Cms.Internal
{
    [ServiceConfiguration(typeof(ContentResolver), IncludeServiceAccessor = false)]
    internal class ContentResolver
    {
        private readonly Core.IContextModeResolver _contextModeResolver;
        private readonly UrlResolver _urlResolver;

        public ContentResolver(UrlResolver urlResolver, Core.IContextModeResolver contextModeResolver)
        {
            _contextModeResolver = contextModeResolver;
            _urlResolver = urlResolver;
        }

        // If making this public - replace optional parameters with a ContentResolverOptions class
        public ResolvedContent Resolve(string contentUrl, bool matchExact = true, bool allowPreview = false)
        {
            var contextMode = _contextModeResolver.Resolve(contentUrl, ContextMode.Default);
            var routeArgument = new RouteArguments() { ContextMode = contextMode, MatchContentRouteConstraints = false };

            if (!allowPreview && contextMode.EditOrPreview())
            {
                // Never resolve Edit/Preview content if not allowed
                return null;
            }

            var matched = _urlResolver.Route(contentUrl, routeArgument);

            if (matched?.Content is null)
            {
                // No match was found
                return null;
            }

            // remainingPath is null mean that contentUrl is exactly matched
            if (string.IsNullOrEmpty(matched.RemainingPath))
            {
                // We found an exact match
                return new ResolvedContent(matched.Content, matched.RouteLanguage);
            }

            if (matchExact || contextMode.EditOrPreview())
            {
                // Partial matches are not possible with the current Edit/Preview format
                return null;
            }

            var remainingPath = matched.RemainingPath.StartsWith("/") ? matched.RemainingPath : "/" + matched.RemainingPath;

            // Return partially matched content together with the remaining path
            return new ResolvedContent(matched.Content, matched.RouteLanguage, remainingPath);            
        }
    }
}
