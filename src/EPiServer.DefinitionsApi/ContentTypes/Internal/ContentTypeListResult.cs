using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class ContentTypeListResult : JsonResult<IEnumerable<ExternalContentType>>
    {
        private readonly ListResult _listResult;

        public ContentTypeListResult(ListResult listResult, ApiController controller)
            : base(listResult, new JsonSerializerSettings(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), controller)
        {
            _listResult = listResult;
        }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await base.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            if (_listResult.ContinuationToken != ContinuationToken.None)
            {
                response.Headers.Add(ContinuationToken.HeaderName, _listResult.ContinuationToken.AsTokenString());
            }

            return response;
        }
    }
}
