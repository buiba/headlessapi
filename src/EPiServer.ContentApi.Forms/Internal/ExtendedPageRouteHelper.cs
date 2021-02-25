using EPiServer.ContentApi.Core;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System.Web;

namespace EPiServer.ContentApi.Forms.Internal
{
    /// <summary>
    /// Handling custom page route logic for ContentDelivery.Form
    /// </summary>
    [ServiceConfiguration(typeof(IPageRouteHelper))]
    internal class ExtendedPageRouteHelper : IPageRouteHelper
    {
        private readonly IPageRouteHelper _defaultPageRouteHelper;
        private readonly FormRenderingService _formRenderingService;

        public ExtendedPageRouteHelper(IPageRouteHelper defaultPageRouteHelper, FormRenderingService formRenderingService)
        {
            _defaultPageRouteHelper = defaultPageRouteHelper;
            _formRenderingService = formRenderingService;
        }

        public PageReference PageLink
        {            
            get
            {                                       
                var requestUrl = HttpContext.Current.Request.Url.AbsolutePath;

                // We only intercept for request from CD.Form
                if (requestUrl.Contains(RouteConstants.VersionTwoApiRoute))
                {
                    // When rendering form using CD.Form api, the request context is missing which cause Form resolves
                    // currentPageLink incorrectly (alway resolve to Start Page). So we must send a currentPageUrl param along with the request 
                    // and CD.Form will use this param to resolve current page         
                    return _formRenderingService.ExtractCurrentPage() ?? _defaultPageRouteHelper.PageLink;
                }

                return _defaultPageRouteHelper.PageLink;
            }
        }

        public PageData Page
        {
            get
            {
                return _defaultPageRouteHelper.Page;
            }
        }

        public string LanguageID
        {
            get
            {
                return _defaultPageRouteHelper.LanguageID;
            }
        }

        public ContentReference ContentLink
        {
            get
            {
                return _defaultPageRouteHelper.ContentLink;
            }
        }

        public IContent Content
        {
            get
            {
                return _defaultPageRouteHelper.Content;
            }
        }
    }
}
