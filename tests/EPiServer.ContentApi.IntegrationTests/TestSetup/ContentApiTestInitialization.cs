using System;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServerTestInitialization))]
    internal class ContentApiTestInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.ConfigureForExternalTemplates();
            context.Services.Configure<RoutingOptions>(r => r.UrlCacheExpirationTime = TimeSpan.Zero);
            context.Services.Configure<ContentApiConfiguration>(c =>
            {
                c.EnablePreviewFeatures = true;

                c.Default()
                    .SetMinimumRoles(string.Empty)
                    .SetRequiredRole(string.Empty);
            });

            context.Services.AddSingleton<RequestScopedContentApiTrackingContextAccessor>()
                .Forward<RequestScopedContentApiTrackingContextAccessor, IContentApiTrackingContextAccessor>()
                .Forward<RequestScopedContentApiTrackingContextAccessor, IRestRequestInitializer>();
        }

        public void Initialize(InitializationEngine context) { }

        public void Uninitialize(InitializationEngine context) { }
    }
}
