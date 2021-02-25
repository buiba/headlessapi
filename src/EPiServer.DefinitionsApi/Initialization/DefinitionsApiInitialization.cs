using System.Configuration;
using System.Web.Http;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.Data;
using EPiServer.DataAbstraction.Internal;
using EPiServer.DefinitionsApi.Configuration;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;
using EPiServer.DefinitionsApi.PropertyGroups.Internal;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.Initialization
{
    /// <summary>
    /// Module responsible for initializing the Definitions API
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(DataInitialization))]
    public class DefinitionsApiInitialization : IConfigurableModule
    {
        private PropertyDataTypeResolver _propertyTypeResolver;
        private SiteBasedCorsPolicyService _corsPolicyService;
        private ISiteDefinitionRepository _siteDefinitionRepository;

        /// <inheritdoc />
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services
                .AddSingleton<PropertyDataTypeResolver>()
                .AddSingleton<ContentTypeValidator>()
                .AddSingleton<ExternalContentTypeRepository>()
                .AddSingleton<ContentTypeMapper>()
                .AddSingleton<ContentTypeAnalyzer>()
                .AddSingleton<PropertyGroupRepository>()
                .AddSingleton<IExternalContentTypeServiceExtension, DefaultExternalContentTypeServiceExtension>()
                .AddSingleton<IExternalContentTypeServiceExtension, PropertyValidationContentTypeServiceExtension>();

            context.Services.Forward<DefinitionsApiOptions, IApiAuthorizationOptions>();
        }

        /// <inheritdoc />
        public void Initialize(InitializationEngine context)
        {
            _propertyTypeResolver = context.Locate.Advanced.GetInstance<PropertyDataTypeResolver>();
            PropertyDefinitionTypeRepository.PropertyDefinitionTypeDeleted += PropertyDefinitionTypeChanged;
            PropertyDefinitionTypeRepository.PropertyDefinitionTypeDeleted += PropertyDefinitionTypeChanged;
            _corsPolicyService = context.Locate.Advanced.GetInstance<SiteBasedCorsPolicyService>();
            _siteDefinitionRepository = context.Locate.Advanced.GetInstance<ISiteDefinitionRepository>();

            _siteDefinitionRepository.SiteDefinitionChanged += _corsPolicyService.ClearCache;

            if (MapHttpAttributeRoutes)
            {
                GlobalConfiguration.Configure(config =>
                {
                    config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                    config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

                    config.MapHttpAttributeRoutes();
                    config.EnableCors();
                });
            }
        }

        /// <inheritdoc />
        public void Uninitialize(InitializationEngine context)
        {
            PropertyDefinitionTypeRepository.PropertyDefinitionTypeDeleted -= PropertyDefinitionTypeChanged;
            PropertyDefinitionTypeRepository.PropertyDefinitionTypeDeleted -= PropertyDefinitionTypeChanged;
            _siteDefinitionRepository.SiteDefinitionChanged -= _corsPolicyService.ClearCache;
        }

        private void PropertyDefinitionTypeChanged(object sender, DataAbstraction.RepositoryEventArgs e)
        {
            _propertyTypeResolver.ClearCaches();
        }

        private static bool MapHttpAttributeRoutes
        {
            get
            {
                if (bool.TryParse(ConfigurationManager.AppSettings["episerver:definitionsapi:maphttpattributeroutes"], out var result))
                {
                    return result;
                }

                return true;
            }
        }
    }
}
