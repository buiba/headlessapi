using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Signature for component that provides <see cref="IPropertyConverter"/> instances
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IPropertyConverterProvider
    {
        /// <summary>
        /// The provider which has higher order will be called first to see if it handles specified <see cref="PropertyData"/> type
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// Determines if provider supports specified <paramref name="propertyData"/> type and if so returns a matching <see cref="IPropertyConverter"/> instance. If <paramref name="propertyData"/> is not supported return null
        /// </summary>
        /// <param name="propertyData">instance of <see cref="PropertyData"/> to resolve <see cref="IPropertyConverter"/> for</param>
        /// <returns>A matching <see cref="IPropertyConverter"/> or null if <paramref name="propertyData"/> is not supported by the provider</returns>
        IPropertyConverter Resolve(PropertyData propertyData);
    }
}
