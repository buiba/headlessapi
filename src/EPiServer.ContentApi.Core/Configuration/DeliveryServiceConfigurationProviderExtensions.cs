using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ServiceLocation
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceConfigurationProvider"/>
    /// </summary>
    public static class DeliveryServiceConfigurationProviderExtensions
    {
        /// <summary>
        /// Configures CMS and ContentDeliveryAPI to be used from external templates application.
        /// </summary>
        /// <param name="services">The service provider extended</param>
        /// <returns>The service provider</returns>
        public static IServiceConfigurationProvider ConfigureForExternalTemplates(this IServiceConfigurationProvider services)
        {
            services.Configure<RoutingOptions>(r => r.ConfigureForExternalTemplates());
            services.Configure<ContentApiConfiguration>(config =>
            {
                config.Default().SetValidateTemplateForContentUrl(false);
            });

            return services;
        }

        /// <summary>
        /// Configures some general options for Content Delivery client.
        /// </summary>
        /// <para>
        /// Preview API: This API is current in preview state meaning it might change between minor versions
        /// </para>
        /// <param name="services">The service provider extended</param>
        /// <returns>The service provider</returns>
        public static IServiceConfigurationProvider ConfigureForContentDeliveryClient(this IServiceConfigurationProvider services)
        {
            services.Configure<ContentApiConfiguration>(config =>
            {
                config.Default()
                        .SetValidateTemplateForContentUrl(false)
                        .SetFlattenPropertyModel(true)
                        .SetEnablePreviewMode(true)
                        .SetIncludeNullValues(false)
                        .SetIncludeMasterLanguage(false)
                        .SetForceAbsolute(true)
                        .SetIncludeInternalContentRoots(false)
                        .SetIncludeSiteHosts(false)
                        .SetExpandedBehavior(ExpandedLanguageBehavior.ContentLanguage)
                        .SetIncludeNumericContentIdentifier(false);
            });

            return services;
        }
    }
}
