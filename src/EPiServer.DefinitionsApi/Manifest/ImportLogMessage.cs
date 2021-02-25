using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EPiServer.DefinitionsApi.Manifest
{
    /// <summary>
    /// Defines an import log message.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ImportLogMessage
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the severity of the message.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ImportLogMessageSeverity Severity { get; }

        /// <summary>
        /// Constructs a new log message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity of the message.</param>
        public ImportLogMessage(string message, ImportLogMessageSeverity severity)
        {
            Message = message;
            Severity = severity;
        }
    }
}
