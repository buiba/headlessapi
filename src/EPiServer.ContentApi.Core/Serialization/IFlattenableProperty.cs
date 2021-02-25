namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Identifies a property that supports flattening the model for serialization.
    /// </summary>
    public interface IFlattenableProperty
    {
        /// <summary>
        /// Flatten the <see cref="IPropertyModel"/> to prepare it for serialization.
        /// </summary>
        /// <returns>The value of the property</returns>
        object Flatten();
    }
}
