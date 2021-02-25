using System.IO;
using System.Linq;
using EPiServer.SpecializedProperties;
using System.Web;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Web;
using System.Web.Mvc;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    /// <summary>
    /// The default implementation of rendering <see cref="PropertyXhtmlString"/>.
    /// Should return data in HTML format as the same in MVC/Webform based application
    /// </summary>
    [ServiceConfiguration(typeof(IXHtmlStringPropertyRenderer), Lifecycle = ServiceInstanceScope.Singleton)]

    public class DefaultXHtmlStringPropertyRenderer : IXHtmlStringPropertyRenderer
	{
        protected XhtmlRenderService _xhtmlRenderService;

		public DefaultXHtmlStringPropertyRenderer() : this(ServiceLocator.Current.GetInstance<XhtmlRenderService>())
		{

		}

		public DefaultXHtmlStringPropertyRenderer(XhtmlRenderService xhtmlRenderService)
		{
            _xhtmlRenderService = xhtmlRenderService;
		}

		/// <inheritdoc />
		public virtual string Render(PropertyXhtmlString propertyXhtmlString, bool excludePersonalizedContent)
		{
			if (propertyXhtmlString == null)
			{
				return string.Empty;
			}

			XhtmlString value = propertyXhtmlString.XhtmlString;
			if (value == null || value.Fragments == null || !value.Fragments.Any())
			{
				return string.Empty;
			}

			HttpContext context = HttpContext.Current;
			if (context == null)
			{
				var siteUrl = SiteDefinition.Current.SiteUrl != null
					? SiteDefinition.Current.SiteUrl.ToString()
					: "http://episerver.com";

				HttpRequest request = new HttpRequest(string.Empty, siteUrl, string.Empty);
				HttpResponse response = new HttpResponse(new StringWriter());
				context = new HttpContext(request, response);
			}

			if (excludePersonalizedContent)
			{
				//Remove Content Fragments when excluding personalized content - they cannot be successfully generated / pushed to Find currently due to HttpContext issues
				if (value.Fragments.Any(f => f is ContentFragment))
				{
					value = value.CreateWritableClone();
					for (var index = value.Fragments.Count - 1; index >= 0; index--)
					{
						if (value.Fragments[index] is ContentFragment)
						{
							value.Fragments.RemoveAt(index);
						}
					}
				}

				context.Items["ImpersonatedVisitorGroupsById"] = new string[] { string.Empty };
			}

			// use impersonated HttpContext to render data
			return Render(new HttpContextWrapper(context), value);
		}

		/// <summary>
		/// Render an <see cref="XhtmlString"/> Using Episerver's built-in RenderXhtmlString function of <see cref="HtmlHelper"/> class.
		/// </summary>
		/// <returns></returns>
		protected virtual string Render(HttpContextBase context, XhtmlString xhtmlString)
		{
			return _xhtmlRenderService.RenderXHTMLString(context, xhtmlString);
		}
	}
}
