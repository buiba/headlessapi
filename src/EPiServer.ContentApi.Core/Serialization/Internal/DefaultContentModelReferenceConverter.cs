using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    /// <summary>
    /// Provides functionality for converting from IContent or ContentReference to ContentReferenceModel
    /// </summary>
    [ServiceConfiguration(typeof(IContentModelReferenceConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class DefaultContentModelReferenceConverter : IContentModelReferenceConverter
    {
        protected readonly IPermanentLinkMapper _linkMapper;
        protected readonly UrlResolverService _urlResolverService;
        public DefaultContentModelReferenceConverter(IPermanentLinkMapper linkMapper, UrlResolverService urlResolverService)
        {
            _linkMapper = linkMapper;
            _urlResolverService = urlResolverService;
        }

        /// <summary>
        /// Maps Instance of IContent to ContentModelReference. Use language of content to resolve Url property
        /// </summary>
        /// <param name="content">Instance of IContent</param>
        /// <returns>Instance of ContentModelReference based on provided IContent</returns>
        public virtual ContentModelReference GetContentModelReference(IContent content)
        {
            if (content == null)
            {
                return null;
            }
            var localizableContent = content as ILocalizable;
            return new ContentModelReference
            {
                Id = content.ContentLink?.ID,
                GuidValue = content.ContentGuid,
                WorkId = content.ContentLink?.WorkID,
                ProviderName = content.ContentLink?.ProviderName,
                Url = _urlResolverService.ResolveUrl(content.ContentLink, localizableContent?.Language?.Name)
            };
        }

        /// <summary>
        /// Maps Instance of ContentReference to ContentModelReference
        /// </summary>
        /// <param name="contentReference">Instance of IContent</param>
        /// <returns>Instance of ContentModelReference based on provided ContentReference</returns>
        public virtual ContentModelReference GetContentModelReference(ContentReference contentReference)
        {
            if (contentReference == null)
            {
                return null;
            }
            
            return new ContentModelReference
            {
                Id = contentReference.ID,
                GuidValue = _linkMapper.Find(contentReference)?.Guid,
                WorkId = contentReference.WorkID,
                ProviderName = contentReference.ProviderName,
                Url = _urlResolverService.ResolveUrl(contentReference, null)
            };
        }
    }
}
