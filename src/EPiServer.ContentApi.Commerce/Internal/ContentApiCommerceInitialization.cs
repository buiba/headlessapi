using EPiServer.ContentApi.Cms;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Security;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Initialize default settings for Content Delivery Commerce
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(ContentApiCmsInitialization))]
    internal class ContentApiCommerceInitialization : IConfigurableModule
    {
        /// <inheritdoc />
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IContentApiRequiredRoleFilter, CatalogRequiredRoleFilter>();
            context.Services.AddSingleton<ContentLoaderService, CatalogContentLoadService>();
        }

        /// <inheritdoc />
        public void Initialize(InitializationEngine context)
        {
        }

        /// <inheritdoc />
        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
