using System;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.Web;
using Moq;
using System.Linq;
using System.Net;
using System.Security.Principal;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal;
using EPiServer.ContentApi.Commerce.Internal.Controller;
using EPiServer.ContentApi.Commerce.Internal.Models.Inventory;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using Xunit;
using Mediachase.Commerce.Catalog;

namespace EPiServer.ContentApi.Commerce.Tests.Controller
{
    public class InventoryApiControllerTest 
    {
        private readonly InventoryApiController _controller;
        private readonly Mock<ContentLoaderService> _contentLoaderServiceMock;
        private readonly Mock<IContentApiSiteFilter> _contentApiSiteFilterMock;
        private readonly Mock<IContentApiRequiredRoleFilter> _contentApiRequiredRoleFilter;
        private readonly Mock<UserService> _userServiceMock;

        public InventoryApiControllerTest()
        {
            var mockInventoryService = new Mock<InventoryService>(null);
            mockInventoryService.Setup(x => x.GetInventories(It.IsAny<string>()))
                .Returns(Enumerable.Empty<InventoryApiModel>());

            _contentLoaderServiceMock = new Mock<ContentLoaderService>();
            _contentApiSiteFilterMock = new Mock<IContentApiSiteFilter>();
            _contentApiRequiredRoleFilter = new Mock<IContentApiRequiredRoleFilter>();

            var referenceConverterMock = new Mock<ReferenceConverter>(null, null);
            referenceConverterMock.Setup(x => x.GetCode(It.IsAny<ContentReference>())).Returns("code"); 

            _userServiceMock = new Mock<UserService>(null);

            _controller = new InventoryApiController(
                _contentApiRequiredRoleFilter.Object,
                _userServiceMock.Object, 
                _contentLoaderServiceMock.Object, 
                _contentApiSiteFilterMock.Object, 
                mockInventoryService.Object,
                referenceConverterMock.Object,
                new Mock<ISecurityPrincipal>().Object);
        }

        [Fact]
        public void Get_WhenContentIdIsInvalid_ShouldReturn400()
        {
            var result = (ContentApiResult<ErrorResponse>)_controller.Get(Guid.Empty);

            Assert.True(result.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void Get_WhenContentIsNull_ShouldReturn404()
        {
            _contentLoaderServiceMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns((IContent)null);

            var result = (ContentApiResult<ErrorResponse>)_controller.Get(Guid.NewGuid());

            Assert.True(result.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public void Get_WhenContentTypeIsNotCorrect_ShouldReturn404()
        {
            _contentLoaderServiceMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new NonCatalogContent());

            var result = (ContentApiResult<ErrorResponse>)_controller.Get(Guid.NewGuid());

            Assert.True(result.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public void Get_WhenContentIsFilteredBySiteFilter_ShouldReturn404()
        {
            _contentLoaderServiceMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new VariationContent() { Code = "Test" });
            _contentApiSiteFilterMock.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>())).Returns(true);

            var result = (ContentApiResult<ErrorResponse>)_controller.Get(Guid.NewGuid());

            Assert.True(result.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public void Get_WhenContentIsFilteredByRequireRoleFilter_ShouldReturn403()
        {
            _contentLoaderServiceMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new VariationContent() { Code = "Test" });
            _contentApiRequiredRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);

            var result = (ContentApiResult<ErrorResponse>)_controller.Get(Guid.NewGuid());

            Assert.True(result.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Get_WhenContentIsFilteredByUserService_ShouldReturn403()
        {
            _contentLoaderServiceMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new VariationContent() { Code = "Test" });
            _userServiceMock.Setup(x => x.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = (ContentApiResult<ErrorResponse>)_controller.Get(Guid.NewGuid());

            Assert.True(result.StatusCode == HttpStatusCode.Forbidden);
        }

        public class NonCatalogContent : IContent
        {
            public PropertyDataCollection Property { get; } = null;
            public string Name { get; set; }
            public ContentReference ContentLink { get; set; }
            public ContentReference ParentLink { get; set; }
            public Guid ContentGuid { get; set; }
            public int ContentTypeID { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}
