using System.Web.Http;
using Owin;

namespace EPiServer.ContentApi.Error.Infrastructure
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            app.UseWebApi(configuration);
        }
    }
}
