using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Action filter that only allows execution of request if EnablePreviewFeatures is
    /// set to true in the content api configuration.
    /// Otherwise returns Forbidden
    /// </summary>
    public class PreviewFeatureFilterAttribute : ActionFilterAttribute
    {
        private readonly Injected<ContentApiConfiguration> _contentApiConfiguration;        

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!_contentApiConfiguration.Service.EnablePreviewFeatures)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden,
                    JsonConvert.SerializeObject(new ErrorResponse(new Error(ErrorCode.Forbidden, $"{nameof(ContentApiConfiguration.EnablePreviewFeatures)} must be set to true to use requested endpoint"))));
            }
        }
    }
}