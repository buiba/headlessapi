using EPiServer.ContentApi.Core;
using EPiServer.Core;
using EPiServer.Web.Routing.Segments;
using Moq;
using System;
using System.Collections.Specialized;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.Routing.Tests
{
    public class ContentApiRouteServiceTest
    {
        protected readonly ContentApiRouteService Subject = new ContentApiRouteService();
        private readonly ContentReference _routedContentLink = new ContentReference(5);

        [Fact]
        public void ShouldRouteRequest_RequestNull_ReturnFalse()
        {
            HttpRequestBase request = null;

            Assert.False(Subject.ShouldRouteRequest(request));
        }

        [Fact]
        public void ShouldRouteRequest_AcceptTypesNull_ReturnFalse()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(req => req.AcceptTypes).Returns((string [])null);

            Assert.False(Subject.ShouldRouteRequest(request.Object));
        }

        [Fact]
        public void ShouldRouteRequest_AcceptHeaderContainsApplicationJson_ReturnTrue()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(req => req.AcceptTypes).Returns(new string[] { "application/json" });

            Assert.True(Subject.ShouldRouteRequest(request.Object));
        }

        [Fact]
        public void ShouldRouteRequest_AcceptHeaderDoesNotContainApplicationJson_ReturnFalse()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(req => req.AcceptTypes).Returns(new string[] { "text/html" });

            Assert.False(Subject.ShouldRouteRequest(request.Object));
        }

        [Fact]
        public void ShouldRouteRequest_AcceptHeaderIsNotPresented_ReturnFalse()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(req => req.Headers).Returns(new NameValueCollection());
            request.SetupGet(req => req.AcceptTypes).Returns(new string[] { });

            Assert.False(Subject.ShouldRouteRequest(request.Object));
        }

        [Fact]
        public void IsRoutableSegment_SegmentEmpty_ReturnFalse()
        {
            var segment = string.Empty;
            Assert.False(Subject.IsRoutableSegment(segment));
        }

        [Fact]
        public void IsRoutableSegment_SegmentWithChildren_ReturnTrue()
        {
            var segment = "children";
            Assert.True(Subject.IsRoutableSegment(segment));
        }

        [Fact]
        public void IsRoutableSegment_SegmentWithAncestors_ReturnTrue()
        {
            var segment = "ancestors";
            Assert.True(Subject.IsRoutableSegment(segment));
        }

        [Fact]
        public void IsRoutableSegment_SegmentNotInPartialList_ReturnFalse()
        {
            var segment = "unkown";
            Assert.False(Subject.IsRoutableSegment(segment));
        }

        [Fact]
        public void BuildRewritePath_RoutedDataIsNull_ReturnPathExlucdeSegment()
        {
            var segmentContext = new Mock<SegmentContext>(new Uri("http://localhost"));

            segmentContext.Setup(ctx => ctx.GetCustomRouteData<string>(It.IsAny<string>())).Returns((string)null);
            segmentContext.SetupGet(ctx => ctx.RoutedContentLink).Returns(_routedContentLink);

            var expectedResult = $"/{RouteConstants.VersionTwoApiRoute}content/{_routedContentLink}";
            var actual = Subject.BuildRewritePath(segmentContext.Object);

            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void BuildRewritePath_RoutedDataIsNotNull_ReturnPathIncludeSegment()
        {
            var segmentContext = new Mock<SegmentContext>(new Uri("http://localhost"));
            var segment = "children";

            segmentContext.Setup(ctx => ctx.GetCustomRouteData<string>(It.IsAny<string>())).Returns(segment);
            segmentContext.SetupGet(ctx => ctx.RoutedContentLink).Returns(_routedContentLink);

            var expectedResult = $"/{RouteConstants.VersionTwoApiRoute}content/{_routedContentLink}/{segment}";
            var actual = Subject.BuildRewritePath(segmentContext.Object);

            Assert.Equal(expectedResult, actual);
        }
    }
}