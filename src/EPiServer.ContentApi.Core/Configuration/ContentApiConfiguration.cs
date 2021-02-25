using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Configuration
{
    /// <summary>
    ///     Options class for configuring ContentDelivery settings usage.
    /// </summary>
    [Options]
    public class ContentApiConfiguration
    {
        private readonly ContentApiOptions _defaultOptions;

        public ContentApiConfiguration()
        {
            _defaultOptions = new ContentApiOptions(
                                                         requiredRole: "contentapiread",
                                                         multiSiteFilteringEnabled: false,
                                                         siteDefinitionApiEnabled: true,
                                                         minimumRole: "contentapiread",
                                                         clients: new List<ContentApiClient>
                                                         {
                                                            new ContentApiClient()
                                                            {
                                                                AccessControlAllowOrigin = "*",
                                                                ClientId="Default"
                                                            }
                                                         });
            _defaultOptions.SetHttpResponseExpireTime(TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Set to true to enable preview features. Default value is false.
        /// </summary>
        public bool EnablePreviewFeatures { get; set; } = false;

        /// <summary>
        /// Get default options for content api. Default() should be used at initialization time to customize/config default value
        /// </summary>
        public ContentApiOptions Default()
        {
            return _defaultOptions;
        }

        /// <summary>
        /// Get default options for the content api for specified <see cref="RestVersion"/>. Should be used at initialization time to customize/config default value
        /// </summary>
        [Obsolete("Use the Default method without restVersion parameter")]
        public ContentApiOptions Default(RestVersion restVersion) => Default();

        /// <summary>
        /// Get content api options. GetOptions() should be used in application for api calling. 
        /// By using this function, Default value will not be changed if clients using api change some values
        /// </summary>
        public ContentApiOptions GetOptions()
        {
            return _defaultOptions.Clone() as ContentApiOptions;
        }

        /// <summary>
        /// Get content api options. Should be used in application for api calling. 
        /// By using this function, Default value will not be changed if clients using api change some values
        /// </summary>
        [Obsolete("Use the GetOptions method without restVersion parameter")]
        public ContentApiOptions GetOptions(RestVersion restVersion) => GetOptions();
    }
}
