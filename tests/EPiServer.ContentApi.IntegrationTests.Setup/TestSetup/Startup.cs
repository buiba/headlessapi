using System.Web.Http;
using Owin;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();
            configuration.EnableCors();
            configuration.Filters.Add(new ImpersonationActionFilter());
            configuration.Filters.Add(new RestRequestInitializerActionFilter());
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings
                = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
            app.UseWebApi(configuration);
        }
    }
}
