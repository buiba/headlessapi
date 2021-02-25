using System.Web.Mvc;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
	/// <summary>
	/// impersonated MVC Controller for HttpContext.
	/// EPiServer Core requires a controller to render XHTML string. So This class is created for that purpose, mostly just a hack way to create required environment for Core.
	/// </summary>
	public class ImpersonationController : Controller
	{
		/// <summary>
		/// impersonated index action
		/// </summary>
		/// <returns></returns>
		public System.Web.Mvc.ContentResult Index()
		{
			return new System.Web.Mvc.ContentResult();
		}
	}
}
