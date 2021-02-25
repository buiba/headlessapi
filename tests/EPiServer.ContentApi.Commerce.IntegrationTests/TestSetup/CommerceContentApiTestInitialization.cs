using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServerTestInitialization))]
    internal class CommerceContentApiTestInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.ConfigureForExternalTemplates();
            context.Services.Configure<ContentApiConfiguration>(c =>
            {
                c.EnablePreviewFeatures = true;

                c.Default()
                    .SetMinimumRoles(string.Empty)
                    .SetRequiredRole(string.Empty);
            });
        }

        public void Initialize(InitializationEngine context) { }

        public void Uninitialize(InitializationEngine context) { }
    }
}
