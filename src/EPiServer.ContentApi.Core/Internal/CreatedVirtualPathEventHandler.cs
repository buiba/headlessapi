using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Internal;
using EPiServer.Web.Routing;
using System.Linq;

namespace EPiServer.ContentApi.Core.Internal
{
    internal class CreatedVirtualPathEventHandler
    {
        private IContentLoader _contentLoader;

        public CreatedVirtualPathEventHandler(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        public void Handle(UrlBuilderEventArgs agrs)
        {
            if (agrs.Host == null)
            {
                return;
            }

            var hosts = agrs.Host.Site?.Hosts?.Cast<HostDefinition>();
            var editHost = hosts.FirstOfType(HostDefinitionType.Edit);
            if (editHost != null)
            {
                var contentLink = agrs.RouteValues[RoutingConstants.NodeKey] as ContentReference;
                if (!ContentReference.IsNullOrEmpty(contentLink))
                {
                    var content = _contentLoader.Get<IContent>(contentLink);
                    if (content is IContentMedia)
                    {
                        agrs.Host = editHost;
                    }
                }
            }
        }
    }
}
