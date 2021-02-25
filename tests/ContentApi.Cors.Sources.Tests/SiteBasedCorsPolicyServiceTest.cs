using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Cors.Internal
{
    public class SiteBasedCorsPolicyServiceTest
    {
        private readonly SiteDefinition _siteDefinition;
        private readonly Mock<ISiteDefinitionResolver> _siteDefinitionResolver;
        private readonly Mock<ILanguageBranchRepository> _languageBranchRepository;
        private readonly SiteBasedCorsPolicyService _subject;
        private readonly HttpRequestMessage _requestMessage;
        private readonly CorsOptions _corsOptions;

        public SiteBasedCorsPolicyServiceTest()
        {
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
            _corsOptions = new CorsOptions();

            _subject = new SiteBasedCorsPolicyService(_siteDefinitionResolver.Object, _languageBranchRepository.Object, _corsOptions);
            _requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://one.com/")
            };
        }

        [Fact]
        public void GetHostInfo_WhenPrimaryAndUndefinedHostsExistInDBAndNoCorsOptionIsConfigured_ShouldSetOriginsInPolicy()
        {
            _corsOptions.Policies = new Dictionary<string, CorsPolicy>(StringComparer.Ordinal);
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
            _corsOptions.Policies = new Dictionary<string, CorsPolicy>(StringComparer.Ordinal);
            var configurePolicy = new CorsPolicy() { AllowAnyHeader = true, AllowAnyMethod = true, AllowAnyOrigin = false };
            configurePolicy.Origins.Add("http://test.com");
            configurePolicy.Origins.Add("http://test2.com");
            _corsOptions.AddPolicy("one.com", configurePolicy);

            var policy = _subject.GetOrCreatePolicy(_requestMessage);
            Assert.True(policy.Origins.Contains("http://one.com"));
            Assert.True(policy.Origins.Contains("http://cd.host01"));
            Assert.True(policy.Origins.Contains("http://sharedhost.com"));
            Assert.True(policy.Origins.Contains("http://cd.host05"));
            Assert.True(policy.Origins.Contains("http://test.com"));
            Assert.True(policy.Origins.Contains("http://test2.com"));
            Assert.False(policy.Origins.Contains("*"));
            Assert.True(policy.AllowAnyHeader);
            Assert.True(policy.AllowAnyMethod);
            Assert.False(policy.AllowAnyOrigin);
            Assert.True(policy.SupportsCredentials);
        }

        [Fact]
        public void GetHostInfo_WhenClientsConfigAllowAllOrigin_ShouldSetOriginsInPolicy()
        {
            _corsOptions.Policies = new Dictionary<string, CorsPolicy>(StringComparer.Ordinal);
            var configurePolicy = new CorsPolicy() { AllowAnyHeader = true, AllowAnyMethod = true, AllowAnyOrigin = true };
            _corsOptions.AddPolicy("one.com", configurePolicy);

            var policy = _subject.GetOrCreatePolicy(_requestMessage);

            Assert.False(policy.Origins.Any());
            Assert.True(policy.AllowAnyOrigin);
            Assert.False(policy.SupportsCredentials);
        }

        [Fact]
        public void GetHostInfo_WhenCannotResolveTheSite_ShouldReturnPolicyAccordingToConfiguration()
        {
            _corsOptions.Policies = new Dictionary<string, CorsPolicy>(StringComparer.Ordinal);
            var configurePolicy = new CorsPolicy() { AllowAnyHeader = true, AllowAnyMethod = true, AllowAnyOrigin = false };
            configurePolicy.Origins.Add("http://test.com");
            configurePolicy.Origins.Add("http://test2.com");
            _corsOptions.AddPolicy("one.com", configurePolicy);

            SiteDefinition siteDefinition = null;
            HostDefinition hostDefinition = null;
            _siteDefinitionResolver.Setup(resolver => resolver.GetByHostname(It.IsAny<string>(), It.IsAny<bool>(), out hostDefinition))
                                   .Returns(siteDefinition);

            var policy = _subject.GetOrCreatePolicy(_requestMessage);

            _languageBranchRepository.Verify(repo => repo.ListEnabled(), Times.Never);
            Assert.True(policy.Origins.Contains("http://test.com"));
            Assert.True(policy.Origins.Contains("http://test2.com"));
        }

        [Fact]
        public void GetHostInfo_WhenPolicyIsAdded_ShouldAddPolicyToCache()
        {
            _corsOptions.Policies = new Dictionary<string, CorsPolicy>(StringComparer.Ordinal);
            var configurePolicy = new CorsPolicy() { AllowAnyHeader = true, AllowAnyMethod = true, AllowAnyOrigin = false };
            configurePolicy.Origins.Add("http://test.com");
            configurePolicy.Origins.Add("http://test2.com");
            _corsOptions.AddPolicy("one.com", configurePolicy);

            _subject.GetOrCreatePolicy(_requestMessage);

            Assert.True(_subject.CachePolicies.Keys.Contains(_requestMessage.RequestUri.Authority));
        }

        [Fact]
        public void GetHostInfo_WhenPolicyExist_ShouldReturnExistingPolicyInCache()
        {
            _corsOptions.Policies = new Dictionary<string, CorsPolicy>(StringComparer.Ordinal);
            var configurePolicy = new CorsPolicy() { AllowAnyHeader = true, AllowAnyMethod = true, AllowAnyOrigin = false };
            configurePolicy.Origins.Add("http://test.com");
            configurePolicy.Origins.Add("http://test2.com");
            _corsOptions.AddPolicy("one.com", configurePolicy);

            _subject.GetOrCreatePolicy(_requestMessage);
            Assert.True(_subject.CachePolicies.Keys.Contains(_requestMessage.RequestUri.Authority));
            _subject.GetOrCreatePolicy(_requestMessage);
            Assert.Equal(1, _subject.CachePolicies.Keys.Count);
            Assert.True(_subject.CachePolicies.Keys.Contains(_requestMessage.RequestUri.Authority));

            HostDefinition hostDefinition;
            _siteDefinitionResolver.Verify(repo => repo.GetByHostname(It.IsAny<string>(), It.IsAny<bool>(), out hostDefinition), Times.Once);
        }
    }
}
