using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    ///     Value provider factory which attaches <see cref="AcceptLanguageHeaderValueProvider"/> to a IEnumerable of string parameter, 
    ///     allowing a list of languages to be retrieved from the Accept-Language header
    /// </summary>
    public class AcceptLanguageHeaderValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            var headers = actionContext.ControllerContext.Request.Headers;
            return new AcceptLanguageHeaderValueProvider(headers);
        }
    }
}
