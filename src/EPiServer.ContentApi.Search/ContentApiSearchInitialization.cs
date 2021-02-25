using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Find.Framework;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.OData.Query;

namespace EPiServer.ContentApi.Search
{
    /// <summary>
    /// Initialize default settings for search
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class ContentApiSearchInitialization : IConfigurableModule, IInitializableHttpModule, IInitializableModule
    {
        protected readonly Injected<FindEventsAssociationService> _associationService;
        protected readonly Injected<InitializationService> _initService;
        protected readonly ILogger _logger = LogManager.GetLogger(typeof(ContentApiSearchInitialization));

        /// <summary>
        /// Inititalize DJ
        /// </summary>
        /// <param name="context"></param>
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton(new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.ToLower | AllowedFunctions.Contains | AllowedFunctions.Any,
                AllowedLogicalOperators = AllowedLogicalOperators.LessThanOrEqual | AllowedLogicalOperators.LessThan |
                    AllowedLogicalOperators.GreaterThan | AllowedLogicalOperators.GreaterThanOrEqual |
                    AllowedLogicalOperators.Equal | AllowedLogicalOperators.NotEqual |
                    AllowedLogicalOperators.And | AllowedLogicalOperators.Or,
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                AllowedQueryOptions = AllowedQueryOptions.Filter
            });
            context.Services.AddSingleton<IFindODataParser, FindODataParser>();
            context.Services.AddScoped<IContentApiSearchProvider, FindContentApiSearchProvider>();
        }

        /// <summary>
        /// Hook to post authenticatereqeust event
        /// </summary>
        /// <param name="application"></param>
        public void InitializeHttpEvents(HttpApplication application)
        {
            application.PostAuthenticateRequest += application_PostAuthenticateRequest;
        }

        /// <summary>
        /// Initialize search and http configuration if enabled.
        /// </summary>
        public void Initialize(InitializationEngine context)
        {
            InitializeContentApiSearch();
            _associationService.Service.Initialize();

            if (ContentApiSearchModuleSettings.ShouldUseDefaultHttpConfiguration)
            {
                _logger.Information("Start building default http configuration for Content Delivery Search.");

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(config => BuildDefaultHttpConfiguration(config));

                _logger.Information("Finish building default http configuration for Content Delivery Search.");
            }
        }

        /// <summary>
        /// Handles the PostAuthenticateRequest event of the application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            _initService.Service.InitializeVirtualRoles();
        }

        public void Uninitialize(InitializationEngine context)
        {
            
        }

        /// <summary>
        /// Initialize SearchClient
        /// </summary>
        /// <returns></returns>
        protected virtual void InitializeContentApiSearch()
        {
            SearchClient.Instance.Conventions.InitializeContentApiSearch();
        }

        /// <summary>
        /// Get default http configuration
        /// </summary>
        private HttpConfiguration BuildDefaultHttpConfiguration(HttpConfiguration config)
        {
            if (config == null)
            {
                config = new HttpConfiguration();
            }

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();
            config.Formatters.XmlFormatter.UseXmlSerializer = true;
            config.MapHttpAttributeRoutes();
            config.EnableCors();

            return config;
        }
    }
}
