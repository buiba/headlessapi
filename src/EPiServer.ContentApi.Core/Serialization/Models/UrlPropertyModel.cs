using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web.Routing;
using Newtonsoft.Json;
using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyUrl"/>
    /// </summary>
    public class UrlPropertyModel : PropertyModel<string, PropertyUrl>
    {
        [Obsolete("Use UrlResolverService instead")]
        protected Injected<IUrlResolver> _urlResolver;

        private readonly Injected<UrlResolverService> _urlResolverService;

        [JsonConstructor]
        internal UrlPropertyModel() { }

        public UrlPropertyModel(PropertyUrl propertyUrl) : base(propertyUrl)
        {
            if (propertyUrl.Url is object)
                Value = _urlResolverService.Service.ResolveUrl(propertyUrl.ToString());
        }
    }
}
