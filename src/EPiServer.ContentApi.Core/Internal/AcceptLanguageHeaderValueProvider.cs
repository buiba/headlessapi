using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http.ValueProviders;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    ///     Value provider to parse Accept-Language header out to a IEnumerable of string for use in specific controllers.
    ///     Attach to a WebApi method via <see cref="AcceptLanguageHeaderValueProviderFactory"/>
    /// </summary>
    public class AcceptLanguageHeaderValueProvider : IValueProvider
    {
        protected readonly HttpRequestHeaders _headers;

        /// <summary>
        /// Initializes a new instacne of the <see cref="AcceptLanguageHeaderValueProvider"/>
        /// </summary>
        /// <param name="headers"></param>
        public AcceptLanguageHeaderValueProvider(HttpRequestHeaders headers)
        {
            _headers = headers;
        }
        
		/// <inheritdoc />
        public bool ContainsPrefix(string prefix)
        {
            return true;
        }

        /// <summary>
        /// Get value by key and culture from http header
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ValueProviderResult GetValue(string key)
        {
            var headerValue = _headers.AcceptLanguage;
            ValueProviderResult result = null;
            if (headerValue != null && headerValue.Any())
            {
                result = new ValueProviderResult(_headers.ParseAcceptLanguageHeader(), _headers.AcceptLanguage.ToString(), CultureInfo.InvariantCulture);
            }
            return result;
        }
    }
}
