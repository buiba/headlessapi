using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// This filter reponsible for extract request language from header and set to <see cref="IContentLanguageAccessor.Language"/>
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>    
    public class ContentLanguageFilterAttribute : FilterAttribute, IActionFilter
    {           
        private readonly Injected<IUpdateCurrentLanguage> _updateCurrentLanguage;

        public override bool AllowMultiple => false;

        public virtual async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var headerLanguage = actionContext.Request.Headers.AcceptLanguage.FirstOrDefault();
            if (headerLanguage != null)
            {
                _updateCurrentLanguage.Service.UpdateLanguage(headerLanguage.Value);
            }

            return await continuation();
        }        
    }
}
