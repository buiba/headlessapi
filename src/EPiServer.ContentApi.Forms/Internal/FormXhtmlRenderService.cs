using System.Web;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.Forms.Implementation.Elements;
using System.Linq;
using EPiServer.ContentApi.Core.Security;

namespace EPiServer.ContentApi.Forms.Internal
{
    /// <summary>
    /// Handles Xhtmlstring that contains forms
    /// </summary>
    [ServiceConfiguration(typeof(XhtmlRenderService))]
    public class FormXhtmlRenderService : XhtmlRenderService
    {
        /// <summary>
        /// to handle current principal
        /// </summary>
        protected readonly ISecurityPrincipal _securityPrincipal;

        public FormXhtmlRenderService(ISecurityPrincipal securityPrincipal) : base()
        {
            _securityPrincipal = securityPrincipal;
        }

        /// <inheritdoc />
        public override string RenderXHTMLString(HttpContextBase context, XhtmlString xhtmlString)
        {

            var template = base.RenderXHTMLString(context, xhtmlString);

            var stringFragments = xhtmlString.Fragments.GetFilteredFragments(_securityPrincipal.GetCurrentPrincipal());

            if ( !stringFragments.Any (fragment => (fragment is ContentFragment)
                                             &&  ((fragment as ContentFragment).GetContent() is FormContainerBlock) ) )
            {
                return template;
            }

            // Some required resources are registered in FormContainerBlockController
            // in normal alloy site, the page is loaded, therefore, those scripts are rendered
            // by  @Html.RequiredClientResources("Header") and  @Html.RequiredClientResources("Footer")
            // but for SPA, page is not loaded again, CD needs to return those scripts for execution at client side
            var header = Framework.Web.Resources.ClientResources.RenderRequiredResources("Header");
            var footer = Framework.Web.Resources.ClientResources.RenderRequiredResources("Footer");

            return string.Concat(header, template, footer);
        }
    }
}
