using EPiServer.ContentApi.Cms.Controllers;
using EPiServer.ContentApi.Cms.Internal;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Xunit;
using IContentApiContextModeResolver = EPiServer.ContentApi.Core.IContextModeResolver;

namespace EPiServer.ContentApi.Tests.Controllers
{
    public class ContentApiControllerTests
    {
        private Mock<UserService> _userService;
        protected Mock<IServiceLocator> _locator;
        protected ContentApiController controller;
        protected Mock<ContentLoaderService> _contentLoaderService;
        protected Mock<IContentApiSiteFilter> _mockSiteFilter;
        protected Mock<IContentApiRequiredRoleFilter> _mockRoleFilter;
        protected Mock<ISecurityPrincipal> _mockPrincipalAccessor;
        protected Mock<IContentApiContextModeResolver> _mockContentApiContextModeResolver;
        protected Mock<ContentConvertingService> _mockContentConvertingService;

        protected string _minimumRoles;
        protected string _administratorRole = "Administrators";
        protected string _webEditorRole = "WebEditors";
        private ContentApiConfiguration _apiConfig;
        protected Mock<RoleService> _roleService;
        protected Mock<ISecurityPrincipal> _principalAccessor;
        private Mock<ISiteDefinitionResolver> siteDefinitionResolver;
        private Mock<IContentLoader> _contentLoader;
        private SiteDefinition siteDefinition;
        protected ContentApiAuthorizationService _mockContentApiAuthorizationService;

        private AccessControlEntry NoAccessAdminControlEntry => new AccessControlEntry("Administrators", AccessLevel.NoAccess, SecurityEntityType.Role);
        private AccessControlEntry FullAccessAdminControlEntry => new AccessControlEntry("Administrators", AccessLevel.FullAccess, SecurityEntityType.Role);
        AccessControlList _aclList;

        private readonly List<PageData> allContents = new List<PageData>() { new PageData(), new PageData(), new PageData(), new PageData() };
        private readonly List<PageData> pagedContents = new List<PageData>() { new PageData(), new PageData() };

        private readonly string _fakeToken = "eyJsYXN0SW5kZXgiOjEsInRvcCI6MiwidG90YWxDb3VudCI6MzN9";

        public ContentApiControllerTests()
        {
            _mockContentConvertingService = CreateMockContentConvertingService();

            _contentLoaderService = new Mock<ContentLoaderService>();
            _aclList = new AccessControlList();
            _aclList.Add(FullAccessAdminControlEntry);
            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = _aclList });

            _mockSiteFilter = CreateMockSiteFilter(false);
            _mockRoleFilter = CreateMockContentApiRequiredRoleFilter(false);
            _mockPrincipalAccessor = CreateMockPrincipalAccessor(new GenericPrincipal(new GenericIdentity("AdminTester"),
                                                     new[] { "Administrators" }));
            _mockContentApiContextModeResolver = new Mock<IContentApiContextModeResolver>();
            _userService = new Mock<UserService>(null);
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);

            _minimumRoles = $"{_administratorRole}, {_webEditorRole}";
            _principalAccessor = new Mock<ISecurityPrincipal>();
            _roleService = new Mock<RoleService>(null);
            _apiConfig = new ContentApiConfiguration();
            _apiConfig.Default().SetMinimumRoles(_minimumRoles);

            _contentLoader = new Mock<IContentLoader>();

            siteDefinitionResolver = new Mock<ISiteDefinitionResolver>();
            siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid()
            };
            siteDefinitionResolver.Setup(x => x.GetByContent(It.IsAny<ContentReference>(), It.IsAny<bool>()))
            .Returns(siteDefinition);

            _roleService.Setup(rs => rs.GetMappedRolesAssociatedWithVirtualRole(It.IsAny<string>()))
                        .Returns((string role) => { return null; });

            _apiConfig.Default().SetRequiredRole("Content Api Access");


            _mockContentApiAuthorizationService = new ContentApiAuthorizationService(_apiConfig, _roleService.Object, _userService.Object,
                        _principalAccessor.Object, _mockSiteFilter.Object, _mockRoleFilter.Object, _contentLoader.Object);

            var contentApiTrackingContextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            contentApiTrackingContextAccessor.Setup(c => c.Current).Returns(new ContentApiTrackingContext());

            controller = new ContentApiController(_mockContentConvertingService.Object,
                                         _contentLoaderService.Object,
                                         _mockSiteFilter.Object,
                                         _mockRoleFilter.Object,
                                         _mockPrincipalAccessor.Object,
                                         _mockContentApiContextModeResolver.Object,
                                         _userService.Object,
                                         _mockContentApiAuthorizationService,
                                         new ContentApiConfiguration(),
                                         Mock.Of<ContentApiSerializerResolver>(),
                                         contentApiTrackingContextAccessor.Object,
                                         new ContentResolver(Mock.Of<UrlResolver>(), Mock.Of<IContentApiContextModeResolver>()),
                                         Mock.Of<ISiteDefinitionResolver>(), Mock.Of<IContentLanguageAccessor>(),
                                         Mock.Of<IUpdateCurrentLanguage>())
            {
                Request = new HttpRequestMessage(HttpMethod.Get, new Uri($"http://localhost/{RouteConstants.VersionTwoApiRoute}"))
            };
        }

        [Fact]
        public void Controller_ShouldHaveAuthorizationAttribute()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(ContentApiController),
                typeof(ContentApiAuthorizationAttribute));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Controller_ShouldHaveCorsAttribute()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(ContentApiController),
                typeof(ContentApiCorsAttribute));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Controller_ShouldHaveCorsOptionsFilter()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(ContentApiController),
                typeof(CorsOptionsActionFilter));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Get_ShouldReturnContentApiResult_WithValidContentReferenceRequest()
        {
            var result = controller.Get("5", new List<string>() { "en" }) as ContentApiResult<IContentApiModel>;

            Assert.True(result?.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public void Get_ShouldGetContentByProvidedReference_WhenLanguagesIsNotSet()
        {
            var content = new PageData() { ACL = _aclList };
            content.Property.Add(MetaDataProperties.PageMasterLanguageBranch, new PropertyString("en"));
            _contentLoaderService
                .Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(content);

            var result = controller.Get("5", new List<string>()) as ContentApiResult<IContentApiModel>;
            Assert.True(result?.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public void Get_ShouldGetSpecificLanguageOfContentByProvidedReference_WhenLanguagesIsSet()
        {
            _contentLoaderService
                .Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(new PageData() { ACL = _aclList });

            var result = controller.Get("5", new List<string> { "sv" }) as ContentApiResult<IContentApiModel>;

            _contentLoaderService.Verify(x => x.Get(It.IsAny<ContentReference>(), It.Is<string>(s => s.Equals("sv", StringComparison.OrdinalIgnoreCase))), Times.Once);
        }

        [Fact]
        public void Get_ShouldReturnBadRequest_WithInvalidContentReferenceRequest()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData());

            var result = controller.Get("badRequest", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.BadRequest, result?.StatusCode);
        }

        [Fact]
        public void Get_ShouldReturnNotFoundResponse_WithNonExistingContentReferenceRequest()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns((PageData)null);

            var result = controller.Get("9999", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public void Get_ShouldReturnForbiddenResponse_WhenContentLacksRequiredRole()
        {
            _mockRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);
            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = _aclList });

            var result = controller.Get("9999", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Get_ShouldReturnNotFoundResponse_WhenContentFailsSiteFilter()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = _aclList });

            _mockSiteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                           .Returns(true);

            var result = controller.Get("9999", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public void Get_ShouldReturnUnauthorized_WithFilteredContent()
        {
            AccessControlList list = new AccessControlList();
            list.Add(FullAccessAdminControlEntry);
            list.Add(new AccessControlEntry("Everyone", AccessLevel.NoAccess, SecurityEntityType.Role));

            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _mockPrincipalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(PrincipalInfo.AnonymousPrincipal);
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.Get("72", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Get_ShouldReturnForbidden_WithUserHavingNoAccess()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);

            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.Get("72", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Get_ShouldReturnContentResponse_WithContentReferenceRequest()
        {
            var result = controller.Get("72", new List<string>() { "en" }) as ContentApiResult<IContentApiModel>;

            Assert.True(result?.StatusCode == HttpStatusCode.OK && result.Value != null);
        }

        [Fact]
        public void GetContent_ShouldReturnContentApiResult_WithValidContentGuidRequest()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = _aclList });

            var result = controller.GetContent(Guid.Parse("0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4"), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<IContentApiModel>;

            Assert.True(result?.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public void GetContent_ShouldReturnNotFoundResponse_WithNonExistingContentGuidRequest()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Throws(new ContentNotFoundException());

            var result = controller.GetContent(Guid.Parse("0d24367a-c7a2-49ea-8b6f-7c1cccdddddd"), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetContent_ShouldReturnForbidden_WithContentGuidRequest()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);

            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.GetContent(Guid.Parse("0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4"), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public void GetContent_ShouldReturnForbidendResponse_WhenContentLacksRequiredRole()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData());

            _mockRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);

            var result = controller.GetContent(Guid.Parse("0d24367a-c7a2-49ea-8b6f-7c1cccdddddd"), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public void GetContent_ShouldReturnNotFoundResponse_WhenContentFailsSiteFilter()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData());
            _mockSiteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                           .Returns(true);

            var result = controller.GetContent(Guid.Parse("0d24367a-c7a2-49ea-8b6f-7c1cccdddddd"), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetContent_ShouldReturnForbiddenResponse_WithContentGuidRequest()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);

            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.GetContent(Guid.Parse("0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4"), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<ErrorResponse>;

            Assert.True(result?.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public void GetContent_ShouldReturnContentResponse_WithContentGuidRequest()
        {
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = _aclList });

            var result = controller.GetContent(Guid.Parse("0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4"), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<IContentApiModel>;

            Assert.True(result?.StatusCode == HttpStatusCode.OK && result.Value != null);
        }

        [Fact]
        public void GetChildren_ShouldReturnContentApiResult_WithValidContentGuidRequest()
        {
            var pages = new List<IContent> { new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList } };
            var pagedResultSet = new ContentDeliveryQueryRange<IContent>(pages, 0, 3, false);
            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<PagingToken>(), It.IsAny<Func<IContent, bool>>()))
                .Returns(pagedResultSet);

            var result = controller.GetChildren(Guid.NewGuid(), new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;
            Assert.True(result?.StatusCode == HttpStatusCode.OK && result.Value.Any());
        }

        [Fact]
        public void GetChildren_ShouldReturnAllContent_WhenNoPagingParametersPassed()
        {
            var contentQueryRange = new ContentDeliveryQueryRange<IContent>(pagedContents, 0, 4, true);

            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(allContents);
            var result = controller.GetChildren("5", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;
            Assert.True(result?.StatusCode == HttpStatusCode.OK && result.Value.Count() == 4);
        }

        [Fact]
        public void GetChildren_ShouldReturnPagedContent_WhenTopParameterPassed()
        {
            var contentQueryRange = new ContentDeliveryQueryRange<IContent>(pagedContents, 0, 4, true);

            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(),
                It.IsAny<string>(),
                It.Is<PagingToken>(p => p.Top == 2),
                It.IsAny<Func<IContent, bool>>()))
                .Returns(contentQueryRange);

            var result = controller.GetChildren("5", new List<string>() { "en" }, null, 2) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.True(result?.StatusCode == HttpStatusCode.OK && result.Value.Count() == 2);
        }

        [Fact]
        public void GetChildren_ShouldReturnPagedContent_WhenNextTokenParameterPassed()
        {
            var contentQueryRange = new ContentDeliveryQueryRange<IContent>(pagedContents, 0, 4, true);

            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(),
                It.IsAny<string>(),
                It.Is<PagingToken>(p => p.LastIndex == 1 && p.Top == 2),
                It.IsAny<Func<IContent, bool>>()))
                .Returns(contentQueryRange);

            var result = controller.GetChildren("5", new List<string>() { "en" }, null, null, _fakeToken) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.True(result?.StatusCode == HttpStatusCode.OK && result.Value.Count() == 2);
        }

        [Fact]
        public void GetChildren_ShouldReturnContinuationHeader_WhenHasMoreContent()
        {
            var contentQueryRange = new ContentDeliveryQueryRange<IContent>(pagedContents, 0, 4, true);

            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(),
                It.IsAny<string>(),
                It.Is<PagingToken>(p => p.LastIndex == 1 && p.Top == 2),
                It.IsAny<Func<IContent, bool>>()))
                .Returns(contentQueryRange);

            var result = controller.GetChildren("5", new List<string>() { "en" }, null, null, _fakeToken) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Contains(result.Headers, h => h.Key.Equals(PagingConstants.ContinuationTokenHeaderName) && h.Value != null);
        }

        [Fact]
        public void GetChildren_ShouldNotReturnContinuationHeader_WhenHasNoMoreContent()
        {
            var contentQueryRange = new ContentDeliveryQueryRange<IContent>(pagedContents, 0, 4, false);

            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(),
                It.IsAny<string>(),
                It.Is<PagingToken>(p => p.LastIndex == 1 && p.Top == 2),
                It.IsAny<Func<IContent, bool>>()))
                .Returns(contentQueryRange);

            var result = controller.GetChildren("5", new List<string>() { "en" }, null, null, _fakeToken) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.DoesNotContain(PagingConstants.ContinuationTokenHeaderName, result.Headers);
        }

        [Theory]
        [InlineData(0, "")]
        [InlineData(2, "fakeToken")]
        [InlineData(null, "abc")]
        [InlineData(null, "abcd")]
        public void GetChildren_ShouldReturnBadRequest_WhenParameterIsInvalid(int? top, string continuationToken)
        {
            var result = controller.GetChildren("5", new List<string>() { "en" }, null, top, continuationToken) as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.BadRequest, result?.StatusCode);
        }

        [Fact]
        public void GetChildren_ShouldReturnBadRequest_WithInvalidContentReferenceRequest()
        {
            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData(), new PageData(), new PageData() });

            var result = controller.GetChildren("badRequest", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.BadRequest, result?.StatusCode);
        }

        [Fact]
        public void GetChildren_ShouldReturnEmptyResponse_WithNonExistingContentReferenceRequest()
        {
            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent>());

            var result = controller.GetChildren("9999", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetChildren_ShouldReturnEmptyResponse_WithNonExistingContentGuidRequest()
        {
            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<PagingToken>(), It.IsAny<Func<IContent, bool>>())).Returns(new ContentDeliveryQueryRange<IContent>(Enumerable.Empty<IContent>(), 0, 0, false));

            var result = controller.GetChildren(Guid.NewGuid(), new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetChildren_ShouldReturnEmptyCollection_WithUnauthorizedUserRequest()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);

            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });
            _mockPrincipalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(PrincipalInfo.AnonymousPrincipal);
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.GetChildren("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetChildren_ShouldReturnEmptyCollection_WithForbiddenUserRequest()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);

            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);
            PrincipalInfo.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("AdminTester"), new[] { "Administrators" });

            var result = controller.GetChildren("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetChildren_ShouldFilterContent_WithoutRequiredRole()
        {
            AccessControlList list = new AccessControlList();
            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });

            _mockRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);
            PrincipalInfo.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("AdminTester"), new[] { "Administrators" });

            var result = controller.GetChildren("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetChildren_ShouldFilterContent_WithSiteFilter()
        {
            AccessControlList list = new AccessControlList();
            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });

            _mockSiteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                           .Returns(true);
            PrincipalInfo.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("AdminTester"), new[] { "Administrators" });

            var result = controller.GetChildren("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetChildren_UseDefaultValueIfTheyAreNotPassedThrough()
        {
            AccessControlList list = new AccessControlList();
            _contentLoaderService.Setup(x => x.GetChildren(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });

            _mockSiteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                           .Returns(true);
            PrincipalInfo.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("AdminTester"), new[] { "Administrators" });

            var result = controller.GetChildren("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;
            _contentLoaderService.Verify(x => x.GetChildren(It.IsAny<ContentReference>(), It.Is<string>(ls => ls == "en")), Times.Once);
        }

        [Fact]
        public void GetAncestors_ShouldReturnContentApiResult_WithValidContentReferenceRequest()
        {
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList } });
            _contentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList } });

            var result = controller.GetAncestors("5", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.NotEmpty(result?.Value);
        }

        [Fact]
        public void GetAncestors_ShouldReturnContentApiResult_WithValidContentGuidRequest()
        {
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList } });
            _contentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList } });

            var result = controller.GetAncestors(Guid.NewGuid(), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.NotEmpty(result?.Value);
        }

        [Fact]
        public void GetAncestors_ShouldReturnBadRequest_WithInvalidContentReferenceRequest()
        {
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList }, new PageData() { ACL = _aclList } });

            var result = controller.GetAncestors("badRequest", new List<string>() { "en" }) as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.BadRequest, result?.StatusCode);
        }

        [Fact]
        public void GetAncestors_ShouldReturnEmptyList_WithNonExistingContentReferenceRequest()
        {
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent>());
            _contentLoaderService.Setup(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<CultureInfo>())).Returns(new List<IContent>());

            _mockSiteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                           .Returns(true);

            var result = controller.GetAncestors("9999", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetAncestors_ShouldReturnEmptyList_WithNonExistingContentGuidRequest()
        {
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new List<IContent>());
            _contentLoaderService.Setup(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<CultureInfo>())).Returns(new List<IContent>());

            _mockSiteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                           .Returns(true);

            var result = controller.GetAncestors(Guid.NewGuid(), new List<string>() { "en" }, string.Empty, null) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetAncestors_ShouldReturnEmptyList_WithAnonymousUser()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });
            _contentLoaderService.Setup(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<CultureInfo>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });

            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.GetAncestors("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetAncestors_ShouldFilterContent_WithoutRequiredRole()
        {
            AccessControlList list = new AccessControlList();
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });
            _contentLoaderService.Setup(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<CultureInfo>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });
            _mockRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);

            var result = controller.GetAncestors("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void GetAncestors_ShouldFilterContent_WithSiteFilter()
        {
            AccessControlList list = new AccessControlList();
            _contentLoaderService.Setup(x => x.GetAncestors(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });
            _contentLoaderService.Setup(x => x.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<CultureInfo>())).Returns(new List<IContent> { new PageData() { ACL = list }, new PageData() { ACL = list }, new PageData() { ACL = list } });

            _mockSiteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                           .Returns(true);

            var result = controller.GetAncestors("72", new List<string>() { "en" }) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void QueryContent_ShouldReturnContentApiResult_WithValidContentReferenceOnlyRequest()
        {
            _contentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = _aclList } });

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "5,81") as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.NotEmpty(result?.Value);
        }

        [Fact]
        public void QueryContent_ShouldReturnContentApiResult_WithValidContentGuidOnlyRequest()
        {
            _contentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>())).Returns(new List<IContent> { new PageData() { ACL = _aclList } });

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4,0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4", string.Empty) as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.NotEmpty(result?.Value);
        }

        [Fact]
        public void QueryContent_WhenContentReferenceParameterIsInvalid_ShouldReturnBadRequest()
        {
            //It is inconsequent that passing invalid Guid causes BadRequest whild invalid ContentReference causes InternalServerError.
            //Dont want to make breaking chnage here and it is however at least consequent with how Get and GetChildren behaves

            //Need to call ToArray() to evaluate parsing of parameters
            _contentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<string>()))
                .Callback<IEnumerable<ContentReference>, string>((contentLinks, language) => { contentLinks.ToArray(); })
                .Returns(new[] { new PageData() { ACL = _aclList } });

            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "", "badrequest") as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.BadRequest, result?.StatusCode);
        }

        [Fact]
        public void QueryContent_WhenGuidParameterIsInvalid_ShouldReturnBadRequest()
        {
            //Need to call ToArray() to evaluate parsing of parameters
            _contentLoaderService.Setup(x => x.GetItemsWithOptions(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
                .Callback<IEnumerable<Guid>, string>((contentGuids, language) => { contentGuids.ToArray(); })
                .Returns(new[] { new PageData() { ACL = _aclList } });

            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "badrequest", "") as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.BadRequest, result?.StatusCode);
        }


        [Fact]
        public void QueryContent_ShouldReturnEmptyCollection_WithUnauthorizedRequest()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);

            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });

            _mockPrincipalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(PrincipalInfo.AnonymousPrincipal);
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4,0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4", "6") as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }


        [Fact]
        public void QueryContent_ShouldFilterContent_WithoutRequiredRole()
        {
            AccessControlList list = new AccessControlList();

            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });

            _mockPrincipalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(PrincipalInfo.AnonymousPrincipal);
            _mockRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);
            _userService.Setup(svc => svc.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4,0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4", "6") as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void QueryContent_ShouldFilterContent_WithSiteFilter()
        {
            AccessControlList list = new AccessControlList();

            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });

            _mockPrincipalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(PrincipalInfo.AnonymousPrincipal);
            _mockRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);
            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4,0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4", "6") as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void QueryContent_ShouldReturnEmptyCollection_WithForbiddenRequest()
        {
            AccessControlList list = new AccessControlList();
            list.Add(NoAccessAdminControlEntry);
            _contentLoaderService.Setup(x => x.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });
            _contentLoaderService.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(new PageData() { ACL = list });

            _userService.Setup(us => us.IsUserAllowedToAccessContent(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

            var result = controller.QueryContent(new List<string> { "en" }, string.Empty, "0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4,0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4", "6") as ContentApiResult<IEnumerable<IContentApiModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Empty(result?.Value);
        }

        [Fact]
        public void QueryContent_ShouldReturnBadRequest_WithInvalidCombinationParams_UrlAndReferences()
        {
            var result = controller.QueryContent(new List<string> { "en" }, "/test-page", "5,81") as ContentApiResult<ErrorResponse>; ;
            Assert.True(result?.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void QueryContent_ShouldReturnBadRequest_WithInvalidCombinationParams_UrlAndGuids()
        {
            var result = controller.QueryContent(new List<string> { "en" }, "/test-page", "0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4,0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4") as ContentApiResult<ErrorResponse>;
            Assert.True(result?.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void QueryContent_ShouldReturnBadRequest_WithInvalidCombinationParams_UrlAndReferencesAndGuids()
        {
            var result = controller.QueryContent(new List<string> { "en" }, "/test-page", "0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4,0d24367a-c7a2-49ea-8b6f-7c1ccc1ee7a4", "5,81") as ContentApiResult<ErrorResponse>; ;
            Assert.True(result?.StatusCode == HttpStatusCode.BadRequest);
        }

        private static Mock<IContentApiRequiredRoleFilter> CreateMockContentApiRequiredRoleFilter(bool shouldFilterContent)
        {
            var mockRoleFilter = new Mock<IContentApiRequiredRoleFilter>();
            mockRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(shouldFilterContent);

            return mockRoleFilter;
        }

        private static Mock<IContentApiSiteFilter> CreateMockSiteFilter(bool shouldFilterContent)
        {
            var siteFilter = new Mock<IContentApiSiteFilter>();
            siteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>()))
                .Returns(shouldFilterContent);
            return siteFilter;
        }

        private static Mock<ISecurityPrincipal> CreateMockPrincipalAccessor(IPrincipal principal)
        {
            var _mockPrincipalAccessor = new Mock<ISecurityPrincipal>();
            _mockPrincipalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(principal);
            return _mockPrincipalAccessor;
        }

        private static Mock<ContentConvertingService> CreateMockContentConvertingService()
        {
            var contentConverter = new Mock<ContentConvertingService>();
            contentConverter.Setup(x => x.Convert(It.IsAny<IContent>(), It.IsAny<ConverterContext>()))
                .Returns(new ContentApiModel());
            return contentConverter;
        }
    }
}
