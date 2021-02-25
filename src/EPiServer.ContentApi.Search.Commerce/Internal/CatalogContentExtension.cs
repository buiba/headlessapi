using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Security;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Search.Commerce.Internal
{
    /// <summary>
    /// Extensions to working with CatalogContentBase role within the context of Content Api Search for Commerce
    /// </summary>
    public static class CatalogContentExtension
    {
        /// <summary>
        ///  Extension method for extract CatalogContentBase roles for indexing into Find
        /// </summary>
        /// <param name="catalogContent"></param>
        /// <returns></returns>
        public static IEnumerable<string> CatalogRolesWithReadAccess(this CatalogContentBase catalogContent)
        {
            var securityEntries = (catalogContent.GetSecurityDescriptor() as IContentSecurityDescriptor)?.Entries;
            if (securityEntries == null)
            {
                return Enumerable.Empty<string>();
            }

            return securityEntries.Where(e => (e.Access & AccessLevel.Read) == AccessLevel.Read).Select(x => x.Name).ToList();
        }
    }

}
