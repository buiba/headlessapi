using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Security.Principal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using Moq;
using Xunit;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.Net.Http.Formatting;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.Web;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Tests.Security
{
    public class ContentApiAuthorizationAttributeTests
    {
        protected HttpActionContext _mockActionContext;
        protected ContentApiAuthorizationAttribute _attribute;
        protected string _administratorRole = "Administrators";                
        protected IPrincipal _principal;
		protected Mock<ISecurityPrincipal> _principalAccessor;
		protected Mock<ContentApiAuthorizationService> _authorizationService;
        private ContentApiConfiguration _apiConfig;

        private Mock<ISiteDefinitionResolver> siteDefinitionResolver;
        private Mock<IContentLoader> _contentLoader;
        private SiteDefinition siteDefinition;
        protected IContentApiSiteFilter _siteFilter;
        protected ContentApiRequiredRoleFilter _requiredRoleFilter;

        public ContentApiAuthorizationAttributeTests()
        {
			_principalAccessor = new Mock<ISecurityPrincipal>();
			_principal = new GenericPrincipal(new GenericIdentity("TestName"), new[] { _administratorRole });
            
            _mockActionContext = CreateMockActionContext(_principal);

			var options = new Mock<ContentApiOptions>();
            options.SetupGet(op => op.MinimumRoles).Returns(string.Empty);
            var roleService = new Mock<RoleService>(null);
            var userService = new Mock<UserService>(null);
            _apiConfig = new ContentApiConfiguration();

            _contentLoader = new Mock<IContentLoader>();

            siteDefinitionResolver = new Mock<ISiteDefinitionResolver>();
            siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid()
            };
            siteDefinitionResolver.Setup(x => x.GetByContent(It.IsAny<ContentReference>(), It.IsAny<bool>()))
            .Returns(siteDefinition);

            _siteFilter = new ContentApiSiteFilter(siteDefinitionResolver.Object, _apiConfig, _contentLoader.Object);

            roleService.Setup(rs => rs.GetMappedRolesAssociatedWithVirtualRole(It.IsAny<string>()))
                        .Returns((string role) => { return null; });

            _apiConfig.Default().SetRequiredRole("Content Api Access");

            _requiredRoleFilter = CreateMockFilter(_apiConfig, roleService.Object);

            _authorizationService = new Mock<ContentApiAuthorizationService>(_apiConfig, roleService.Object, userService.Object, 
                        _principalAccessor.Object, _siteFilter, _requiredRoleFilter, _contentLoader.Object);

            _attribute = new ContentApiAuthorizationAttribute();
            _attribute.AuthorizationService = _authorizationService.Object;
            _attribute.ContentBuilderService = Mock.Of<ContentResultService>();
			_attribute.PrincipalAccessor = new DefaultSecurityPrincipal();
		}
        
        public class OnAuthorization : ContentApiAuthorizationAttributeTests
        {
            public class When_response_status_code_is_200 : OnAuthorization
            {
                [Fact]
                public void It_should_do_nothing()
                {                    
                    _authorizationService.Setup(x => x.Authorize(It.IsAny<HttpActionContext>())).Returns(new Tuple<System.Net.HttpStatusCode, string>(System.Net.HttpStatusCode.OK, string.Empty));

                    _attribute.OnAuthorization(_mockActionContext);

                    Assert.True(_mockActionContext.Response == null);
                }
            }

            public class When_response_status_code_is_not_200 : OnAuthorization
            {
                [Fact]
                public void It_should_write_error_message_to_current_context_response()
                {                    
                    _authorizationService.Setup(x => x.Authorize(It.IsAny<HttpActionContext>())).Returns(new Tuple<System.Net.HttpStatusCode, string>(System.Net.HttpStatusCode.Forbidden, string.Empty));

                    _attribute.OnAuthorization(_mockActionContext);

                    Assert.True(_mockActionContext.Response.StatusCode == System.Net.HttpStatusCode.Forbidden);
                }
            }
        }       
        

		public HttpActionContext CreateMockActionContext(IPrincipal principal = null)
        {
            var httpActionDescriptorMock = new Mock<HttpActionDescriptor>();
			_principalAccessor.Setup(svc => svc.GetCurrentPrincipal()).Returns(principal);


			httpActionDescriptorMock
                .Setup(x => x.GetCustomAttributes<AllowAnonymousAttribute>())
                .Returns(new Collection<AllowAnonymousAttribute>());

            HttpActionContext mockActionContext = new HttpActionContext()
            {
                ControllerContext = new HttpControllerContext()
                {
                    Request = new HttpRequestMessage(),
                    RequestContext = new HttpRequestContext() { Principal = principal }
                },
                ActionArguments = { { "SomeArgument", "null" } },

            };

            httpActionDescriptorMock.Object.ControllerDescriptor =
                new HttpControllerDescriptor();
            mockActionContext.ActionDescriptor = httpActionDescriptorMock.Object;

            var httpConfiguration = new HttpConfiguration();
            mockActionContext.ControllerContext.ControllerDescriptor = new HttpControllerDescriptor(httpConfiguration, "Test", typeof(ApiController));
            mockActionContext.ControllerContext.Configuration = httpConfiguration;
            mockActionContext.ControllerContext.Configuration.Formatters.Add(new JsonMediaTypeFormatter());
            return mockActionContext;
        }

        private ContentApiRequiredRoleFilter CreateMockFilter(ContentApiConfiguration apiConfig = null, RoleService roleService = null)
        {
            return new ContentApiRequiredRoleFilter();
        }
    }
}
