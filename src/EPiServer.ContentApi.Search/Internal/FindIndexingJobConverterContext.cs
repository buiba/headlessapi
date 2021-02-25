using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using System.Globalization;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    /// Context used when indexing content by Find indexing job in admin mode
    /// </summary>
    public class FindIndexingJobConverterContext : ConverterContext
    {
        public FindIndexingJobConverterContext(ContentApiOptions contentApiOptions, string select, string expand, bool excludePersonalizedContent, CultureInfo language) : base(contentApiOptions, select, expand, excludePersonalizedContent, language)
        {
        }
    }
}
