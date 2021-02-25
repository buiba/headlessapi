using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    ///  Interface that must be implemented by an item that contains that the property ContentModelReference 
    ///  It 's needed for data serialization for clients
    /// </summary>
    public interface IContentItem
    {
        /// <summary>
        /// Mapped property for ContentReference
        /// </summary>
        ContentModelReference ContentLink { get; set; }
    }
}
