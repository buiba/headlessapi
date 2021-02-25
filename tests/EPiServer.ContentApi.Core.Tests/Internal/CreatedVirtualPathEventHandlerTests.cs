using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Web.Routing;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Internal
{
    public class CreatedVirtualPathEventHandlerTests : TestBase
    {
        Mock<IContentLoader> _contentLoader;
        Mock<RequestContext> _requestContext;
        UrlBuilder _urlBuilder;

        public CreatedVirtualPathEventHandlerTests()
        {
            _contentLoader = new Mock<IContentLoader>();
            _requestContext = new Mock<RequestContext>();
            _urlBuilder = new UrlBuilder("locahost.com");
        }

        [Fact]
        public void WhenHostIsNull_ShouldNotChangeArgument()
        {
            // Arrange
            var args = new UrlBuilderEventArgs(_requestContext.Object, new RouteValueDictionary(), _urlBuilder, null);
            var handler = new CreatedVirtualPathEventHandler(_contentLoader.Object);

            // Act
            handler.Handle(args);

            // Assert
            Assert.Null(args.Host);
            _contentLoader.Verify(x => x.Get<IContent>(It.IsAny<ContentReference>()), Times.Never);
        }

        [Theory]
        [InlineData(HostDefinitionType.Primary)]
        [InlineData(HostDefinitionType.Undefined)]
        [InlineData(HostDefinitionType.Edit)]
        public void WhenOnlyOneHost_ShouldNotChangeArgument(HostDefinitionType hostDefinitionType)
        {
            // Arrange
            var hostName = "host.com";
            var host = new HostDefinition() { Name = hostName, Type = hostDefinitionType };
            var siteWithPublicHost = new SiteDefinition
            {
                SiteUrl = new Uri($"http://{hostName}/"),
                Name = "site with public host",
                Hosts = new List<HostDefinition> { host }
            };

            var args = new UrlBuilderEventArgs(_requestContext.Object, new RouteValueDictionary(), _urlBuilder, host);
            var handler = new CreatedVirtualPathEventHandler(_contentLoader.Object);

            // Act
            handler.Handle(args);

            // Assert
            Assert.Equal(args.Host.Name, hostName);
            _contentLoader.Verify(x => x.Get<IContent>(It.IsAny<ContentReference>()), Times.Never);
        }

        [Theory]
        [InlineData(HostDefinitionType.Primary)]
        [InlineData(HostDefinitionType.Undefined)]
        public void WhenContentLinkIsEmpty_ShouldNotChangeArgument(HostDefinitionType hostDefinitionType)
        {
            // Arrange
            var editHostName = "host.com";
            var primaryHostName = "primary.com";
            var primaryHost = new HostDefinition() { Name = primaryHostName, Type = hostDefinitionType };
            var editHost = new HostDefinition() { Name = editHostName, Type = HostDefinitionType.Edit };
            var siteWithPublicHost = new SiteDefinition
            {
                SiteUrl = new Uri($"http://{editHostName}/"),
                Name = "site with public host",
                Hosts = new List<HostDefinition> { primaryHost, editHost }
            };
            var routeValues = new RouteValueDictionary();
            routeValues[RoutingConstants.NodeKey] = ContentReference.EmptyReference;
            var args = new UrlBuilderEventArgs(_requestContext.Object, routeValues, _urlBuilder, primaryHost);
            var handler = new CreatedVirtualPathEventHandler(_contentLoader.Object);

            // Act
            handler.Handle(args);

            // Assert
            Assert.Equal(args.Host.Name, primaryHostName);
            _contentLoader.Verify(x => x.Get<IContent>(It.IsAny<ContentReference>()), Times.Never);
        }

        [Theory]
        [InlineData(HostDefinitionType.Primary)]
        [InlineData(HostDefinitionType.Undefined)]
        public void WhenContentLinkIsNotContentMedia_ShouldNotChangeArgument(HostDefinitionType hostDefinitionType)
        {
            // Arrange
            var editHostName = "host.com";
            var primaryHostName = "primary.com";
            var primaryHost = new HostDefinition() { Name = primaryHostName, Type = hostDefinitionType };
            var editHost = new HostDefinition() { Name = editHostName, Type = HostDefinitionType.Edit };
            var siteWithPublicHost = new SiteDefinition
            {
                SiteUrl = new Uri($"http://{editHostName}/"),
                Name = "site with public host",
                Hosts = new List<HostDefinition> { primaryHost, editHost }
            };
            var routeValues = new RouteValueDictionary();
            var contentLink = new ContentReference(1);
            routeValues[RoutingConstants.NodeKey] = contentLink;
            _contentLoader.Setup(x => x.Get<IContent>(contentLink)).Returns(new Mock<IContent>().Object);
            var args = new UrlBuilderEventArgs(_requestContext.Object, routeValues, _urlBuilder, primaryHost);
            var handler = new CreatedVirtualPathEventHandler(_contentLoader.Object);

            // Act
            handler.Handle(args);

            // Assert
            Assert.Equal(args.Host.Name, primaryHostName);
            _contentLoader.Verify(x => x.Get<IContent>(It.IsAny<ContentReference>()), Times.Once);
        }

        [Theory]
        [InlineData(HostDefinitionType.Primary)]
        [InlineData(HostDefinitionType.Undefined)]
        public void WhenContentLinkIsContentMedia_ShouldChangeArgument(HostDefinitionType hostDefinitionType)
        {
            // Arrange
            var editHostName = "host.com";
            var primaryHostName = "primary.com";
            var primaryHost = new HostDefinition() { Name = primaryHostName, Type = hostDefinitionType };
            var editHost = new HostDefinition() { Name = editHostName, Type = HostDefinitionType.Edit };
            var siteWithPublicHost = new SiteDefinition
            {
                SiteUrl = new Uri($"http://{editHostName}/"),
                Name = "site with public host",
                Hosts = new List<HostDefinition> { primaryHost, editHost }
            };
            var routeValues = new RouteValueDictionary();
            var contentLink = new ContentReference(1);
            routeValues[RoutingConstants.NodeKey] = contentLink;
            _contentLoader.Setup(x => x.Get<IContent>(contentLink)).Returns(new Mock<IContentMedia>().Object);
            var args = new UrlBuilderEventArgs(_requestContext.Object, routeValues, _urlBuilder, primaryHost);
            var handler = new CreatedVirtualPathEventHandler(_contentLoader.Object);

            // Act
            handler.Handle(args);

            // Assert
            Assert.Equal(args.Host.Name, editHostName);
            _contentLoader.Verify(x => x.Get<IContent>(It.IsAny<ContentReference>()), Times.Once);
        }

    }
}
