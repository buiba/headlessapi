using System.Web.Http;

namespace EPiServer.ContentManagementApi.Docs
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }
    }
}
