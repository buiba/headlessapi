using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.DefinitionsApi.IntegrationTests.TestSetup
{
    internal static class ServiceFixtureExtensions
    {
        public static IDisposable WithContentTypeIds(this ServiceFixture _, params Guid[] contentTypeIds)
        {
            var scope = new ProvisioningScope(contentTypeIds);
            return scope;
        }

        public static async Task WithSite(this ServiceFixture _, SiteDefinition site, Func<Task> run)
        {
            var repo = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
            repo.Save(site);
            try
            {
                await run();
            }
            // Cleanup
            finally
            {
                repo.Delete(site.Id);
            }
        }

        public static async Task WithTab(this ServiceFixture _, TabDefinition tab, Func<Task> run)
        {
            var repo = ServiceLocator.Current.GetInstance<ITabDefinitionRepository>();
            repo.Save(tab);
            try
            {
                await run();
            }
            // Cleanup
            finally
            {
                repo.Delete(tab);
            }
        }

        public static async Task WithTabs(this ServiceFixture _, IEnumerable<TabDefinition> tabs, Func<Task> run)
        {
            var repo = ServiceLocator.Current.GetInstance<ITabDefinitionRepository>();
            foreach (var tab in tabs)
            {
                repo.Save(tab);
            }

            try
            {
                await run();
            }
            // Cleanup
            finally
            {
                foreach (var tab in tabs)
                {
                    repo.Delete(tab);
                }
            }
        }


        private class ProvisioningScope : IDisposable
        {
            private readonly IContentTypeRepository _contentTypeRepository;
            private readonly Guid[] _contentTypeIds;

            public ProvisioningScope(params Guid[] contentTypeIds)
            {
                _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
                _contentTypeIds = contentTypeIds;
            }

            public void Dispose()
            {
                foreach (var contentTypeId in _contentTypeIds)
                {
                    var contentType = _contentTypeRepository.Load(contentTypeId);
                    if (contentType != null)
                    { 
                        _contentTypeRepository.Delete(contentType.ID);
                    }
                }
                //Ensure potential cached block properties are cleared when block type is deleted
                ServiceLocator.Current.GetInstance<PropertyDataTypeResolver>().ClearCaches();
            }
        }
    }
}
