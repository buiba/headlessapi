using EPiServer.ContentApi.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace EPiServer.ContentApi.Cms
{
    /// <summary>
    /// Initialize Dependency injection and virtual roles for content api
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class ContentApiCmsInitialization : IConfigurableModule, IInitializableHttpModule, IInitializableModule
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(ContentApiCmsInitialization));
        private readonly Injected<InitializationService> _initService;

        public void ConfigureContainer(ServiceConfigurationContext context)
        {

        }

        /// <summary>
        /// Hook to post authenticate request event
        /// </summary>
        /// <param name="application"></param>
        public void InitializeHttpEvents(HttpApplication application)
        {
            application.PostAuthenticateRequest += application_PostAuthenticateRequest;
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

        /// <summary>
        /// Initialize default http configuration
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(InitializationEngine context)
        {
            if (ContentApiModuleSettings.ShouldUseDefaultHttpConfiguration)
            {
                _logger.Information("Start building default http configuration for Content Delivery Cms.");

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(config => BuildDefaultHttpConfiguration(config));

                _logger.Information("Finish building default http configuration for Content Delivery Cms.");
            }

        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        /// <summary>
        /// Build default http configuration
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
