using System.Collections.Generic;
using EPiServer.Find.Api;
using EPiServer.Find.Api.Querying;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Interface for parsing OData filter and orderby clauses down for use with Episerver Find
    /// </summary>
    public interface IFindODataParser
    {
        /// <summary>
        /// Parse filter string to create a Filter
        /// </summary>
        Filter ParseFilter(string filter);

        /// <summary>
        /// parse orderby string to create Find's IEnumerable
        /// </summary>
        IEnumerable<Sorting> ParseOrderBy(string orderby);
    }
}
