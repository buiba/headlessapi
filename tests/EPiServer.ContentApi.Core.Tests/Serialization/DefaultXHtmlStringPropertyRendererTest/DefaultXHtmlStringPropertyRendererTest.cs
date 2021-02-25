using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Core.Internal;
using EPiServer.ServiceLocation;
using System;

using Moq;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
	/// <summary>
	/// Test class for <see cref="DefaultXHtmlStringPropertyRenderer"/>
	/// </summary>
	public class DefaultXHtmlStringPropertyRendererTest : TestBase
	{
		protected DefaultXHtmlStringPropertyRenderer Subject;
		protected Mock<XhtmlRenderService> _htmlHelperWrapper;
		protected Mock<ContentApiConfiguration> _apiConfiguration;        

		public DefaultXHtmlStringPropertyRendererTest()
		{
            _htmlHelperWrapper = CreateMockableHelpHelperWrapper();
			Subject = new DefaultXHtmlStringPropertyRenderer(_htmlHelperWrapper.Object);
        }

		protected ContentFragment CreateContentFragment(ContentReference contentLink, System.Guid guid)
		{
			return ServiceLocator.Current.GetInstance<ContentFragmentFactory>()
									     .CreateContentFragment(contentLink, guid, null);
		}

		protected HttpContext CreateHttpContext()
		{
			var httpContext = new HttpContext(
					new HttpRequest("", "http://newhttpcontext.com", ""),
					new HttpResponse(new StringWriter())
				);

			httpContext.Items["serviceLocator"] = _locator;
			return httpContext;
		}

		protected Mock<XhtmlRenderService> CreateMockableHelpHelperWrapper()
		{   
            var apiConfiguration = new ContentApiConfiguration();

            var defaultOptions = apiConfiguration.Default();
            defaultOptions.SetHttpResponseExpireTime(TimeSpan.FromSeconds(5));


            var htmlHelperWrapper = new Mock<XhtmlRenderService>(_apiConfiguration);
			htmlHelperWrapper.Setup(hp => hp.RenderXHTMLString(It.IsAny<HttpContextBase>(), It.IsAny<XhtmlString>())).Returns(
				(HttpContextBase contextBase, XhtmlString xhtmlString) =>
				{
					if (xhtmlString == null || xhtmlString.Fragments == null || !xhtmlString.Fragments.Any())
					{
						return string.Empty;
					}

					var result = new StringBuilder();
					foreach (var frag in xhtmlString.Fragments)
					{
						result.Append(frag.GetViewFormat());
					}

					return result.ToString();
				}
			);

			return htmlHelperWrapper;
		}
	}


	public class HeadlessXhtmlString : XhtmlString
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XhtmlString"/> class.
		/// </summary>
		public HeadlessXhtmlString()
            : this(null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="XhtmlString"/> class.
		/// </summary>
		/// <param name="unparsedString">The unparsed xhtml string.</param>
		public HeadlessXhtmlString(string unparsedString):base(unparsedString){ }

		/// <summary>
		/// Creates a writable deep clone of the current object.
		/// </summary>
		/// <returns>A writable copy of the current instance.</returns>
		protected override XhtmlString CreateWriteableCloneImplementation()
		{
			return this;
		}
	}
}
