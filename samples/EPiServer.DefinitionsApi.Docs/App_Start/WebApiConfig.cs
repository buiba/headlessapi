using System.Web.Http;

namespace EPiServer.DefinitionsApi.Docs
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }
    }
}
