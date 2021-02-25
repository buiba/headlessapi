using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.Core.Tracking.Internal;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System.Web;
using EPiServer.ContentApi.Core.Security.Internal;

namespace EPiServer.ContentApi.Core.Internal
{
    [InitializableModule]
    public class ContentApiCoreInitialization : IConfigurableModule
    {
        private IContentRouteEvents _contentRouteEvents;
        private CreatedVirtualPathEventHandler _createdVirtualPathEventHandler;
        private CorsPolicyService _corsPolicyService;
        private ISiteDefinitionRepository _siteDefinitionRepository;

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<BlockPropertyConverterProvider>();
            context.Services.AddSingleton<IPropertyConverterProvider>(s => s.GetInstance<BlockPropertyConverterProvider>());
            context.Services.AddSingleton<JsonSerializer>();
            context.Services.AddSingleton<IContentApiSerializer>(s => s.GetInstance<JsonSerializer>());
            context.Services.AddSingleton<IJsonSerializerConfiguration>(s => s.GetInstance<JsonSerializer>());            

            context.ConfigurationComplete += (o, e) =>
            {
                e.Services.TryAdd<IContentApiTrackingContextAccessor, HttpContextContentApiTrackingContextAccessor>(ServiceInstanceScope.Singleton);
            };
        }

        public void Initialize(InitializationEngine context)
        {
            context.Locate.Advanced.GetInstance<BlockPropertyConverterProvider>().Initialize(context.Locate.Advanced.GetInstance<IPropertyConverterResolver>());
            
            _contentRouteEvents = context.Locate.Advanced.GetInstance<IContentRouteEvents>();
            _createdVirtualPathEventHandler = new CreatedVirtualPathEventHandler(
                context.Locate.Advanced.GetInstance<IContentLoader>());
            _contentRouteEvents.CreatedVirtualPath += contentRouteEvents_CreatedVirtualPath;

            SetDefaultApiOptions(context);

            _corsPolicyService = context.Locate.Advanced.GetInstance<CorsPolicyService>();
            _siteDefinitionRepository = context.Locate.Advanced.GetInstance<ISiteDefinitionRepository>();

            _siteDefinitionRepository.SiteDefinitionChanged += _corsPolicyService.ClearCache;
        }

        private void contentRouteEvents_CreatedVirtualPath(object sender, UrlBuilderEventArgs e)
        {
            _createdVirtualPathEventHandler.Handle(e);
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentRouteEvents.CreatedVirtualPath -= contentRouteEvents_CreatedVirtualPath;

            _siteDefinitionRepository.SiteDefinitionChanged -= _corsPolicyService.ClearCache;
        }

        private static void SetDefaultApiOptions(InitializationEngine context)
        {
            var options = context.Locate.Advanced.GetInstance<ExternalApplicationOptions>();
            if (options.OptimizeForDelivery)
            {
                var defaultContentApiOptions = context.Locate.Advanced.GetInstance<ContentApiConfiguration>().Default();
                defaultContentApiOptions
                    .SetValidateTemplateForContentUrl(false)
                    .SetFlattenPropertyModel(true)
                    .SetEnablePreviewMode(true)
                    .SetIncludeNullValues(false)
                    .SetIncludeMasterLanguage(false);
            }
        }
    }
}
