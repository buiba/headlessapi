using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc.Html;
using System.IO;
using System.Web;
using System.Web.Mvc;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
	/// <summary>
	/// Wrapper for HTMLHelper. This is needed for UT
	/// </summary>
	[ServiceConfiguration(typeof(XhtmlRenderService))]
	public class XhtmlRenderService
	{
        private readonly ContentApiConfiguration _apiConfiguration;        

        public XhtmlRenderService() : this(ServiceLocator.Current.GetInstance<ContentApiConfiguration>())
        {

        }

        public XhtmlRenderService(ContentApiConfiguration apiConfiguration)
        {
            _apiConfiguration = apiConfiguration;            
        }

        /// <summary>
        ///  Render XHTML string
        /// </summary>
        public virtual string RenderXHTMLString(HttpContextBase context, XhtmlString xhtmlString)
		{
			if (context == null || xhtmlString == null)
			{
				return string.Empty;
			}

			var routeData = new System.Web.Routing.RouteData();
			routeData.Values.Add("controller", "impersonation");
			routeData.Values.Add("action", "index");

			// While rendering as HTML format, EPiCMS.Core requires the following things:
			//		- HttpContext: Create a fake <see cref="HttpContextWrapper"/> from the given HttpContext context
			//		- A MVC controller: Using a fake <see cref="ImpersonationController"/>
			//		- Both `controller` and `action` key need to be presented in RouteData.
			var htmlHelper = new HtmlHelper(new ViewContext()
			{
				HttpContext = context,
				ViewData = new ViewDataDictionary(),
				TempData = new TempDataDictionary(),
				Controller = new ImpersonationController(),
				RouteData = routeData,
			}, new ViewPage());

			using (var writer = new StringWriter())
			{
				htmlHelper.ViewContext.Writer = writer;
                var options = _apiConfiguration.Default();

                htmlHelper.RenderXhtmlString(xhtmlString, new VirtualPathArguments
                {
                    ContextMode = Web.ContextMode.Default,
                    ForceCanonical = true,
                    ForceAbsolute = options.ForceAbsolute,
                    ValidateTemplate = false
                });

				writer.Flush();
				return writer.ToString();
			}
		}
	}
}
