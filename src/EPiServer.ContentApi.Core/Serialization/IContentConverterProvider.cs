using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Signature for component that provides <see cref="IContentConverter"/> instances
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IContentConverterProvider
    {
        /// <summary>
        /// The mapper with higher order is chosen to handle a specific content in different contexts. The Default implementation has Order 100.
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// Determines if provider supports specified <paramref name="content"/> type and if so returns a matching <see cref="IContentConverter"/> instance. If <paramref name="content"/> is not supported return null
        /// </summary>
        /// <param name="content">instance of <see cref="IContent"/> to resolve <see cref="IContentConverter"/> for</param>
        /// <returns>A matching <see cref="IContentConverter"/> or null if <paramref name="content"/> is not supported by the provider</returns>
        IContentConverter Resolve(IContent content);
    }
}
