using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    /// <summary>
    /// Customize required role filter for Commerce catalog content
    /// </summary>
    [ServiceConfiguration(typeof(IContentApiRequiredRoleFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class CatalogRequiredRoleFilter : ContentApiRequiredRoleFilter
    {
        public CatalogRequiredRoleFilter(RoleService roleService, ContentApiConfiguration apiConfiguration)
            : base(roleService, apiConfiguration)
        {
        }

        protected override IContentSecurityDescriptor GetContentSecurityDescriptor(IContent content)
        {
            if (content is CatalogContentBase catalogContent)
            {
                return catalogContent.GetSecurityDescriptor() as IContentSecurityDescriptor;
            }

            return base.GetContentSecurityDescriptor(content);
        }
    }
}
