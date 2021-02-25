using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Interface for mapping <see cref="IContent"/> to <see cref="ContentApiModel"/>
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IContentConverter
    {
        /// <summary>
        /// Maps an instance of IContent to ContentApiModel
        /// </summary>
        /// <param name="content">The IContent object that the ContentModel is generated from</param>
        ///<param name="contentMappingContext">The context for current content mapping</param>
        /// <returns>Instance of ContentModel</returns>
        ContentApiModel Convert(IContent content, ConverterContext contentMappingContext);
    }
}
