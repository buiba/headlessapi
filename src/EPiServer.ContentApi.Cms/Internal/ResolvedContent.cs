using EPiServer.Core;

namespace EPiServer.ContentApi.Cms.Internal
{
    internal class ResolvedContent
    {
        public ResolvedContent(IContent content, string routeLanguage, string remainingRoute = null)
        {
            Content = content;
            RouteLanguage = routeLanguage;
            RemainingRoute = remainingRoute;
        }

        public IContent Content { get; }

        public string RouteLanguage { get; }

        public string RemainingRoute { get; }
    }
}
