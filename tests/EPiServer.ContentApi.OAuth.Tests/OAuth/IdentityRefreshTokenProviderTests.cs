using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using EPiServer.ContentApi.OAuth.Internal;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataHandler.Serializer;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.OAuth.Tests.OAuth
{
	public class IdentityRefreshTokenProviderTests
	{

		internal IdentityRefreshTokenProvider RefreshTokenProvider;
		internal Mock<IRefreshTokenRepository> RefreshTokenRepository;
		internal TimeSpan RefreshTokenExpiration;

		internal const string Username = "testUser";
		internal const string ClientId = "TestClient";

		public IdentityRefreshTokenProviderTests()
		{
			RefreshTokenRepository = new Mock<IRefreshTokenRepository>();
			RefreshTokenRepository.Setup(repo => repo.CreateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns(
					(string hashValue, string clientId, string subject, DateTime issuedUtc, DateTime expiresUtc) =>
					{
						return new RefreshToken
						{
							RefreshTokenValue = hashValue,
							ClientId = clientId,
							Subject = subject,
							IssuedUtc = issuedUtc,
							ExpiresUtc = expiresUtc
						};
					}
				);

			RefreshTokenRepository.Setup(repo => repo.Add(It.IsAny<RefreshToken>())).Returns(System.Guid.NewGuid());

			RefreshTokenExpiration = TimeSpan.FromHours(2);

			RefreshTokenProvider = new IdentityRefreshTokenProvider(new ContentApiOAuthOptions()
			{
				RefreshTokenExpireTimeSpan = RefreshTokenExpiration
			}, RefreshTokenRepository.Object);
		}

		[Fact]
		public void CreateAsync_ShouldNotCreateRefreshTokenWithoutClientId()
		{
			var ticket = CreateMockTicket(Username, "");

			var tokenCreateContext = GetMockTokenCreateContext(ticket);
			RefreshTokenProvider.CreateAsync(tokenCreateContext);

			Assert.True(String.IsNullOrEmpty(tokenCreateContext.Token));
		}

		[Fact]
		public void CreateAsync_ShouldCalculateRefreshTokenTimespanCorrectly()
		{
			var ticket = CreateMockTicket(Username, ClientId);
			var tokenCreateContext = GetMockTokenCreateContext(ticket);

			RefreshTokenProvider.CreateAsync(tokenCreateContext);

			Assert.True(tokenCreateContext.Ticket.Properties.IssuedUtc.Value.Add(RefreshTokenExpiration) == tokenCreateContext.Ticket.Properties.ExpiresUtc);
		}

		[Fact]
		public void CreateAsync_ShouldAddRefreshToken()
		{
			var ticket = CreateMockTicket(Username, ClientId);

			var tokenCreateContext = GetMockTokenCreateContext(ticket);

			RefreshTokenProvider.CreateAsync(tokenCreateContext);

			RefreshTokenRepository.Verify(repo => repo.Add(It.IsAny<IRefreshToken>()), Times.Once);
		}

		[Fact]
		public void CreateAsync_ShouldSetTokenOnContext()
		{
			var ticket = CreateMockTicket(Username, ClientId);

			var tokenCreateContext = GetMockTokenCreateContext(ticket);

			RefreshTokenProvider.CreateAsync(tokenCreateContext);

			Assert.True(!String.IsNullOrEmpty(tokenCreateContext.Token));
		}

        [Fact]
        public void CreateAsync_ShouldRemoveExistedTokenOfRequestingIdentity()
        {
            var ticket = CreateMockTicket(Username, ClientId);
            var tokenCreateContext = GetMockTokenCreateContext(ticket);

            var refreshToken = new RefreshToken()
            {
                ClientId = ClientId,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(1)
            };

            RefreshTokenRepository.Setup(repo => repo.FindByUsername(It.IsAny<string>())).Returns(new List<IRefreshToken>() { refreshToken });

            RefreshTokenProvider.CreateAsync(tokenCreateContext);

            RefreshTokenRepository.Verify(repo => repo.Remove(It.IsAny<IRefreshToken>()), Times.Once);
        }

        [Fact]
        public void CreateAsync_NewlyCreatedTokenShouldHaveSameExpireDateWithExistedOne_WhenGrantTypeIsRefreshToken()
        {
            var ticket = CreateMockTicket(Username, ClientId);
            var tokenCreateContext = GetMockTokenCreateContext(ticket);
            tokenCreateContext.OwinContext.Set(AuthorisationConstants.GrantType, "refresh_token");

            var refreshToken = new RefreshToken()
            {
                ClientId = ClientId,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(1)
            };

            RefreshTokenRepository.Setup(repo => repo.FindByUsername(It.IsAny<string>())).Returns(new List<IRefreshToken>() { refreshToken });

            RefreshTokenProvider.CreateAsync(tokenCreateContext);

            // We need to remove milisecond of refreshToken.ExpiresUtc before do the assertion because tokenCreateContext.Ticket.Properties.ExpiresUtc does not store milisecond
            refreshToken.ExpiresUtc = refreshToken.ExpiresUtc.AddTicks(-(refreshToken.ExpiresUtc.Ticks % TimeSpan.TicksPerSecond));

            Assert.True(tokenCreateContext.Ticket.Properties.ExpiresUtc.Value.DateTime == refreshToken.ExpiresUtc);
        }

        [Fact]
        public void CreateAsync_NewlyCreatedTokenShouldHaveNewExpireDate_WhenGrantTypeIsPassword()
        {
            var ticket = CreateMockTicket(Username, ClientId);
            var tokenCreateContext = GetMockTokenCreateContext(ticket);
            tokenCreateContext.OwinContext.Set(AuthorisationConstants.GrantType, "password");

            var refreshToken = new RefreshToken()
            {
                ClientId = ClientId,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(-1)
            };

            RefreshTokenRepository.Setup(repo => repo.FindByUsername(It.IsAny<string>())).Returns(new List<IRefreshToken>() { refreshToken });

            RefreshTokenProvider.CreateAsync(tokenCreateContext);

            Assert.True(tokenCreateContext.Ticket.Properties.IssuedUtc.Value.Add(RefreshTokenExpiration) == tokenCreateContext.Ticket.Properties.ExpiresUtc);
        }

        [Fact]
		public void ReceiveAsync_ShouldSetAllowOriginFromOwinContext()
		{
			string allowedOrigin = "http://testorigin.local:80";
			var owinContext = new OwinContext();
			owinContext.Set<string>(AuthorisationConstants.AllowedOrigin, allowedOrigin);

			var receiveContext = new AuthenticationTokenReceiveContext(owinContext, CreateSecureDataFormatForCreate(), "tokenvalue");

			RefreshTokenProvider.ReceiveAsync(receiveContext);

			Assert.True(receiveContext.OwinContext.Response.Headers["Access-Control-Allow-Origin"] == allowedOrigin);
		}

		[Fact]
		public void ReceiveAsync_ShouldDeserializeLocatedRefreshToken()
		{
			var refreshTokenRepository = CreateLimitedMockRefreshTokenRepository();

			var secureDataFormat = CreateSecureDataFormatForReceive();

			var refreshTokenProvider = new IdentityRefreshTokenProvider(new ContentApiOAuthOptions()
			{
				RefreshTokenExpireTimeSpan = RefreshTokenExpiration
			}, refreshTokenRepository.Object);

			var owinContext = new OwinContext();

			var receiveContext = new AuthenticationTokenReceiveContext(owinContext, secureDataFormat, "token");

			refreshTokenProvider.ReceiveAsync(receiveContext);

			Assert.NotNull(receiveContext.Ticket);
		}		

		[Fact]
		public void ReceiveAsync_ShouldNotRemoveRefreshTokenOnceDeserialized_WhenClientsDontMatch()
		{
			var refreshTokenRepository = CreateLimitedMockRefreshTokenRepository();

			var secureDataFormat = CreateSecureDataFormatForReceive();

			var refreshTokenProvider = new IdentityRefreshTokenProvider(new ContentApiOAuthOptions()
			{
				RefreshTokenExpireTimeSpan = RefreshTokenExpiration
			}, refreshTokenRepository.Object);

			var owinContext = new OwinContext();
			owinContext.Set(AuthorisationConstants.ClientId, "InvalidClient");
			var receiveContext = new AuthenticationTokenReceiveContext(owinContext, secureDataFormat, "token");

			refreshTokenProvider.ReceiveAsync(receiveContext);

			refreshTokenRepository.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
		}

		private static Mock<IRefreshTokenRepository> CreateLimitedMockRefreshTokenRepository()
		{
			var refreshTokenRepository = new Mock<IRefreshTokenRepository>();

			refreshTokenRepository
				.Setup(x => x.FindByValue(It.IsAny<string>()))
				.Returns((new RefreshToken()
				{
					ClientId = ClientId,
					IssuedUtc = DateTime.UtcNow,
					ExpiresUtc = DateTime.UtcNow.AddDays(2),
					RefreshTokenValue = "test",
					ProtectedTicket = "protected",
					Subject = Username
				}));
			return refreshTokenRepository;
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

		private AuthenticationTokenCreateContext GetMockTokenCreateContext(AuthenticationTicket ticket)
		{
			var tokenCreateContext = new AuthenticationTokenCreateContext(new OwinContext(), CreateSecureDataFormatForCreate(), ticket);
            return tokenCreateContext;
		}

		private ISecureDataFormat<AuthenticationTicket> CreateSecureDataFormatForReceive()
		{
			Mock<IDataProtector> mockDataProtector = new Mock<IDataProtector>();
			mockDataProtector.Setup(sut => sut.Protect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("protectedText"));
			mockDataProtector.Setup(sut => sut.Unprotect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("originalText"));

			var secureDataFormat = new Mock<ISecureDataFormat<AuthenticationTicket>>();
			secureDataFormat.Setup(x => x.Unprotect(It.IsAny<string>())).Returns(CreateMockTicket(Username, ClientId));
			return secureDataFormat.Object;
		}

		private SecureDataFormat<AuthenticationTicket> CreateSecureDataFormatForCreate()
		{
			Mock<IDataProtector> mockDataProtector = new Mock<IDataProtector>();
			mockDataProtector.Setup(sut => sut.Protect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("protectedText"));
			mockDataProtector.Setup(sut => sut.Unprotect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("originalText"));

			var secureDataFormat = new SecureDataFormat<AuthenticationTicket>(new TicketSerializer(),
				mockDataProtector.Object, new Base64TextEncoder());
			return secureDataFormat;
		}
	}
}
