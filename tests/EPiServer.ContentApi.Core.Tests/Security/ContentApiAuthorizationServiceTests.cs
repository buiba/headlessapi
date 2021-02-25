using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Security.Principal;
using EPiServer.ContentApi.Core.Internal;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.Net.Http.Formatting;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.Web;
using EPiServer.Core;
using EPiServer.Security;

namespace EPiServer.ContentApi.Core.Tests.Security
{
    public class ContentApiAuthorizationServiceTests
    {
        protected HttpActionContext _mockActionContext;
        protected ContentApiAuthorizationService _authorizationService;
        protected string _administratorRole = "Administrators";
        protected string _webEditorRole = "WebEditors";
        protected IPrincipal _principal;
        protected string _minimumRoles;
        protected Mock<UserService> _userService;
        protected Mock<RoleService> _roleService;
        protected Mock<ISecurityPrincipal> _principalAccessor;
        private ContentApiConfiguration _apiConfig;

        private Mock<ISiteDefinitionResolver> siteDefinitionResolver;
        private Mock<IContentLoader> _contentLoader;
        private SiteDefinition siteDefinition;
        protected Mock<IContentApiSiteFilter> _siteFilter;
        protected Mock<ContentApiRequiredRoleFilter> _requiredRoleFilter;

        protected PageData _page;

        public ContentApiAuthorizationServiceTests()
        {
            _principalAccessor = new Mock<ISecurityPrincipal>();
            _principal = new GenericPrincipal(new GenericIdentity("TestName"), new[] { _administratorRole });

            _mockActionContext = CreateMockActionContext(_principal);
            _minimumRoles = $"{_administratorRole}, {_webEditorRole}";
            _roleService = new Mock<RoleService>(null);
            _userService = new Mock<UserService>(null);

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

            _siteFilter = new Mock<IContentApiSiteFilter>();

            _roleService.Setup(rs => rs.GetMappedRolesAssociatedWithVirtualRole(It.IsAny<string>()))
                        .Returns((string role) => { return null; });

            _apiConfig.Default().SetRequiredRole("Content Api Access");

            _requiredRoleFilter = CreateMockFilter(_apiConfig, _roleService.Object);

            _authorizationService = new ContentApiAuthorizationService(_apiConfig, _roleService.Object, _userService.Object,
                _principalAccessor.Object, _siteFilter.Object, _requiredRoleFilter.Object, _contentLoader.Object);

            var pageProperties = new PropertyDataCollection
            {
                { "PageLink", new PropertyContentReference(1) },
                { "PagePendingPublish", new PropertyBoolean(false) },
                { "PageStopPublish", new PropertyDate(DateTime.UtcNow.AddDays(2)) },
                { "PageWorkStatus", new PropertyNumber(4) }
            };
            _page = new PageData(new AccessControlList(), pageProperties);
        }


        public class Authorize : ContentApiAuthorizationServiceTests
        {
            public class When_minimum_role_has_not_been_set_up : ContentApiAuthorizationServiceTests
            {
                [Fact]
                public void It_should_return_response_with_200_status_code()
                {
                    _apiConfig.Default().SetMinimumRoles(string.Empty);
                    _authorizationService = new ContentApiAuthorizationService(_apiConfig, _roleService.Object, _userService.Object,
                                    _principalAccessor.Object, _siteFilter.Object, _requiredRoleFilter.Object, _contentLoader.Object);

                    var response = _authorizationService.Authorize(_mockActionContext);

                    Assert.True(response.Item1 == System.Net.HttpStatusCode.OK);
                }
            }

            public class When_minimum_role_has_been_setup : ContentApiAuthorizationServiceTests
            {
                public class When_principal_is_not_valid : When_minimum_role_has_been_setup
                {
                    [Fact]
                    public void It_should_return_response_with_401_status_code()
                    {
                        // setup
                        _mockActionContext = CreateMockActionContext(null);

                        // action
                        var response = _authorizationService.Authorize(_mockActionContext);

                        // asert
                        Assert.True(response.Item1 == System.Net.HttpStatusCode.Unauthorized);
                    }
                }

                public class When_principal_is_valid : When_minimum_role_has_been_setup
                {
                    public class When_user_do_not_belong_to_minimum_role : When_principal_is_valid
                    {
                        public When_user_do_not_belong_to_minimum_role()
                        {
                            IPrincipal principal = new GenericPrincipal(new GenericIdentity("TestName"), new[] { "Users" });
                            _mockActionContext = CreateMockActionContext(principal);
                            _roleService.Setup(rs => rs.GetMappedRolesAssociatedWithVirtualRole(It.IsAny<string>())).Returns(new List<string>());
                        }

                        [Fact]
                        public void It_should_return_response_with_403_status_code()
                        {
                            // action
                            var response = _authorizationService.Authorize(_mockActionContext);

                            // assert
                            Assert.True(response.Item1 == System.Net.HttpStatusCode.Forbidden);
                        }
                    }
                }

                public class When_user_belong_to_valid_minimum_role : When_principal_is_valid
                {
                    public When_user_belong_to_valid_minimum_role()
                    {
                        IPrincipal principal = new GenericPrincipal(new GenericIdentity("TestName"), new[] { $"{_webEditorRole}" });
                        _mockActionContext = CreateMockActionContext(principal);
                    }

                    public class When_minimum_role_is_virtual_role : When_user_belong_to_valid_minimum_role
                    {
                        private readonly string _virtualRole = "virtualrole";
                        public When_minimum_role_is_virtual_role()
                        {
                            _minimumRoles = $"{_virtualRole}";
                            _apiConfig.Default().SetMinimumRoles(_minimumRoles);

                            _roleService.Setup(rs => rs.GetMappedRolesAssociatedWithVirtualRole(It.Is<string>(r => r.Equals(_virtualRole, StringComparison.OrdinalIgnoreCase))))
                                        .Returns(new List<string> { _webEditorRole, _administratorRole });
                        }

                        [Fact]
                        public void It_should_return_response_with_200_status_code()
                        {
                            // action
                            var response = _authorizationService.Authorize(_mockActionContext);

                            // assert
                            Assert.True(response.Item1 == System.Net.HttpStatusCode.OK);
                        }
                    }

                    public class When_minimum_role_is_normal_role : When_user_belong_to_valid_minimum_role
                    {
                        [Fact]
                        public void It_should_return_response_with_403_status_code()
                        {
                            // action
                            var response = _authorizationService.Authorize(_mockActionContext);

                            // assert
                            Assert.True(response.Item1 == System.Net.HttpStatusCode.Forbidden);
                        }
                    }
                }
            }
        }

        public HttpActionContext CreateMockActionContext(IPrincipal principal = null)
        {
            var httpActionDescriptorMock = new Mock<HttpActionDescriptor>();

            httpActionDescriptorMock
                .Setup(x => x.GetCustomAttributes<AllowAnonymousAttribute>())
                .Returns(new Collection<AllowAnonymousAttribute>());

            var mockActionContext = new HttpActionContext()
            {
                ControllerContext = new HttpControllerContext()
                {
                    Request = new HttpRequestMessage(),
                    RequestContext = new HttpRequestContext() { Principal = principal }
                },
                ActionArguments = { { "SomeArgument", "null" } },
            };

            _principalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(principal);

            httpActionDescriptorMock.Object.ControllerDescriptor =
                new HttpControllerDescriptor();
            mockActionContext.ActionDescriptor = httpActionDescriptorMock.Object;

            var httpConfiguration = new HttpConfiguration();
            mockActionContext.ControllerContext.ControllerDescriptor = new HttpControllerDescriptor(httpConfiguration, "Test", typeof(ApiController));
            mockActionContext.ControllerContext.Configuration = httpConfiguration;
            mockActionContext.ControllerContext.Configuration.Formatters.Add(new JsonMediaTypeFormatter());
            return mockActionContext;
        }

        private Mock<ContentApiRequiredRoleFilter> CreateMockFilter(ContentApiConfiguration apiConfig = null, RoleService roleService = null)
        {
            return new Mock<ContentApiRequiredRoleFilter>();
        }

        public class IsContentValid : ContentApiAuthorizationServiceTests
        {
            [Fact]
            public void ShouldReturnFalse_WhenContentIsNull()
            {
                var result = _authorizationService.CanUserAccessContent((IContent)null);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsFilteredBySiteFilter()
            {
                _siteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>())).Returns(true);

                var result = _authorizationService.CanUserAccessContent(_page);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsFilteredByRequireRoleFilter()
            {
                _requiredRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);

                var result = _authorizationService.CanUserAccessContent(_page);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsFilteredByUserService()
            {
                _userService.Setup(x => x.IsUserAllowedToAccessContent(It.IsAny<IContent>(),
                    It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

                var result = _authorizationService.CanUserAccessContent(_page);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnTrue_WhenContentIsValid()
            {
                _siteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>())).Returns(false);
                _requiredRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(false);
                _userService.Setup(x => x.IsUserAllowedToAccessContent(It.IsAny<IContent>(),
                    It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);

                var result = _authorizationService.CanUserAccessContent(_page);

                Assert.True(result);
            }
        }

        public class IsContentReferenceValid : ContentApiAuthorizationServiceTests
        {
            private ContentReference _pageLink = new ContentReference(1);
            private IContent content = new PageData();

            [Fact]
            public void ShouldReturnFalse_WhenCannotGetContentFromContentLoader()
            {
                _contentLoader.Setup(x => x.TryGet(It.IsAny<ContentReference>(), out content)).Returns(false);

                var result = _authorizationService.CanUserAccessContent(_pageLink);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsNull()
            {
                var result = _authorizationService.CanUserAccessContent((ContentReference)null);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsFilteredBySiteFilter()
            {
                _siteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>())).Returns(true);

                var result = _authorizationService.CanUserAccessContent(_pageLink);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsFilteredByRequireRoleFilter()
            {
                _requiredRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(true);

                var result = _authorizationService.CanUserAccessContent(_pageLink);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnFalse_WhenContentIsFilteredByUserService()
            {
                _userService.Setup(x => x.IsUserAllowedToAccessContent(It.IsAny<IContent>(),
                    It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(false);

                var result = _authorizationService.CanUserAccessContent(_pageLink);

                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnTrue_WhenContentIsValid()
            {
                _contentLoader.Setup(x => x.TryGet(It.IsAny<ContentReference>(), out content)).Returns(true);

                _siteFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>(), It.IsAny<SiteDefinition>())).Returns(false);
                _requiredRoleFilter.Setup(x => x.ShouldFilterContent(It.IsAny<IContent>())).Returns(false);
                _userService.Setup(x => x.IsUserAllowedToAccessContent(It.IsAny<IContent>(),
                    It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);

                var result = _authorizationService.CanUserAccessContent(_pageLink);

                Assert.True(result);
            }
        }
    }
}
