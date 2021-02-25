using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Handling Content Api url resolver logic
    /// </summary>
    [ServiceConfiguration(typeof(UrlResolverService))]
    public class UrlResolverService
    {
        protected readonly UrlResolver _urlResolver;
        private readonly ContentApiConfiguration _contentApiConfiguration;

        /// <internal-api/>  //For easier mocking
        protected UrlResolverService() { }
                
        public UrlResolverService(UrlResolver urlResolver, ContentApiConfiguration contentApiConfiguration)
        {
            _urlResolver = urlResolver;
            _contentApiConfiguration = contentApiConfiguration;
        }

        [Obsolete("Use constructor with ContentApiConfiguration parameter instead")]
        public UrlResolverService(UrlResolver urlResolver, OptionsResolver optionsResolver, ApiRequestContext apiRequestContext)
            : this (urlResolver, ServiceLocator.Current.GetInstance<ContentApiConfiguration>())
        {            
        }

        /// <summary>
        /// Resolve url by content link and language. By default, this function always resolve url using view mode context 
        /// </summary>        
        public virtual string ResolveUrl(ContentReference contentLink, string language)
        {
            var options = _contentApiConfiguration.Default();
            return _urlResolver.GetUrl(contentLink, language, new VirtualPathArguments
            {
                ContextMode = Web.ContextMode.Default,
                ForceCanonical = true,
                ForceAbsolute = options.ForceAbsolute,
                ValidateTemplate = options.ValidateTemplateForContentUrl
            });
        }

        /// <summary>
        /// Resolve an internal link.
        /// </summary>        
        public virtual string ResolveUrl(string internalLink)
        {
            var options = _contentApiConfiguration.Default();
            return _urlResolver.GetUrl(new UrlBuilder(internalLink), new VirtualPathArguments
            {
                ContextMode = Web.ContextMode.Default,
                ForceCanonical = true,
                ForceAbsolute = options.ForceAbsolute,
                ValidateTemplate = options.ValidateTemplateForContentUrl
            });
        }
    }
}
