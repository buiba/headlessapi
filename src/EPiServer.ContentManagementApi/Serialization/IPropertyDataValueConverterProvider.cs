using EPiServer.ContentApi.Core.Serialization;

namespace EPiServer.ContentManagementApi.Serialization
{
    /// <summary>
    /// Signature for component that provides <see cref="IPropertyDataValueConverter"/> instances
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IPropertyDataValueConverterProvider
    {
        /// <summary>
        /// Determines if provider supports specified <paramref name="propertyModel"/> type and if so returns a matching <see cref="IPropertyDataValueConverter"/> instance. If <paramref name="propertyModel"/> is not supported return null
        /// </summary>
        /// <param name="propertyModel">instance of <see cref="IPropertyModel"/> to resolve <see cref="IPropertyDataValueConverter"/> for</param>
        /// <returns>The first matching <see cref="IPropertyDataValueConverter"/> or null if <paramref name="propertyModel"/> is not supported by the provider</returns>
        IPropertyDataValueConverter Resolve(IPropertyModel propertyModel);
    }
}
