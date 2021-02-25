using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Find.ClientConventions;
using EPiServer.Find.Framework;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Search.Commerce.Internal
{
    /// <summary>
    /// Initialize default settings for Content Api Search Commerce
    /// </summary>
    [InitializableModule]    
    [ModuleDependency(typeof(ContentApiSearchInitialization))]
    public class ContentApiCommerceInitialization : IConfigurableModule, IInitializableModule
    {
        /// <inheritdoc />
        public void ConfigureContainer(ServiceConfigurationContext context)
        {            
            context.Services.AddScoped<IContentApiSearchProvider, FindCommerceSearchProvider>();
            context.Services.AddSingleton<FindEventsAssociationService, FindCommerceEventsAssociationService>();            
        }

        /// <inheritdoc />
        public void Initialize(InitializationEngine context)
        {
            SearchClient.Instance.Conventions.ForInstancesOf<CatalogContentBase>()
                .IncludeField(x => x.CatalogRolesWithReadAccess());
        }

        /// <inheritdoc />
        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
