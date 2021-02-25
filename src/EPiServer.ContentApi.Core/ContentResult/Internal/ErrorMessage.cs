using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.ContentResult.Internal
{
    public static class ErrorMessage
    {
        public static string InternalServerError = "The server encountered an internal error or misconfiguration";

        public static string InvalidHeaderValue = "Header value is not valid";       

        public static string Forbidden = "Access denied";

        public static string NotFound = "Content was not found";

        public static string InvalidFilterClause = "Filter clause is not valid";

        public static string InvalidOrderByClause = "Orderby clause is not valid";
    }
}
