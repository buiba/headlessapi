using System;
using System.Collections.Generic;
using EPiServer.DataAbstraction;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    /// <summary>
    /// This interface is intended to be used internally by EPiServer. We do not support any backward compatibility on this.
    /// Interface to create an extension to extend write operations for <see cref="ExternalContentType"/>
    /// </summary>
    public interface IExternalContentTypeServiceExtension
    {
        /// <summary>
        /// Saves content types
        /// </summary>
        /// <param name="externalContentTypes"></param>
        /// <param name="internalContentTypes"></param>
        /// <param name="contentTypeSaveOptions"></param>
        void Save(IEnumerable<ExternalContentType> externalContentTypes, IEnumerable<ContentType> internalContentTypes, ContentTypeSaveOptions contentTypeSaveOptions);

        /// <summary>
        /// Deletes content type
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True, if delete successful else False</returns>
        bool TryDelete(Guid id);
    }
}
