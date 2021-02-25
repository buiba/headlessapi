using EPiServer.Globalization;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;
using Moq;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.Routing.Tests
{
    public class RoutingEventHandlerTest
    {
        protected readonly RoutingEventHandler Subject;

        private readonly Mock<IContentRouteEvents> _contentRouteEvents;
        private readonly Mock<TestHttpContext> _httpContext;
        private readonly Mock<ContentApiRouteService> _contentApiRouteService;

        public RoutingEventHandlerTest()
        {
            _contentRouteEvents = new Mock<IContentRouteEvents>();
            _httpContext = new Mock<TestHttpContext>();
            _contentApiRouteService = new Mock<ContentApiRouteService>();

            Subject = new RoutingEventHandler(_contentRouteEvents.Object, () => _httpContext.Object, _contentApiRouteService.Object);
            Subject.AttachEventHandler();
        }

        [Fact]
        public void RoutedContent_IfContentIsRoutable_ShouldRewritePath_Children()
        {
            // setup
            var httpRequest = CreateHttpRequest("application/json", "en");
            _httpContext.Setup(req => req.Request).Returns(httpRequest.Object);

            // currently, only support routing with segments: children, ancestors
            var routingArgs = new RoutingEventArgs(new TestSegmentContext(new Uri("http://localhost:80/en/alloy-plan/children")));
            ContentLanguage.PreferredCulture = new CultureInfo("en");

            // should route request
            _contentApiRouteService.Setup(sbc => sbc.ShouldRouteRequest(It.IsAny<HttpRequestBase>())).Returns(true);

            // act
            _contentRouteEvents.Raise(x => x.RoutedContent += null, (object)null, routingArgs);

            // assert
            _contentApiRouteService.Verify(x => x.BuildRewritePath(It.IsAny<SegmentContext>()), Times.Once);
        }

        [Fact]
        public void RoutedContent_IfContentIsNotRoutable_ShouldNotRewritePath()
        {
            // setup
            var httpRequest = CreateHttpRequest("application/json", "en");
            _httpContext.Setup(req => req.Request).Returns(httpRequest.Object);
            var routingArgs = new RoutingEventArgs(new TestSegmentContext(new Uri("http://localhost:80/en/alloy-plan")));
            ContentLanguage.PreferredCulture = new CultureInfo("en");

            // should not route request
            _contentApiRouteService.Setup(svc => svc.ShouldRouteRequest(It.IsAny<HttpRequestBase>())).Returns(false);

            // act
            _contentRouteEvents.Raise(x => x.RoutedContent += null, (object)null, routingArgs);

            // assert
            _contentApiRouteService.Verify(x => x.BuildRewritePath(It.IsAny<SegmentContext>()), Times.Never);
        }

        private Mock<TestHttpRequest> CreateHttpRequest(string acceptTypeHeader, string acceptLanguageHeader)
        {
            var request = new Mock<TestHttpRequest>();
            request.SetupGet(req => req.AcceptTypes).Returns(new string[] { acceptTypeHeader });

            NameValueCollection headers = new NameValueCollection();
            headers.Add("Accept-Language", acceptLanguageHeader);
            request.SetupGet(req => req.Headers).Returns(headers);
            return request;
        }
    }

    public class TestHttpContext : HttpContextBase
    {
    }

    public class TestHttpRequest : HttpRequestBase
    {
    }

    public class TestSegmentContext : SegmentContext
    {
        public TestSegmentContext(Uri requestUrl) : base(requestUrl)
        {
        }

        public override NameValueCollection QueryString { get { throw new System.NotImplementedException(); } }

        public override SegmentContext Copy()
        {
            throw new System.NotImplementedException();
        }
    }
}

