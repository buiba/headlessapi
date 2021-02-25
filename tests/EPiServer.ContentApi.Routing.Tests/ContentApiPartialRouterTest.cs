using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing.Segments;
using Moq;
using System;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.Routing.Tests
{
    public class ContentApiPartialRouterTest
    {
        private ContentApiPartialRouter contentApiPartialRouter;
        private Mock<ServiceAccessor<HttpContextBase>> httpContextAccessor;
        private Mock<ContentApiRouteService> contentApiRouteService;

        public ContentApiPartialRouterTest()
        {
            httpContextAccessor = new Mock<ServiceAccessor<HttpContextBase>>(); 
            contentApiRouteService = new Mock<ContentApiRouteService>();

            contentApiPartialRouter = new ContentApiPartialRouter(httpContextAccessor.Object, contentApiRouteService.Object);
        }     

        [Fact]
        public void RoutePartial_ShouldReturnNull_IfRequestIsNotRoutable()
        {
            var segmentContext = new Mock<SegmentContext>(new Uri("http://localhost"));
            contentApiRouteService.Setup(x => x.ShouldRouteRequest(It.IsAny<HttpRequestBase>())).Returns(false);

            Assert.Null(contentApiPartialRouter.RoutePartial(new PageData(), segmentContext.Object));
        }

        [Fact]
        public void RoutePartial_ShouldReturnNull_IfRequestIsRoutableAndSegmentIsNotRoutable()
        {
            var segmentContext = new Mock<SegmentContext>(new Uri("http://localhost"));
            contentApiRouteService.Setup(x => x.ShouldRouteRequest(It.IsAny<HttpRequestBase>())).Returns(true);
            contentApiRouteService.Setup(x => x.IsRoutableSegment(It.IsAny<string>())).Returns(false);

            Assert.Null(contentApiPartialRouter.RoutePartial(new PageData(), segmentContext.Object));
        }

        [Fact]
        public void RoutePartial_ShouldReturnContent_IfBothRequestAndSegmentIsRoutable()
        {
            var content = new PageData();
            var segmentContext = new Mock<SegmentContext>(new Uri("http://localhost"));
            contentApiRouteService.Setup(x => x.ShouldRouteRequest(It.IsAny<HttpRequestBase>())).Returns(true);
            contentApiRouteService.Setup(x => x.IsRoutableSegment(It.IsAny<string>())).Returns(true);

            Assert.Equal(contentApiPartialRouter.RoutePartial(content, segmentContext.Object), content);
        }
    }
}