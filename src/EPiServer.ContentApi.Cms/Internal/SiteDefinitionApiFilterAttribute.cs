using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Cms.Internal
{
    /// <summary>
    /// Action filter that only allows execution of request if SiteDefinitionApiEnabled option
    /// is set to true in the content api options.
    /// Otherwise returns Forbidden
    /// </summary>
    internal class SiteDefinitionApiFilterAttribute : ActionFilterAttribute
    {
        private readonly Injected<ContentApiConfiguration> _apiConfiguration;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var options = _apiConfiguration.Service.Default();

            if (!options.SiteDefinitionApiEnabled)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden,
                    JsonConvert.SerializeObject(new ErrorResponse(new Error(ErrorCode.Forbidden, "Forbidden"))));
            }
        }
    }
}
