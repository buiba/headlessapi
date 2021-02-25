using System;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Exception thrown when a OData filter cannot be parsed into a filter by a given <see cref="IContentApiSearchProvider"/>
    /// </summary>
    public class FilterParseException : Exception
    {
        public FilterParseException(string message) : base(message) { }

        public FilterParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
