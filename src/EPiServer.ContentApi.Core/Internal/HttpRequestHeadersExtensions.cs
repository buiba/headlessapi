using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Extension method for HttpRequestHeader to work with AcceptLanguageHeader
    /// </summary>
    public static class HttpRequestHeadersExtensions
    {
        /// <summary>
        /// Get list of language from AcceptLanguageHeader
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static IEnumerable<string> ParseAcceptLanguageHeader(this HttpRequestHeaders headers)
        {
            var acceptLanguageHeader = headers.AcceptLanguage;
            if (acceptLanguageHeader == null || !acceptLanguageHeader.Any())
			{
				return Enumerable.Empty<string>();
			}
                
            return acceptLanguageHeader.Where(x => x.Value != "*").Select(x => x.Value);
        }
    }
}
