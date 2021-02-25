using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.Core.Security.Internal
{
    /// <summary>
    ///     Action Filter which returns 200 Response for OPTIONS Requests. CORs requests will hit this action filter only if they have a valid origin
    /// </summary>
    public class CorsOptionsActionFilter : ActionFilterAttribute
    {
		/// <inheritdoc />
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Method == HttpMethod.Options)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.OK
                );
            }
        }
    }
}