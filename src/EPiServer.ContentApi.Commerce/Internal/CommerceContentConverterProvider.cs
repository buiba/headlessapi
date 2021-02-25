using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Commerce.Internal
{
    [ServiceConfiguration(typeof(IContentConverterProvider), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class CommerceContentConverterProvider : IContentConverterProvider
    {
        private readonly CommerceContentModelMapper _commerceContentModelMapper;

        public CommerceContentConverterProvider(CommerceContentModelMapper commerceContentModelMapper)
        {
            _commerceContentModelMapper = commerceContentModelMapper;
        }
        public int SortOrder => 300;

        public IContentConverter Resolve(IContent content)
        {
            return content is CatalogContentBase ? _commerceContentModelMapper : null;
        }
    }
}
