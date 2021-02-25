using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Security
{

    public class CorsPolicyServiceTest
    {
        private SiteDefinition _siteDefinition;
        private Mock<ISiteDefinitionResolver> _siteDefinitionResolver;
        private Mock<ILanguageBranchRepository> _languageBranchRepository;        
        private CorsPolicyService _subject;
        private ContentApiConfiguration _apiConfiguration;
        private HttpRequestMessage _requestMessage;
        private List<ContentApiClient> _clientList;

        public CorsPolicyServiceTest() 
        {
            _apiConfiguration = new ContentApiConfiguration();            
            _languageBranchRepository = new Mock<ILanguageBranchRepository>();
            _siteDefinitionResolver = new Mock<ISiteDefinitionResolver>();

            _siteDefinition = new SiteDefinition()
            {
                Id = Guid.NewGuid(),
                Name = "site one",
                SiteUrl = new Uri("http://one.com/"),
                StartPage = new ContentReference(5),
                Hosts = new List<HostDefinition>()
                {
                    new HostDefinition()
                    {
                        Name = "one.com",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("en")
                    },
                     new HostDefinition()
                    {
                        Name = "sharedhost.com",
                        Type = HostDefinitionType.Primary
                    },
                      new HostDefinition()
                    {
                        Name = "cd.host01",
                        Type = HostDefinitionType.Primary,
                        Language = new CultureInfo("sv")
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host02",
                        Type = HostDefinitionType.Edit
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host03",
                        Type = HostDefinitionType.RedirectPermanent
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host04",
                        Type = HostDefinitionType.RedirectTemporary
                    },
                    new HostDefinition()
                    {
                        Name = "cd.host05",
                        Type = HostDefinitionType.Undefined
                    },
                    new HostDefinition()
                    {
                        Name = "*",
                        Type = HostDefinitionType.Undefined
                    },
                }
            };

            var matchedHost = new HostDefinition();

            _siteDefinitionResolver.Setup(resolver => resolver.GetByHostname(It.IsAny<string>(), It.IsAny<bool>(), out matchedHost))
                                   .Returns(_siteDefinition);
            _languageBranchRepository.Setup(repo => repo.ListEnabled())
                                     .Returns(new List<LanguageBranch>() { new LanguageBranch(new CultureInfo("en")), new LanguageBranch(new CultureInfo("sv")) });

            _subject = new CorsPolicyService(_apiConfiguration, _siteDefinitionResolver.Object, _languageBranchRepository.Object);
            _requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://one.com/")
            };

            _clientList = new List<ContentApiClient>()
            {
                new ContentApiClient()
                {
                    ClientId = "01",
                    AccessControlAllowOrigin = "http://cd.com"
                },
                new ContentApiClient()
                {
                    ClientId = "02",
                    AccessControlAllowOrigin = "http://cd02.com"
                }
            };
        }

        [Fact]
        public void GetHostInfo_WhenPrimaryAndUndefinedHostsExistInDBAndNoClientConfiguration_ShouldSetOriginsInPolicy() 
        {
            _apiConfiguration.Default().SetClients(null);

            var policy = _subject.GetOrCreatePolicy(_requestMessage);

            Assert.True(policy.Origins.Contains("http://one.com"));
            Assert.True(policy.Origins.Contains("http://cd.host01"));
            Assert.True(policy.Origins.Contains("http://sharedhost.com"));
            Assert.True(policy.Origins.Contains("http://cd.host05"));
            Assert.False(policy.Origins.Contains("*"));
            Assert.True(policy.AllowAnyHeader);
            Assert.True(policy.AllowAnyMethod);
            Assert.False(policy.AllowAnyOrigin);
            Assert.True(policy.SupportsCredentials);
        }

        [Fact]
        public void GetHostInfo_WhenPrimaryAndUndefinedHostsExistInDBAndHaveClientConfigurations_ShouldSetOriginsInPolicy()
        {
            _apiConfiguration.Default().SetClients(_clientList);

            var policy = _subject.GetOrCreatePolicy(_requestMessage);
            Assert.True(policy.Origins.Contains("http://one.com"));
            Assert.True(policy.Origins.Contains("http://cd.host01"));
            Assert.True(policy.Origins.Contains("http://sharedhost.com"));
            Assert.True(policy.Origins.Contains("http://cd.com"));
            Assert.True(policy.Origins.Contains("http://cd02.com"));
            Assert.True(policy.Origins.Contains("http://cd.host05"));
            Assert.False(policy.Origins.Contains("*"));
            Assert.True(policy.AllowAnyHeader);
            Assert.True(policy.AllowAnyMethod);
            Assert.False(policy.AllowAnyOrigin);
            Assert.True(policy.SupportsCredentials);
        }

        [Fact]
        public void GetHostInfo_WhenClientsConfigAllowAllOrigin_ShouldSetOriginsInPolicy()
        {
            var clients = new List<ContentApiClient>()
            {
                new ContentApiClient()
                {
                    ClientId = "01",
                    AccessControlAllowOrigin = "*"
                }
            };
            _apiConfiguration.Default().SetClients(clients);

            _subject = new CorsPolicyService(_apiConfiguration, _siteDefinitionResolver.Object, _languageBranchRepository.Object);
            var policy = _subject.GetOrCreatePolicy(_requestMessage);

            Assert.False(policy.Origins.Any());
            Assert.True(policy.AllowAnyOrigin);
            Assert.False(policy.SupportsCredentials);
        }

        [Fact]
        public void GetHostInfo_WhenCannotResolveTheSite_ShouldReturnPolicyAccordingToConfiguration()
        {
            _apiConfiguration.Default().SetClients(_clientList);
            SiteDefinition siteDefinition = null;
            HostDefinition hostDefinition = null;
            _siteDefinitionResolver.Setup(resolver => resolver.GetByHostname(It.IsAny<string>(), It.IsAny<bool>(), out hostDefinition))
                                   .Returns(siteDefinition);

            var policy = _subject.GetOrCreatePolicy(_requestMessage);

            _languageBranchRepository.Verify(repo => repo.ListEnabled(), Times.Never);
            Assert.True(policy.Origins.Contains("http://cd.com"));
            Assert.True(policy.Origins.Contains("http://cd02.com"));
        }

        [Fact]
        public void GetHostInfo_WhenPolicyIsAdded_ShouldAddPolicyToCache()
        {
            _apiConfiguration.Default().SetClients(_clientList);

            _subject.GetOrCreatePolicy(_requestMessage);
            
            Assert.True(_subject._cache.Keys.Contains(_requestMessage.RequestUri.Authority));
        }

        [Fact]
        public void GetHostInfo_WhenPolicyExist_ShouldReturnExistingPolicyInCache()
        {
            var hostDefinition = new HostDefinition();
            _apiConfiguration.Default().SetClients(_clientList);

            _subject.GetOrCreatePolicy(_requestMessage);
            Assert.True(_subject._cache.Keys.Contains(_requestMessage.RequestUri.Authority));
            _subject.GetOrCreatePolicy(_requestMessage);
            Assert.Equal(1, _subject._cache.Keys.Count);
            Assert.True(_subject._cache.Keys.Contains(_requestMessage.RequestUri.Authority));
            _siteDefinitionResolver.Verify(resolver => resolver.GetByHostname(It.IsAny<string>(), It.IsAny<bool>(), out hostDefinition), Times.Once);
        }
    }
}
