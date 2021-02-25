using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    ///     Serializable model class for EPiServer.SpecializedProperties.LinkItem 
    /// </summary>
    public class LinkItemNode : IContentItem
    {
        protected readonly Injected<IContentModelReferenceConverter> _contentModelService;
        protected readonly Injected<IUrlResolver> _urlResolver;
        protected readonly Injected<UrlResolverService> _urlResolverService;

        //expose for testing purpose
        internal LinkItemNode()
        {
        }

        public LinkItemNode(string href, string title, string target, string text)
        {
            Href = href;
            Title = title;
            Target = target;
            Text = text;

            var urlBuilder = new UrlBuilder(href);
            var content = _urlResolver.Service.Route(urlBuilder);
            if (content != null)
            {
                ContentLink = _contentModelService.Service.GetContentModelReference(content);
                Href = _urlResolverService.Service.ResolveUrl(content.ContentLink, urlBuilder.QueryLanguage);
            }
        }

        public string Href { get; set; }
        public string Title { get; set; }
        public string Target { get; set; }
        public string Text { get; set; }
        public ContentModelReference ContentLink { get; set ; }        
    }
}
