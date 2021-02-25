using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Editor;
using EPiServer.Framework.Modules;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Text.RegularExpressions;

namespace EPiServer.ContentApi.Core.Internal
{
    [ServiceConfiguration(typeof(IContextModeResolver))]
    public class ContextModeResolver : IContextModeResolver
    {
        private static IModuleResourceResolver _moduleResourceResolver;

        /// <summary>
        /// The regular expression to find if we are inside a route that's used by the editorial system.
        /// </summary>
        private static readonly Regex _editRouteRegex = new Regex(",{2}\\d+");

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextModeResolver"/> class.
        /// </summary>
        /// <param name="moduleResourceResolver"></param>
        public ContextModeResolver(IModuleResourceResolver moduleResourceResolver)
        {
            _moduleResourceResolver = moduleResourceResolver;
        }

        /// <summary>
        /// Determines which <see cref="ContextMode"/> the request is executed under
        /// </summary>
        /// <param name="contentUrl">Content URL</param>
        /// <param name="defaultContextMode">Fallback context mode</param>
        /// <returns>
        /// The context mode
        /// </returns>
        public ContextMode Resolve(string contentUrl, ContextMode defaultContextMode)
        {
            var urlBuilder = new UrlBuilder(contentUrl);
            if (IsCmsPath(urlBuilder) == true)
            {
                if (IsEditingActive(urlBuilder))
                {
                    return ContextMode.Edit;
                }
                else if (IsPreviewingActive(urlBuilder))
                {
                    return ContextMode.Preview;
                }
            }
            return defaultContextMode;
        }

        /// <summary>
        /// Determine if the url is inside protected CMS module
        /// </summary>
        /// <param name="urlBuilder"></param>
        private static bool IsCmsPath(UrlBuilder urlBuilder)
        {
            if (_moduleResourceResolver == null)
                return true;

            if (urlBuilder.Path.StartsWith(_moduleResourceResolver.ResolvePath("CMS", null), StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// Determine if URL contains <see cref="PageEditing.EpiEditMode"/>
        /// </summary>
        /// <param name="urlBuilder"></param>
        /// <returns>
        /// The value of <see cref="PageEditing.EpiEditMode"/>
        /// </returns>
        internal static bool IsEditingActive(UrlBuilder urlBuilder)
        {
            if (urlBuilder.QueryCollection[PageEditing.EpiEditMode] != null)
            {
                return urlBuilder.QueryCollection[PageEditing.EpiEditMode].Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// Determine if the request contains a versionId using the <see cref="_editRouteRegex"/> regular expression
        /// </summary>
        /// <param name="urlBuilder"></param>
        internal static bool IsPreviewingActive(UrlBuilder urlBuilder)
        {
            if(!string.IsNullOrEmpty(urlBuilder.Path) && _editRouteRegex.IsMatch(urlBuilder.Path))
            {
                return true;
            }
            return false;
        }
    }
}
