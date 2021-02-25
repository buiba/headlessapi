using System.Security.Principal;
using System.Web.Http.Controllers;

namespace EPiServer.ContentApi.Core.Security
{
	/// <summary>
	/// Initialzing and accessing <see cref="IPrincipal"/> within ContentApi's scope.
	/// </summary>
	public interface ISecurityPrincipal
	{
		/// <summary>
		/// Initialize principal with provided HttpActionContext
		/// We must set the <see cref="IPrincipal"/> on to places: Thread.CurrentPrincipal and HttpContext.Current.User 
		/// </summary>
		/// <param name="actionContext"></param>
		void InitializePrincipal(HttpActionContext actionContext);

		/// <summary>
		/// Get the current principal 
		/// </summary>
		/// <returns></returns>
		IPrincipal GetCurrentPrincipal();

		/// <summary>
		/// Retrieve an anonymous Principal with configured virtual roles applied such as for example EveryoneRole
		/// </summary>
		/// <remarks>The returned principal is a singleton instance and should not be altered.</remarks>
		IPrincipal GetAnonymousPrincipal();
	}
}
