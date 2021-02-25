using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Signature for component that resolves which <see cref="IContentConverter"/> instance that should be used
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IContentConverterResolver
    {
        /// <summary>
        /// Resolves which <see cref="IContentConverter"/> instance that should be used for <paramref name="content"/>
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> instance to resolve <see cref="IContentConverter"/> for</param>
        /// <returns>A matching <see cref="IContentConverter"/></returns>
        IContentConverter Resolve(IContent content);
    }
}
