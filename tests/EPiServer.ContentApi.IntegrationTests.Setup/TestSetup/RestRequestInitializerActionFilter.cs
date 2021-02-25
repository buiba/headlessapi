using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class RestRequestInitializerActionFilter : IActionFilter
    {
        public bool AllowMultiple => false;

        private Injected<IEnumerable<IRestRequestInitializer>> _restRequestInitializers;

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            foreach (var notifyRestRequest in _restRequestInitializers.Service)
            {
                notifyRestRequest.InitiateRequest();
            }
            return await continuation();
        }
    }
}
