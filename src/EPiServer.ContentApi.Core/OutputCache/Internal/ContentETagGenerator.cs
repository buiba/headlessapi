using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    /// <summary>
    /// Generates an ETag for content
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentETagGenerator
    {
        private readonly string[] _headerNames;

        /// <summary>
        /// Creates a new instance of <see cref="ContentETagGenerator"/>
        /// </summary>
        public ContentETagGenerator() : this(Enumerable.Empty<IContentApiHeaderProvider>()) { }

        /// <summary>
        /// Creates a new instance of <see cref="ContentETagGenerator"/>
        /// </summary>
        /// <param name="contentApiHeaderProviders">Declaration of headers that are part of the etag calculation</param>
        public ContentETagGenerator(IEnumerable<IContentApiHeaderProvider> contentApiHeaderProviders)
        {
            _headerNames = contentApiHeaderProviders.SelectMany(x => x.HeaderNames).Distinct().OrderBy(x => x).ToArray();
        }

        /// <summary>
        /// Generates an ETag for content
        /// </summary>
        public virtual string Generate(HttpRequestMessage httpRequestMessage, ContentApiTrackingContext contentApiTrackingContext)
        {
            var hashCodeCombiner = HashCodeCombiner.Start();

            hashCodeCombiner.Add(httpRequestMessage.RequestUri.ToString());

            foreach (var headerName in _headerNames)
            {
                if (httpRequestMessage.Headers.TryGetValues(headerName, out var headerValues))
                {
                    hashCodeCombiner.Add(headerName);
                    hashCodeCombiner.Add(headerValues);
                }
            }

            foreach (var item in contentApiTrackingContext.ReferencedContent.OrderBy(x => x.Key.GetHashCode()))
            {
                hashCodeCombiner.Add(item.Key.ContentLink);
                hashCodeCombiner.Add(item.Key.Language);
                hashCodeCombiner.Add(item.Value?.SavedTime);
            }
            hashCodeCombiner.Add(contentApiTrackingContext.ReferencedContent.Count);

            return hashCodeCombiner.CombinedHash.ToString();
        }

        /// <summary>
        ///    Generates an ETag for a specific content
        /// </summary>
        public virtual string Generate(IContent content)
        {
            var contentLanguage = (content as ILocale)?.Language;
            DateTime? savedTime = null;
            if (content is IChangeTrackable changeTrackable)
            {
                savedTime = changeTrackable.Saved;
            }

            var hashCodeCombiner = HashCodeCombiner.Start();

            hashCodeCombiner.Add(content.ContentLink);
            hashCodeCombiner.Add(contentLanguage);
            hashCodeCombiner.Add(savedTime);

            return hashCodeCombiner.CombinedHash.ToString();
        }
    }
}