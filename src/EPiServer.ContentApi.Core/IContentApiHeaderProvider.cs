using System.Collections.Generic;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Interface for declaration of headers used in the content api
    /// </summary>
    public interface IContentApiHeaderProvider
    {
        /// <summary>
        /// List of headernames
        /// </summary>
        IEnumerable<string> HeaderNames { get; }
    }
}