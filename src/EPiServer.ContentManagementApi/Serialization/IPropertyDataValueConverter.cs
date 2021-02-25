using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;

namespace EPiServer.ContentManagementApi.Serialization
{
    /// <summary>
    /// Signature for a component that converts a <see cref="IPropertyModel"/> instance to a <see cref="PropertyData"/> instance
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IPropertyDataValueConverter
    {
        /// <summary>
        /// Returns an instance of PropertyData based from the provided IPropertyModel
        /// </summary>
        /// <param name="propertyModel">Instance of IPropertyModel which the PropertyData result is generated from.</param>
        /// <param name="propertyData">Instance of PropertyData</param>
        /// <returns>Instance of PropertyData</returns>
        object Convert(IPropertyModel propertyModel, PropertyData propertyData);
    }

}
