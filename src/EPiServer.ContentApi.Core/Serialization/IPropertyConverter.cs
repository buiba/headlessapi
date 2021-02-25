using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Signature for a component that converts a <see cref="PropertyData"/> instance to a <see cref="IPropertyModel"/> instance
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IPropertyConverter
    {
        /// <summary>
        /// Returns an instance of IPropertyModel based from the provided PropertyData
        /// </summary>
        /// <param name="propertyData">Instance of PropertyData which the IPropertyModel result is generated from.</param>
        /// <param name="contentMappingContext">The context for current content mapping</param>
        /// <returns>Instance of IPropertyModel</returns>
        IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext);
    }
}
