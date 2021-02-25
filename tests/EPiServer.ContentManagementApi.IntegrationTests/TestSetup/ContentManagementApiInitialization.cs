using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Configuration;
using EPiServer.Framework;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentManagementApi.IntegrationTests.TestSetup
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServerTestInitialization))]
    internal class ContentManagementApiInitialization : IInitializableModule, IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
            // Accept anonymous calls during test.
            ServiceLocator.Current.GetInstance<ContentManagementApiOptions>()
                .ClearAllowedScopes()
                .SetRequiredRole(string.Empty);
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {            
            context.Services.Configure<ContentApiConfiguration>(c =>
            {
                c.Default()
                    .SetValidateTemplateForContentUrl(false);
            });

            context.Services.AddBlobProvider<FileBlobProvider, FileBlobProviderOptions>(
                "BlobProvider",
                true,
                opt => {
                    opt.Path = VirtualPathUtilityEx.AppDataPathKey + "\\blobs";
                }
            );
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
