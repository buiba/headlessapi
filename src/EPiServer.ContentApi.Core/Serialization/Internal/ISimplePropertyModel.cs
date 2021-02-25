namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    /// <summary>
    /// Identify a <see cref="IPropertyModel"/> that contains a Value property
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface ISimplePropertyModel
    {
        object Value { get; }
    }
}
