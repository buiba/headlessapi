using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    [ServiceConfiguration(typeof(ISiteDefinitionConverter), Lifecycle = ServiceInstanceScope.Singleton, IncludeServiceAccessor = false)]
    internal class DefaultSiteDefinitionConverter : ISiteDefinitionConverter
    {
        // Silly enough, we are using different names than CMS is in the ContentRootRepository - Candidate for a V3 change?
        private const string ContentAssetsRoot = "ContentAssetsRoot";
        private const string GlobalAssetsRoot = "GlobalAssetsRoot";
        private const string RootPage = SystemContentRootNames.RootPage;
        private const string WasteBasket = SystemContentRootNames.WasteBasket;
        private const string StartPage = "StartPage";
        private const string SiteAssetsRoot = "SiteAssetsRoot";
        // These are the names as registered in the ContentRootRepository
        private static readonly HashSet<string> DefaultCmsContentRootNames = new HashSet<string>
        {
            SystemContentRootNames.ContentAssets,
            SystemContentRootNames.GlobalAssets,
            SystemContentRootNames.RootPage,
            SystemContentRootNames.WasteBasket,
            StartPage,
            "SiteAssets"
        };

        private readonly IContentLoader _contentLoader;
        private readonly IContentModelReferenceConverter _contentModelService;
        private readonly ContentRootRepository _contentRootRepository;
        private readonly ILanguageBranchRepository _languageBranchRepository;

        public DefaultSiteDefinitionConverter(IContentLoader contentLoader, IContentModelReferenceConverter contentModelService, ContentRootRepository contentRootRepository, ILanguageBranchRepository languageBranchRepository)
        {
            _contentLoader = contentLoader;
            _contentModelService = contentModelService;
            _contentRootRepository = contentRootRepository;
            _languageBranchRepository = languageBranchRepository;
        }

        public SiteDefinitionModel Convert(SiteDefinition site, ConverterContext context)
        {
            if (site is null) return null;

            var options = context.Options;

            return new SiteDefinitionModel
            {
                Id = site.Id,
                Name = site.Name,
                Hosts = options.IncludeSiteHosts ? CreateHostModel(site.Hosts).ToList() : null,
                EditLocation = options.IncludeSiteHosts ? null : GetEditLocation(site),
                ContentRoots = GetContentRoots(site, options.IncludeInternalContentRoots),
                Languages = AddLanguageModels(site, options.IncludeMasterLanguage, options.IncludeSiteHosts)
            };
        }

        private IEnumerable<HostDefinitionModel> CreateHostModel(IEnumerable<HostDefinition> hostDefinitions)
        {
            foreach (var hostDefinition in hostDefinitions)
            {
                yield return new HostDefinitionModel
                {
                    Language = hostDefinition.Language is null ? null : new LanguageModel { Name = hostDefinition.Language.Name, DisplayName = hostDefinition.Language.DisplayName },
                    Name = hostDefinition.Name,
                    Type = hostDefinition.Type.ToString()
                };
            }
        }

        private Dictionary<string, ContentModelReference> GetContentRoots(SiteDefinition siteDefintion, bool includeInternalRoots)
        {
            var roots = new Dictionary<string, ContentModelReference>
            {
                [GlobalAssetsRoot] = _contentModelService.GetContentModelReference(siteDefintion.GlobalAssetsRoot),
                [StartPage] = _contentModelService.GetContentModelReference(siteDefintion.StartPage)
            };

            if (includeInternalRoots)
            {
                roots[ContentAssetsRoot] = _contentModelService.GetContentModelReference(siteDefintion.ContentAssetsRoot);
                roots[RootPage] = _contentModelService.GetContentModelReference(siteDefintion.RootPage);
                roots[WasteBasket] = _contentModelService.GetContentModelReference(siteDefintion.WasteBasket);
            }

            if (siteDefintion.SiteAssetsRoot?.ID != siteDefintion.GlobalAssetsRoot?.ID)
            {
                roots[SiteAssetsRoot] = _contentModelService.GetContentModelReference(siteDefintion.SiteAssetsRoot);
            }

            foreach (var customRootReference in _contentRootRepository.List().Where(x => !DefaultCmsContentRootNames.Contains(x.Key)))
            {
                roots[customRootReference.Key] = _contentModelService.GetContentModelReference(customRootReference.Value);
            }

            return roots;
        }

        private IEnumerable<SiteDefinitionLanguageModel> AddLanguageModels(SiteDefinition site, bool includeMasterLanguage, bool includeSiteHosts)
        {
            var page = _contentLoader.Get<PageData>(site.StartPage);

            var languages = new List<SiteDefinitionLanguageModel>();
            foreach (var language in page.ExistingLanguages)
            {
                var urlSegment = _languageBranchRepository.Load(language)?.URLSegment;

                var languageModel = new SiteDefinitionLanguageModel()
                {
                    DisplayName = language.DisplayName,
                    Name = language.Name,
                    IsMasterLanguage = includeMasterLanguage && page.MasterLanguage.Equals(language),
                    UrlSegment = urlSegment,
                    Url = includeSiteHosts ? null : GetLanguageLocation(site, language, urlSegment),
                };

                languages.Add(languageModel);
            }

            return languages;
        }

        private string GetLanguageLocation(SiteDefinition site, CultureInfo language, string urlSegment)
        {
            var host = site.GetPrimaryHost(language);
            if (host is object)
            {
                return host.Url.ToString();
            }

            host = site.GetPrimaryHost(null);
            if (host is object)
            {
                return new Uri(host.Url, urlSegment + "/").ToString();
            }

            return null;
        }

        private string GetEditLocation(SiteDefinition site)
            => site.Hosts.FirstOrDefault(x => x.Type == HostDefinitionType.Edit)?.Url.ToString();
    }
}
