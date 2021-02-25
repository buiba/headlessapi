using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Error.Internal
{
    internal class ErrorActionResult : JsonResult<ErrorResponse>
    {
        public ErrorActionResult(HttpStatusCode statusCode, ErrorResponse error, JsonSerializerSettings serializerSettings, Encoding encoding, HttpRequestMessage request)
            : base(error, serializerSettings, encoding, request)
        {
            StatusCode = statusCode;
        }

        public ErrorActionResult(HttpStatusCode statusCode, ErrorResponse error, JsonSerializerSettings serializerSettings, Encoding encoding, ApiController controller)
            : base(error, serializerSettings, encoding, controller)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await base.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            response.StatusCode = StatusCode;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(ErrorResponse.MediaType) { CharSet = Encoding.WebName };

            return response;
        }
    }
}
