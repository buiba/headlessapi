using System.Configuration;
using System.Web.Http;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.ContentManagementApi.Configuration;
using EPiServer.ContentManagementApi.Serialization;
using EPiServer.ContentManagementApi.Serialization.Internal;
using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentManagementApi.Initialization
{
    /// <summary>
    /// Module responsible for initializing the Content Management API
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(DataInitialization))]
    internal class ContentManagementApiInitialization : IConfigurableModule
    {
        private SiteBasedCorsPolicyService _corsPolicyService;
        private ISiteDefinitionRepository _siteDefinitionRepository;

        /// <inheritdoc />
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<PropertyBlockValueConverterProvider>();
            context.Services.AddSingleton<IPropertyDataValueConverterProvider>(s => s.GetInstance<PropertyBlockValueConverterProvider>());
            context.Services.AddSingleton<DefaultPropertyDataValueConverterProvider>();
            context.Services.AddSingleton<IPropertyDataValueConverterProvider>(s => s.GetInstance<DefaultPropertyDataValueConverterProvider>());
            context.Services.Forward<ContentManagementApiOptions, IApiAuthorizationOptions>();
        }

        /// <inheritdoc />
        public void Initialize(InitializationEngine context)
        {
            context.Locate.Advanced.GetInstance<PropertyBlockValueConverterProvider>()
                .Initialize(context.Locate.Advanced.GetInstance<IPropertyDataValueConverterResolver>());
            context.Locate.Advanced.GetInstance<DefaultPropertyDataValueConverterProvider>()
               .RegisterConverters();

            _corsPolicyService = context.Locate.Advanced.GetInstance<SiteBasedCorsPolicyService>();
            _siteDefinitionRepository = context.Locate.Advanced.GetInstance<ISiteDefinitionRepository>();

            _siteDefinitionRepository.SiteDefinitionChanged += _corsPolicyService.ClearCache;

            if (MapHttpAttributeRoutes)
            {
                GlobalConfiguration.Configure(config =>
                {
                    
                    config.MapHttpAttributeRoutes();
                    config.EnableCors();
                });
            }

            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            var converters = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters;
            converters.Add(new ContentApiPatchModelJsonConverter());
        }

        /// <inheritdoc />
        public void Uninitialize(InitializationEngine context)
        {
            _siteDefinitionRepository.SiteDefinitionChanged -= _corsPolicyService.ClearCache;
        }

        private static bool MapHttpAttributeRoutes
        {
            get
            {
                if (bool.TryParse(ConfigurationManager.AppSettings["episerver:contentmanagementapi:maphttpattributeroutes"], out var result))
                {
                    return result;
                }

                return true;
            }
        }
    }
}
