using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Convenience extensions to working with IContent within the context of ContentApi Search
    /// </summary>
    internal static class IContentExtensions
    {
        /// <summary>
        ///     Extension method for transforming IContent into ContentApiModel for indexing into Find
        /// </summary>
        /// <param name="content">IContent to convert</param>
        /// <returns></returns>
        internal static ContentApiModel ContentApiModel(this IContent content)
        {
            var contentConvertingService = ServiceLocator.Current.GetInstance<ContentConvertingService>();

            return contentConvertingService.ConvertToContentApiModel(content, new FindIndexingJobConverterContext(ServiceLocator.Current.GetInstance<ContentApiConfiguration>().Default(), string.Empty, "*", true, (content as ILocale)?.Language));
        }
    }    
}
