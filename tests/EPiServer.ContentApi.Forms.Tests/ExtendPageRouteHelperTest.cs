using EPiServer.ContentApi.Forms.Internal;
using EPiServer.Web.Routing;
using Moq;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.Forms.Tests
{
    public class ExtendPageRouteHelperTest : TestBase
    {
        internal ExtendedPageRouteHelper Subject;
        private readonly Mock<IPageRouteHelper> _pageRouteHelper;
        
        public ExtendPageRouteHelperTest()
        {
            _pageRouteHelper = new Mock<IPageRouteHelper>();
            Subject = new ExtendedPageRouteHelper(_pageRouteHelper.Object, _formRenderingService.Object);
        }

        [Fact]
        public void PageLink_WhenRequestNotFromContentDelivery_ShouldNotCallExtractCurrentPage()
        {
            HttpContext.Current = CreateHttpContext("http://newhttpcontext.com", string.Empty);
            var pageLink = Subject.PageLink;
            _formRenderingService.Verify(x => x.ExtractCurrentPage(), Times.Never);
        }

        [Fact]
        public void PageLink_WhenRequestIsFromContentDelivery_ShouldCallExtractCurrentPage()
        {
            HttpContext.Current = CreateHttpContext("http://localhost/api/episerver/v2.0/content", string.Empty);
            var pageLink = Subject.PageLink;
            _formRenderingService.Verify(x => x.ExtractCurrentPage(), Times.Once);
        }
    }
}
