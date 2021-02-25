using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    [ServiceConfiguration(typeof(IContentConverterProvider), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class DefaultContentConverterProvider : IContentConverterProvider
    {
        private readonly DefaultContentConverter _defaultContentConverter;

        public DefaultContentConverterProvider(DefaultContentConverter defaultContentConverter)
        {
            _defaultContentConverter = defaultContentConverter;
        }

        public int SortOrder => 100;

        public IContentConverter Resolve(IContent content) => _defaultContentConverter;
    }
}
