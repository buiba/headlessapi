using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Data.Entity;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.Owin.Testing;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class ServiceFixture : IDisposable
    {
        private readonly DatabaseFixture _databaseFixture;
        private readonly EPiServerFixture _episerverFixture;
        private readonly TestServer _server;

        public HttpClient Client => _server.HttpClient;

        public IContentRepository ContentRepository { get; }

        public IContentTypeRepository ContentTypeRepository { get; }

        public ITabDefinitionRepository TabDefinitionRepository { get; }

        public ISiteDefinitionRepository SiteDefinitionRepository { get; }

        public CategoryRepository CategoryRepository { get; }

        public UrlResolver UrlResolver { get; }

        public IPermanentLinkMapper PermanentLinkMapper { get; }

        public ContentLanguageSettingRepository ContentLanguageSettingRepo { get; }

        public DisplayOptions DisplayOptions { get; }

        public ServiceFixture()
        {
            try
            {
                _databaseFixture = new DatabaseFixture();
                EPiServerTestInitialization.CmsDatabase = _databaseFixture.CmsDatabase;
                _episerverFixture = new EPiServerFixture();
                ContentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
                ContentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
                TabDefinitionRepository = ServiceLocator.Current.GetInstance<ITabDefinitionRepository>();
                UrlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
                PermanentLinkMapper = ServiceLocator.Current.GetInstance<IPermanentLinkMapper>();
                ContentLanguageSettingRepo = ServiceLocator.Current.GetInstance<ContentLanguageSettingRepository>();
                SiteDefinitionRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
                DisplayOptions = ServiceLocator.Current.GetInstance<DisplayOptions>();
                CategoryRepository = ServiceLocator.Current.GetInstance<CategoryRepository>();
                _server = TestServer.Create<Startup>();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _server?.Dispose();
            _episerverFixture?.Dispose();
            _databaseFixture?.Dispose();
        }

        public T GetWithDefaultName<T>(ContentReference parentLink, bool create = false, string language = "en", Action<T> init = null) where T : IContentData
        {
            var content = ContentRepository.GetDefault<T>(parentLink, CultureInfo.GetCultureInfo(language));
            (content as IContent).ContentGuid = Guid.NewGuid();
            (content as IContent).Name = (content as IContent).ContentGuid.ToString("N");
            init?.Invoke(content);
            if (create)
            {
                content = (T)((IReadOnly)ContentRepository.Get<T>(ContentRepository.Save((content as IContent), SaveAction.Publish, AccessLevel.NoAccess))).CreateWritableClone();
            }
            return content;
        }

        public T GetDraftWithDefaultName<T>(ContentReference parentLink, bool create = false, string language = "en", Action<T> init = null) where T : IContentData
        {
            var content = ContentRepository.GetDefault<T>(parentLink, CultureInfo.GetCultureInfo(language));
            (content as IContent).ContentGuid = Guid.NewGuid();
            (content as IContent).Name = (content as IContent).ContentGuid.ToString("N");
            init?.Invoke(content);
            if (create)
            {
                content = (T)((IReadOnly)ContentRepository.Get<T>(ContentRepository.Save((content as IContent), SaveAction.CheckOut, AccessLevel.NoAccess))).CreateWritableClone();
            }
            return content;
        }

        public T CreateLanguageBranchWithDefaultName<T>(ContentReference contentLink, bool create = false, string language = "sv", Action<T> init = null) where T : IContentData
        {
            var content = ContentRepository.CreateLanguageBranch<T>(contentLink, CultureInfo.GetCultureInfo(language));
            (content as IContent).Name = (content as IContent).ContentGuid.ToString("N");
            init?.Invoke(content);
            if (create)
            {
                (content as IContent).ContentLink = ContentRepository.Save(content as IContent, SaveAction.Publish, AccessLevel.NoAccess);
            }
            return content;
        }

        public ContentReference CreateDraftWithFullAccessForEveryone(IContent content)
        {
            content.SaveSecurityInfo(ServiceLocator.Current.GetInstance<IContentSecurityRepository>(), new AccessControlList() { new AccessControlEntry("Everyone", AccessLevel.FullAccess, SecurityEntityType.Role) }, SecuritySaveType.Replace);
            return ContentRepository.Save(content, SaveAction.ForceNewVersion, AccessLevel.NoAccess);
        }

        public async Task WithContent(IContent item, Func<Task> run, bool create = true) => await WithContentItems(new[] { item }, run, create);

        public async Task WithDisplayOption(string id, string name, string tag, Func<Task> run)
        {
            // Arrange
            DisplayOptions.Add(id, name, tag);

            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                DisplayOptions.Remove(id);
            }
        }

        public async Task WithContentItems(IEnumerable<IContent> items, Func<Task> run, bool create = true)
        {
            items = items.ToList();

            // Arrange
            if (create)
            {
                CreateContentItems(items);
            }
            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                ClearContentItems(items.Select(i => i.ContentLink));
            }
        }

        public async Task WithContentType(ContentType contentType, Func<Task> run, bool create = true)
        {
            // Arrange
            if (create)
            {
               ContentTypeRepository.Save(contentType);
            }
            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                ContentTypeRepository.Delete(contentType);
            }
        }

        public async Task WithCategories(IEnumerable<Category> categories, Func<Task> run, bool create = true)
        {
            categories = categories.ToList();

            // Arrange
            if (create)
            {
                foreach (var category in categories)
                {
                    CategoryRepository.Save(category);
                }
            }
            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                foreach (var category in categories)
                {
                    try
                    {
                        CategoryRepository.Delete(category);
                    }
                    catch { }
                }
            }
        }

        public void CreateContentItems(IEnumerable<IContent> items, SaveAction saveAction = SaveAction.Publish)
        {
            foreach (var content in items)
            {
                var readOnly = content as IReadOnly;
                if (readOnly == null || !readOnly.IsReadOnly)
                {
                    ContentRepository.Save(content, saveAction, AccessLevel.NoAccess);
                }
            }

        }

        public void ClearContentItems(IEnumerable<ContentReference> contentLinks)
        {
            foreach (var contentLink in contentLinks)
            {
                try
                {
                    ContentRepository.Delete(contentLink, true, AccessLevel.NoAccess);
                }
                catch { }
            }
        }

        public async Task WithSite(SiteDefinition site, Func<Task> run)
        {
            // Arrange
            SiteDefinitionRepository.Save(site);
            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                SiteDefinitionRepository.Delete(site.Id);
            }
        }

        public async Task WithHost(HostDefinition host, Func<Task> run)
        {
            var currentSite = SiteDefinition.Current.CreateWritableClone();
            // Arrange
            currentSite.Hosts.Add(host);
            SiteDefinitionRepository.Save(currentSite);
            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                currentSite.Hosts.Remove(host);
                SiteDefinitionRepository.Save(currentSite);
            }
        }

        public async Task WithHosts(IEnumerable<HostDefinition> hosts, Func<Task> run, bool clearOldHosts = false)
        {
            var currentSite = SiteDefinition.Current.CreateWritableClone();
            var oldHosts = new List<HostDefinition>();
            if (clearOldHosts)
            {
                currentSite.Hosts.ToList().ForEach(x => oldHosts.Add(x));
                currentSite.Hosts.Clear();
            }

            // Arrange
            foreach (var host in hosts)
            {
                currentSite.Hosts.Add(host);
            }
            SiteDefinitionRepository.Save(currentSite);
            try
            {
                // Act & Assert
                await run();
            }
            // Cleanup
            finally
            {
                foreach (var host in hosts)
                {
                    currentSite.Hosts.Remove(host);
                }

                if (clearOldHosts)
                {
                    foreach (var host in oldHosts)
                    {
                        currentSite.Hosts.Add(host);
                    }
                }

                SiteDefinitionRepository.Save(currentSite);
            }
        }

        public void SaveContentLanguageSetting(ContentLanguageSetting setting)
        {
            ContentLanguageSettingRepo.Save(setting);
        }
    }
}
