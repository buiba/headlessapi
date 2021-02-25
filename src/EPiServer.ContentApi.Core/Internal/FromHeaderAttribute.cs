using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Custom attribute for binding parameter value from request's headers
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]    
    public sealed class FromHeaderAttribute : ModelBinderAttribute
    {
        /// <inheritdoc />
        public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            return new[] { new FromHeaderValueProviderFactory() };
        }

        private class FromHeaderValueProviderFactory : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(HttpActionContext actionContext)
            {
                return new FromHeaderProvider(actionContext, CultureInfo.CurrentCulture);
            }
        }
    }

    /// <summary>
    /// A name/value pair provider that retrieve value from request's headers
    /// </summary>
    public sealed class FromHeaderProvider : NameValuePairsValueProvider
    {
        public FromHeaderProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(
                () => actionContext.ControllerContext.Request.Headers                
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.First())), culture)
        {
        }
    }
}
