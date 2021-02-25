using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Cms.UI.AspNetIdentity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OAuth.Messages;
using Moq;
using Xunit;
using EPiServer.ContentApi.OAuth.Internal;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
    public class IdentityAuthorizationServerProviderTests
    {
        internal IdentityAuthorizationServerProvider<ApplicationUserManager<ApplicationUser>, ApplicationUser, string>
            ServerProvider;

        internal const string Username = "testUser";
        internal const string Password = "testPassword";
        internal const string ClientId = "TestClient";

        public IdentityAuthorizationServerProviderTests()
        {
            var options = new ContentApiOAuthOptions();
            var clients = new List<ApiClientInfo>()
            {
                new ApiClientInfo()
                {
                    ClientId = "TestClient",
                    AccessControlAllowOrigin = "http://mytest.local"
                }
            };

            options.Clients = clients;

            ServerProvider = new IdentityAuthorizationServerProvider<ApplicationUserManager<ApplicationUser>, ApplicationUser, string>(options);
        }

        [Fact]
        public async void ValidateClientAuthentication_ShouldReturnError_WhenGrantTypeIsMissing()
        {
            var parameters = new Dictionary<string, string[]> {
                { "client_id", new []{ "Default" } }
            };
            var validationContext = new OAuthValidateClientAuthenticationContext(new OwinContext(), new OAuthAuthorizationServerOptions(),
                new ReadableStringCollection(parameters));

            await ServerProvider.ValidateClientAuthentication(validationContext);

            Assert.True(validationContext.IsValidated == false && validationContext.Error == OAuthErrors.InvalidGrant);
        }

        [Fact]
        public async void ValidateClientAuthentication_ShouldReturnError_WhenRefreshTokenIsMissingAndGrantTypeIsRefresh()
        {
            var parameters = new Dictionary<string, string[]> {
                { "grant_type", new []{ "refresh_token" } },
                { "client_id", new []{ "Default" } }
            };
            var validationContext = new OAuthValidateClientAuthenticationContext(new OwinContext(), new OAuthAuthorizationServerOptions(),
                new ReadableStringCollection(parameters));

            await ServerProvider.ValidateClientAuthentication(validationContext);

            Assert.True(validationContext.IsValidated == false && validationContext.Error == OAuthErrors.InvalidRefreshToken);
        }

        [Fact]
        public async void ValidateClientAuthentication_ShouldReturnError_WhenClientIdIsEmpty()
        {

            var validationContext = new OAuthValidateClientAuthenticationContext(new OwinContext(), new OAuthAuthorizationServerOptions(),
                new ReadableStringCollection(new Dictionary<string, string[]>()));

            await ServerProvider.ValidateClientAuthentication(validationContext);

            Assert.True(validationContext.Error == OAuthErrors.InvalidClientId && validationContext.IsValidated == false);
        }

        [Fact]
        public async void ValidateClientAuthentication_ShouldReturnError_WhenClientIdIsNotValid()
        {
            var parameters = new Dictionary<string, string[]> {
                { "grant_type", new []{ "password" } }
            };
            var validationContext = new OAuthValidateClientAuthenticationContext(new OwinContext(), new OAuthAuthorizationServerOptions(),
                new ReadableStringCollection(parameters));

            var authenticationString =
                Convert.ToBase64String(
                    System.Text.Encoding.Unicode.GetBytes("NotDefaultClient:fjasdfj8a9sfu908f8adf90sadsf908asf"));

            validationContext.Request.Headers["Authorization"] = $"Basic {authenticationString}";

            await ServerProvider.ValidateClientAuthentication(validationContext);

            Assert.True(validationContext.Error == OAuthErrors.InvalidClientId && validationContext.IsValidated == false);
        }

        [Fact]
        public async void ValidateClientAuthentication_ShouldReturnError_WhenClientHasOriginMismatch()
        {
            var parameters = new Dictionary<string, string[]> {
                { "grant_type", new []{ "password" } }
            };
            var validationContext = new OAuthValidateClientAuthenticationContext(new OwinContext(), new OAuthAuthorizationServerOptions(),
                new ReadableStringCollection(parameters));

            var authenticationString =
                Convert.ToBase64String(
                    ASCIIEncoding.ASCII.GetBytes("TestClient:fjasdfj8a9sfu908f8adf90sadsf908asf"));

            validationContext.Request.Headers["Authorization"] = $"Basic {authenticationString}";
            validationContext.Request.Headers["Origin"] = "http://anotherorigin.local";

            await ServerProvider.ValidateClientAuthentication(validationContext);

            Assert.True(validationContext.Error == OAuthErrors.InvalidOrigin && validationContext.IsValidated == false);
        }

        [Fact]
        public async void ValidateClientAuthentication_ShouldValidateClient()
        {
            var parameters = new Dictionary<string, string[]> {
                { "grant_type", new []{ "password" } }
            };
            var validationContext = new OAuthValidateClientAuthenticationContext(new OwinContext(), new OAuthAuthorizationServerOptions(),
                new ReadableStringCollection(parameters));

            var authenticationString =
                Convert.ToBase64String(
                    ASCIIEncoding.ASCII.GetBytes("TestClient:fjasdfj8a9sfu908f8adf90sadsf908asf"));

            validationContext.Request.Headers["Authorization"] = $"Basic {authenticationString}";

            await ServerProvider.ValidateClientAuthentication(validationContext);

            Assert.True(validationContext.IsValidated && validationContext.ClientId == "TestClient");
        }

        [Fact]
        public async void ValidateClientAuthentication_ShouldAttachAccessControlAllowOriginToContextForClient()
        {
            var parameters = new Dictionary<string, string[]> {
                { "grant_type", new []{ "password" } }
            };
            var validationContext = new OAuthValidateClientAuthenticationContext(new OwinContext(), new OAuthAuthorizationServerOptions(),
                new ReadableStringCollection(parameters));

            var authenticationString =
                Convert.ToBase64String(
                    ASCIIEncoding.ASCII.GetBytes("TestClient:fjasdfj8a9sfu908f8adf90sadsf908asf"));

            validationContext.Request.Headers["Authorization"] = $"Basic {authenticationString}";

            await ServerProvider.ValidateClientAuthentication(validationContext);

            Assert.True(validationContext.OwinContext.Get<string>(AuthorisationConstants.AllowedOrigin) == "http://mytest.local");
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldAttachAccessControlToResponse()
        {
            var allowedOrigin = "http://mytest.local";
            var owinContext = new OwinContext();
            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, allowedOrigin);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), "TestClient", "testUser", "testPassword", new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(credentialsContext.OwinContext.Response.Headers[AuthorisationConstants.AccessControlAllowOrigin] ==
                        allowedOrigin);
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldDefaultToAllIfAccessControlNotSpecified()
        {
            var owinContext = new OwinContext();
            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), "TestClient", "testUser", "testPassword", new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(credentialsContext.OwinContext.Response.Headers[AuthorisationConstants.AccessControlAllowOrigin] ==
                        "*");
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldNotGrant_WhenUserManagerNotFound()
        {
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));


            var owinContext = new OwinContext();

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, "wrongPassword", new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(!credentialsContext.IsValidated && credentialsContext.Error == OAuthErrors.ServerError);
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldNotGrant_WhenUserManagerThrowsException()
        {
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));

            var mockUserManager = new Mock<ApplicationUserManager<ApplicationUser>>(userStore);
            mockUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            var owinContext = new OwinContext();
            owinContext.Set<ApplicationUserManager<ApplicationUser>>(mockUserManager.Object);

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, Password, new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(!credentialsContext.IsValidated && credentialsContext.Error == OAuthErrors.ServerError);
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldNotGrant_WithInactiveUserCredentials()
        {
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));

            var mockUserManager = new Mock<ApplicationUserManager<ApplicationUser>>(userStore);
            mockUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<ApplicationUser>(new ApplicationUser
                {
                    Username = Username                    
                }));

            var owinContext = new OwinContext();
            owinContext.Set(mockUserManager.Object);

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, Password, new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(!credentialsContext.IsValidated && credentialsContext.Error == OAuthErrors.InvalidCredentials);
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldNotGrant_WithLockedOutUserCredentials()
        {
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));

            var mockUserManager = new Mock<ApplicationUserManager<ApplicationUser>>(userStore);
            mockUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<ApplicationUser>(new ApplicationUser
                {
                    Username = Username,
                    IsLockedOut = true,
                    IsApproved = true
                }));

            var owinContext = new OwinContext();
            owinContext.Set(mockUserManager.Object);

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, Password, new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(!credentialsContext.IsValidated && credentialsContext.Error == OAuthErrors.InvalidCredentials);
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldNotGrant_WithInactiveAndLockedOutUserCredentials()
        {
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));

            var mockUserManager = new Mock<ApplicationUserManager<ApplicationUser>>(userStore);
            mockUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<ApplicationUser>(new ApplicationUser
                {
                    Username = Username,
                    IsApproved = false,
                    IsLockedOut = true
                }));

            var owinContext = new OwinContext();
            owinContext.Set(mockUserManager.Object);

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, Password, new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(!credentialsContext.IsValidated && credentialsContext.Error == OAuthErrors.InvalidCredentials);
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldNotGrant_WithInvalidUserCredentials()
        {
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));

            var mockUserManager = new Mock<ApplicationUserManager<ApplicationUser>>(userStore);
            mockUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var owinContext = new OwinContext();
            owinContext.Set<ApplicationUserManager<ApplicationUser>>(mockUserManager.Object);

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, "wrongPassword", new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(!credentialsContext.IsValidated && credentialsContext.Error == OAuthErrors.InvalidCredentials);
        }


        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldGrant_WhenUserIsValid()
        {
            var claimsIdentity = new ClaimsIdentity();
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));


            var owinContext = new OwinContext();

            var mockUserManager = new Mock<ApplicationUserManager<ApplicationUser>>(userStore);
            mockUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ApplicationUser
            {
                Username = Username,
                IsApproved = true
            }));

            mockUserManager.Setup(x => x.CreateIdentityAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(claimsIdentity));

            owinContext.Set<ApplicationUserManager<ApplicationUser>>(mockUserManager.Object);

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, "wrongPassword", new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(credentialsContext.IsValidated && credentialsContext.Ticket.Identity == claimsIdentity);
        }

        [Fact]
        public async void GrantResourceOwnerCredentials_ShouldAttachAdditionalPropertiesToTicket()
        {
            var claimsIdentity = new ClaimsIdentity();
            var userStore =
                new UserStore<ApplicationUser>(
                    new IdentityDbContext<ApplicationUser>(Effort.DbConnectionFactory.CreateTransient(), true));


            var owinContext = new OwinContext();

            var mockUserManager = new Mock<ApplicationUserManager<ApplicationUser>>(userStore);
            mockUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ApplicationUser
            {
                Username = Username,
                IsApproved = true
            }));

            mockUserManager.Setup(x => x.CreateIdentityAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(claimsIdentity));

            owinContext.Set<ApplicationUserManager<ApplicationUser>>(mockUserManager.Object);

            owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, null);
            var credentialsContext = new OAuthGrantResourceOwnerCredentialsContext(owinContext, new OAuthAuthorizationServerOptions(), ClientId, Username, Password, new List<string>());

            await ServerProvider.GrantResourceOwnerCredentials(credentialsContext);

            Assert.True(credentialsContext.Ticket.Properties.Dictionary[AuthorisationConstants.ClientId] == ClientId && credentialsContext.Ticket.Properties.Dictionary["username"] == Username);
        }

        [Fact]
        public async void GrantRefreshToken_ShouldReturnError_WhenClientMismatch()
        {
            var owinContext = new OwinContext();
            var ticket = CreateMockTicket(Username, ClientId);

            var refreshTokenContext = new OAuthGrantRefreshTokenContext(owinContext,
                new OAuthAuthorizationServerOptions(), ticket, "mismatchClientId");

            await ServerProvider.GrantRefreshToken(refreshTokenContext);

            Assert.True(!refreshTokenContext.IsValidated && refreshTokenContext.Error == OAuthErrors.InvalidClientId);
        }

        [Fact]
        public async void GrantRefreshToken_ShouldValidateAndGrantRefreshToken()
        {
            var owinContext = new OwinContext();
            var ticket = CreateMockTicket(Username, ClientId);

            var refreshTokenContext = new OAuthGrantRefreshTokenContext(owinContext,
                new OAuthAuthorizationServerOptions(), ticket, ClientId);

            await ServerProvider.GrantRefreshToken(refreshTokenContext);

            Assert.True(refreshTokenContext.IsValidated && refreshTokenContext.Ticket.Identity.Name == Username);
        }

        [Fact]
        public async void TokenEndpoint_AdditionalPropertiesAreAttached()
        {
            var owinContext = new OwinContext();
            var ticket = CreateMockTicket(Username, ClientId);

            var tokenContext = new OAuthTokenEndpointContext(owinContext, new OAuthAuthorizationServerOptions(), ticket,
                new TokenEndpointRequest(new ReadableStringCollection(new Dictionary<string, string[]>())));

            await ServerProvider.TokenEndpoint(tokenContext);

            Assert.True(tokenContext.AdditionalResponseParameters[AuthorisationConstants.ClientId] as string == ClientId &&
                        tokenContext.AdditionalResponseParameters[AuthorisationConstants.Username] as string == Username);
        }

        private AuthenticationTicket CreateMockTicket(string userName, string clientId)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userName)
            };

            var identity = new ClaimsIdentity(claims, "authType");

            return new AuthenticationTicket(identity, new AuthenticationProperties(
                new Dictionary<string, string>
                {
                    {
                        AuthorisationConstants.ClientId, clientId
                    },
                    {
                        AuthorisationConstants.Username, userName
                    }
                }));
        }

    }
}
