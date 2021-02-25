using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Error.Internal
{
    /// <summary>
    ///     Class for returning error information about requests.
    /// </summary>
    internal class Error
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Error"/>
        /// </summary>
        public Error()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Error"/>
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public Error(string code, string message = null)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Error Code.
        /// </summary>
        public string Code { get; set; }


        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }


        /// <summary>
        /// Error target
        /// </summary>
        public string Target { get; set; }


        /// <summary>
        /// Details about error.
        /// </summary>
        public IEnumerable<ErrorDetails> Details { get; set; } = Enumerable.Empty<ErrorDetails>();
    }

    internal class ErrorDetails : Error
    {
        public dynamic InnerError { get; set; }
    }
}
