using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Commerce;

namespace EPiServer.ContentApi.Search.Commerce.Internal
{
    /// <summary>
    /// Customize ContentApiSearchProvider to handle role access filter for CatalogContentBase 
    /// </summary>
    public class FindCommerceSearchProvider : FindContentApiSearchProvider
    {

        public FindCommerceSearchProvider(IClient searchClient,
            ContentConvertingService contentConvertingService,
            ContentApiSearchConfiguration searchConfig, ContentApiConfiguration contentApiConfig,
            IFindODataParser parser, RoleService roleService)
            : base(searchClient, contentConvertingService, searchConfig, contentApiConfig, parser, roleService)
        {
        }

        /// <inheritdoc />
        public override ITypeSearch<IContent> AttachContentApiRoleFilter(ITypeSearch<IContent> search, string role)
        {
            var mappedRoles = GetMappedRoles(role);

            var filter = BuildContentSecurableFilter(mappedRoles);

            // If content is Catalog content, we need build an extra filter for CatalogContentBase,
            // otherwise the default ContentSecurableFilter will filter out all Catalog content
            var catalogRoleFilter = _searchClient.BuildFilter<CatalogContentBase>();
            foreach (var item in mappedRoles)
            {
                catalogRoleFilter = catalogRoleFilter.Or(x => x.CatalogRolesWithReadAccess().Match(item));
            }

            filter = filter.Or(x => catalogRoleFilter);

            return search.Filter(filter);
        }
    }
}
