using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.ContentResult
{
    /// <summary>
    /// Base interface for JSON serializer configuration
    /// </summary>
    public interface IJsonSerializerConfiguration
    {
        /// <summary>
        /// Default JSON serializer settings
        /// </summary>
        /// <returns></returns>
        JsonSerializerSettings Settings { get; set; }
    }
}
