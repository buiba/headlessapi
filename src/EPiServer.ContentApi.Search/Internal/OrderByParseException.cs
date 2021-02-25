using System;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Exception thrown when a OData orderby clause cannot be parsed into a Find sorting clause by a given <see cref="IContentApiSearchProvider"/>
    /// </summary>
    public class OrderByParseException : Exception
    {
        public OrderByParseException(string message) : base(message) { }

        public OrderByParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
