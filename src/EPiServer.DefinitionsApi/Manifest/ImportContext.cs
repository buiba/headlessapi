using System.Collections.Generic;

namespace EPiServer.DefinitionsApi.Manifest
{
    /// <summary>
    /// A context object that contains information and settings for the import.
    /// </summary>
    public class ImportContext
    {
        private readonly List<ImportLogMessage> _log = new List<ImportLogMessage>();

        /// <summary>
        /// Gets or sets whether the import should try to continue when errors occur.
        /// </summary>
        public bool ContinueOnError { get; set; }

        /// <summary>
        /// Gets the log messages.
        /// </summary>
        public IEnumerable<ImportLogMessage> Log => _log;

        /// <summary>
        /// Adds a new log message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity of the message.</param>
        public void AddLogMessage(string message, ImportLogMessageSeverity severity = ImportLogMessageSeverity.Information)
        {
            _log.Add(new ImportLogMessage(message, severity));
        }
    }
}
