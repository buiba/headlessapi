using EPiServer.ContentApi.Core.Serialization;

namespace EPiServer.ContentManagementApi.Serialization
{
    /// <summary>
    /// Signature for component that resolves <see cref="IPropertyDataValueConverter"/> instances
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IPropertyDataValueConverterResolver
    {
        /// <summary>
        /// Resolves a matching <see cref="IPropertyDataValueConverter"/> instance for <paramref name="propertyModel"/>. If no matching converter is registered null is returned
        /// </summary>
        /// <param name="propertyModel">instance of <see cref="IPropertyModel"/> to resolve <see cref="IPropertyDataValueConverter"/> for</param>
        /// <returns>A first matching <see cref="IPropertyDataValueConverter"/> or null if <paramref name="propertyModel"/> is not matched</returns>
        IPropertyDataValueConverter Resolve(IPropertyModel propertyModel);
    }
}
