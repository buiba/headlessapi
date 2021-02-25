using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Signature for component that resolves <see cref="IPropertyConverter"/> instances
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IPropertyConverterResolver
    {
        /// <summary>
        /// Resolves a matching <see cref="IPropertyConverter"/> instance for <paramref name="propertyData"/>. If no matching converter is registered null is returned
        /// </summary>
        /// <param name="propertyData">instance of <see cref="PropertyData"/> to resolve <see cref="IPropertyConverter"/> for</param>
        /// <returns>A matching <see cref="IPropertyConverter"/> or null if <paramref name="propertyData"/> is not matched</returns>
        IPropertyConverter Resolve(PropertyData propertyData);
    }
}
