using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ServiceLocation;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace EPiServer.ContentApi.Core.Security.Internal
{
	/// <summary>
	///      Content Api attribute with enforces the MinimumRoles property on the <see cref="ContentApiOptions"/>
	///      This is used to set which minimum roles (if any) are required to hit the Content Search Api
	/// </summary>
	public class ContentApiAuthorizationAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Service for handling authorization logic
        /// </summary>
        public Injected<ContentApiAuthorizationService> AuthorizationService;
     
        /// <summary>
        /// Service for handler Json Serialize
        /// </summary>
        public Injected<ContentResultService> ContentBuilderService;

		/// <summary>
		/// Service for initializing and accessing current <see cref="IPrincipal"/>
		/// </summary>
		public Injected<ISecurityPrincipal> PrincipalAccessor;

		/// <summary>        
		/// Authorizing user identity using AuthorizationService before forwarding the request to controller.         
		/// When it come to this method, the request has been go through OWIN pipeline
		/// and the token has been parsed. User information and claims can be retrieved from Principal object of current context
		/// </summary>
		/// <param name="actionContext"></param>
		public override void OnAuthorization(HttpActionContext actionContext)
        {
			PrincipalAccessor.Service.InitializePrincipal(actionContext);

			Tuple<HttpStatusCode, string> response = AuthorizationService.Service.Authorize(actionContext);
            var httpStatusCode = response.Item1;
            var errorMessage = response.Item2;

            if (httpStatusCode != HttpStatusCode.OK)
            {
                var error = new ErrorResponse(new Error(ErrorCode.Unauthorized, errorMessage));
                actionContext.Response = actionContext.Request.CreateResponse(httpStatusCode);                
                actionContext.Response.Content = ContentBuilderService.Service.BuildContent(error);
            }
        }
    }
}
