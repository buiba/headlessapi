using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Castle.DynamicProxy.Generators;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Internal
{
    public class DefaultSiteDefinitionConverterTests : IDisposable
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
        private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de");
        private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da");
        private static readonly CultureInfo Swedish = CultureInfo.GetCultureInfo("sv");

        private readonly Mock<IContentLoader> _contentLoader;
        private readonly Dictionary<string, ContentReference> _contentRoots;
        private readonly Mock<ContentRootRepository> _contentRootRepository;
        private readonly Mock<IContentModelReferenceConverter> _contentModelReferenceConverter;
        private readonly Mock<ILanguageBranchRepository> _languageBranchRepository;
        private readonly ConverterContext _context;
        private readonly DefaultSiteDefinitionConverter _subject;

        public DefaultSiteDefinitionConverterTests()
        {
            _contentLoader = new Mock<IContentLoader>();
            _contentLoader.Setup(x => x.Get<PageData>(It.IsAny<ContentReference>())).Returns(new PageData());

            _contentModelReferenceConverter = new Mock<IContentModelReferenceConverter>();
            _contentModelReferenceConverter.Setup(x => x.GetContentModelReference(It.IsAny<ContentReference>()))
                .Returns<ContentReference>(x => x is null ? null : new ContentModelReference { Id = x.ID });

            var rootPage = new ContentReference(1);
            var wasteBasket = new ContentReference(2);
            var globalAssetsRoot = new ContentReference(3);
            var contentAssetsRoot = new ContentReference(4);
            SystemDefinition.Current = new SystemDefinition(rootPage, wasteBasket, globalAssetsRoot, contentAssetsRoot);

            _contentRoots = new Dictionary<string, ContentReference>
            {
                { SystemContentRootNames.RootPage, rootPage },
                { SystemContentRootNames.WasteBasket, wasteBasket },
                { SystemContentRootNames.GlobalAssets, globalAssetsRoot },
                { SystemContentRootNames.ContentAssets, contentAssetsRoot },
            };

            _contentRootRepository = new Mock<ContentRootRepository>();
            _contentRootRepository.Setup(x => x.List()).Returns(_contentRoots);

            _languageBranchRepository = new Mock<ILanguageBranchRepository>();
            _languageBranchRepository.Setup(x => x.Load(It.IsAny<CultureInfo>())).Returns<CultureInfo>(c => new LanguageBranch(c) { URLSegment = c.ThreeLetterISOLanguageName /* Testable convention */ });

            _context = new ConverterContext(new ContentApiConfiguration().GetOptions(), null, null, true, CultureInfo.InvariantCulture, ContextMode.Default);

            _subject = new DefaultSiteDefinitionConverter(
                _contentLoader.Object,
                _contentModelReferenceConverter.Object,
                _contentRootRepository.Object,
                _languageBranchRepository.Object);
        }

        public void Dispose()
        {
            SystemDefinition.Current = null;
        }

        [Fact]
        public void Convert_ShouldMapBasicProperties()
        {
            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site"
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Equal(site.Id, siteModel.Id);
            Assert.Equal(site.Name, siteModel.Name);
        }

        [Fact]
        public void Convert_ShouldMapHosts()
        {
            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                Hosts = new[]
                {
                    new HostDefinition { Name = "one.com" },
                    new HostDefinition { Name = "two.com", Type = HostDefinitionType.Primary },
                    new HostDefinition { Name = "three.com", Type = HostDefinitionType.Primary, Language = English },
                    new HostDefinition { Name = "four.com", Type = HostDefinitionType.Edit },
                    new HostDefinition { Name = "five.com", UseSecureConnection = true },
                }
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Collection(siteModel.Hosts,
                s =>
                {
                    Assert.Equal(site.Hosts[0].Name, s.Name);
                    Assert.Equal(nameof(HostDefinitionType.Undefined), s.Type);
                },
                s =>
                {
                    Assert.Equal(site.Hosts[1].Name, s.Name);
                    Assert.Equal(nameof(HostDefinitionType.Primary), s.Type);
                },
                s =>
                {
                    Assert.Equal(site.Hosts[2].Name, s.Name);
                    Assert.Equal(nameof(HostDefinitionType.Primary), s.Type);
                    Assert.Equal(site.Hosts[2].Language.Name, s.Language.Name);
                    Assert.Equal(site.Hosts[2].Language.DisplayName, s.Language.DisplayName);
                },
                s =>
                {
                    Assert.Equal(site.Hosts[3].Name, s.Name);
                    Assert.Equal(nameof(HostDefinitionType.Edit), s.Type);
                },
                s =>
                {
                    Assert.Equal(site.Hosts[4].Name, s.Name);
                    Assert.Equal(nameof(HostDefinitionType.Undefined), s.Type);
                });
        }

        [Fact]
        public void Convert_WhenOptionIsSetToExcludeHosts_ShouldSetHostsToNull()
        {
            _context.Options.SetIncludeSiteHosts(false);

            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                Hosts = new[] { new HostDefinition { Name = "one.com" } }
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Null(siteModel.Hosts);
        }

        [Fact]
        public void Convert_ShouldMapDefaultContentRoots()
        {
            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                StartPage = new ContentReference(10),
                SiteAssetsRoot = new ContentReference(11),
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Collection(siteModel.ContentRoots.OrderBy(x => x.Key),
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.ContentAssetsRoot), x.Key);
                    Assert.Equal(SystemDefinition.Current.ContentAssetsRoot.ID, x.Value.Id);
                },
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.GlobalAssetsRoot), x.Key);
                    Assert.Equal(SystemDefinition.Current.GlobalAssetsRoot.ID, x.Value.Id);
                },
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.RootPage), x.Key);
                    Assert.Equal(SystemDefinition.Current.RootPage.ID, x.Value.Id);
                },
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.SiteAssetsRoot), x.Key);
                    Assert.Equal(site.SiteAssetsRoot.ID, x.Value.Id);
                },
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.StartPage), x.Key);
                    Assert.Equal(site.StartPage.ID, x.Value.Id);
                },
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.WasteBasket), x.Key);
                    Assert.Equal(site.WasteBasket.ID, x.Value.Id);
                });
        }

        [Fact]
        public void Convert_WhenOptionIsSetToExcludeInternalRoots_ShouldMapDefaultContentRoots()
        {
            _context.Options.SetIncludeInternalContentRoots(false);

            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                StartPage = new ContentReference(10),
                SiteAssetsRoot = new ContentReference(11),
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Collection(siteModel.ContentRoots.OrderBy(x => x.Key),
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.GlobalAssetsRoot), x.Key);
                    Assert.Equal(SystemDefinition.Current.GlobalAssetsRoot.ID, x.Value.Id);
                },
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.SiteAssetsRoot), x.Key);
                    Assert.Equal(site.SiteAssetsRoot.ID, x.Value.Id);
                },
                x =>
                {
                    Assert.Equal(nameof(SiteDefinition.StartPage), x.Key);
                    Assert.Equal(site.StartPage.ID, x.Value.Id);
                });
        }

        [Fact]
        public void Convert_WhenSiteDoesNotHaveAnySiteAssetRoot_ShouldNotIncludeSiteAssetRoot()
        {
            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                StartPage = new ContentReference(10),
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.DoesNotContain(nameof(SiteDefinition.SiteAssetsRoot), siteModel.ContentRoots.Keys);
        }

        [Fact]
        public void Convert_ShouldMapCustomContentRoots()
        {
            _contentRoots["Custom"] = new ContentReference(100);

            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                StartPage = new ContentReference(10),
                SiteAssetsRoot = new ContentReference(11),
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Contains("Custom", siteModel.ContentRoots.Keys);
            Assert.Equal(100, siteModel.ContentRoots["Custom"].Id);
        }

        [Fact]
        public void Convert_ShouldMapLanguageFromTheStartPage()
        {
            var languages = new[] { English, German };

            var startPage = CreatePageData(languages);

            _contentLoader.Setup(x => x.Get<PageData>(startPage.ContentLink)).Returns(startPage);

            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                StartPage = startPage.ContentLink
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Collection(siteModel.Languages,
                x =>
                {
                    Assert.Equal(languages[0].Name, x.Name);
                    Assert.Equal(languages[0].DisplayName, x.DisplayName);
                    Assert.Equal(languages[0].ThreeLetterISOLanguageName, x.UrlSegment); // Mock uses testable convention
                    Assert.True(x.IsMasterLanguage);
                },
                x => {
                    Assert.Equal(languages[1].Name, x.Name);
                    Assert.Equal(languages[1].DisplayName, x.DisplayName);
                    Assert.Equal(languages[1].ThreeLetterISOLanguageName, x.UrlSegment); // Mock uses testable convention
                    Assert.False(x.IsMasterLanguage);
                });
        }

        [Fact]
        public void Convert_WhenOptionIsSetToExcludeMasterLanguage_ShouldNotSetMasterLanguagePropertyOnLanguage()
        {
            _context.Options.SetIncludeMasterLanguage(false);

            // First is master
            var languages = new[] { English, German };

            var startPage = CreatePageData(languages);

            _contentLoader.Setup(x => x.Get<PageData>(startPage.ContentLink)).Returns(startPage);

            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                StartPage = startPage.ContentLink
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.False(siteModel.Languages.Single(x => x.Name == English.Name).IsMasterLanguage);
        }

        [Fact]
        public void Convert_WhenOptionIsSetToExcludeHosts_ShouldMapLanguagesWithLocation()
        {
            _context.Options.SetIncludeSiteHosts(false);

            var languages = new[] { English, German, Danish, Swedish };

            var startPage = CreatePageData(languages);

            _contentLoader.Setup(x => x.Get<PageData>(startPage.ContentLink)).Returns(startPage);

            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                StartPage = startPage.ContentLink,
                Hosts = new[]
                {
                    new HostDefinition { Name = "undefined.org" },
                    new HostDefinition { Name = "edit.com", Type = HostDefinitionType.Edit },
                    new HostDefinition { Name = "primary.org", Type = HostDefinitionType.Primary },
                    new HostDefinition { Name = "redirect.com", Type = HostDefinitionType.RedirectPermanent, Language = English },
                    new HostDefinition { Name = "primary.com", Type = HostDefinitionType.Primary, Language = English },
                    new HostDefinition { Name = "redirect1.se", Language = Swedish, Type = HostDefinitionType.RedirectPermanent },
                    new HostDefinition { Name = "redirect1.se", Language = Swedish, Type = HostDefinitionType.RedirectTemporary },
                    new HostDefinition { Name = "undefined1.se", Language = Swedish },
                    new HostDefinition { Name = "undefined2.se", Language = Swedish },
                    new HostDefinition { Name = "secure.da", UseSecureConnection = true, Language = Danish },
                }
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Collection(siteModel.Languages,
                x =>
                {
                    Assert.Equal(English.Name, x.Name);
                    Assert.Equal("http://primary.com/", x.Url);
                },
                x => 
                {
                    Assert.Equal(German.Name, x.Name);
                    Assert.Equal($"http://primary.org/{German.ThreeLetterISOLanguageName}/", x.Url);
                },
                x =>
                {
                    Assert.Equal(Danish.Name, x.Name);
                    Assert.Equal("https://secure.da/", x.Url);
                },
                x =>
                {
                    Assert.Equal(Swedish.Name, x.Name);
                    Assert.Equal("http://undefined1.se/", x.Url);
                });
        }

        [Fact]
        public void Convert_WhenEditHostIsDefinedAndOptionIsSetToExcludeHosts_ShouldAssignEditLocation()
        {
            _context.Options.SetIncludeSiteHosts(false);

            var site = new SiteDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Test site",
                Hosts = new[]
                {
                    new HostDefinition { Name = "undefined.org" },
                    new HostDefinition { Name = "primary.org", Type = HostDefinitionType.Primary },
                    new HostDefinition { Name = "edit.org", UseSecureConnection = true, Type = HostDefinitionType.Edit },
                }
            };

            var siteModel = _subject.Convert(site, _context);

            Assert.Equal("https://edit.org/", siteModel.EditLocation);
        }

        private static PageData CreatePageData(CultureInfo[] languages)
        {
            var properties = new PropertyDataCollection
            {
                { "PageLink", new PropertyPageReference(new PageReference(155)) },
                { "PageMasterLanguageBranch", new PropertyString(languages[0].Name) },
            };

            return new PageData(new AccessControlList(), properties) { ExistingLanguages = languages };
        }
    }
}
