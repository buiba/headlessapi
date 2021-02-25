using System.Text;

namespace EPiServer.ContentApi.Core.ContentResult
{
    /// <summary>
    /// Base serializer interface for serialize object to string
    /// </summary>
    public interface IContentApiSerializer
    {
        /// <summary>
        /// Media type used when build response. Ex: application/json, application/xml
        /// </summary>
        string MediaType { get; }

        /// <summary>
        /// Encoding used when build response
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// serialize object to string
        /// </summary>
        string Serialize(object value);
    }
}
