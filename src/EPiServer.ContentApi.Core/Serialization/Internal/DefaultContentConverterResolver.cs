using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    [ServiceConfiguration(typeof(IContentConverterResolver), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class DefaultContentConverterResolver : IContentConverterResolver
    {
        private readonly List<IContentConverterProvider> _contentConverterProviders;
#pragma warning disable CS0618 // Type or member is obsolete
        public DefaultContentConverterResolver(IEnumerable<IContentModelMapper> legacyMappers, IEnumerable<IContentConverterProvider> contentConverterProviders)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            var nonSystemLegacyMappers = legacyMappers.Where(lm => !lm.GetType().Assembly.GetName().Name.StartsWith("EPiServer.ContentApi"));
            _contentConverterProviders = (nonSystemLegacyMappers != null ? 
                ((IEnumerable<IContentConverterProvider>)nonSystemLegacyMappers.Select(m => new ContentModelWrapper(m))).Concat(contentConverterProviders) :
                contentConverterProviders)
                .OrderByDescending(p => p.SortOrder)
                .ToList();
        }

        public IContentConverter Resolve(IContent content)
        {
            foreach (var provider in _contentConverterProviders)
            {
                var converter = provider.Resolve(content);
                if (converter != null)
                {
                    return converter;
                }
            }

            throw new InvalidOperationException($"There is no {nameof(IContentConverter)} registered for content of type {content.GetType()}");
        }
    }
}
