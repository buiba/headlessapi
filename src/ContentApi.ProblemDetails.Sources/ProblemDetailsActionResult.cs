using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Problems.Internal
{
    internal class ProblemDetailsActionResult : JsonResult<ProblemDetails>
    {
        public ProblemDetailsActionResult(HttpStatusCode statusCode, ProblemDetails problem, JsonSerializerSettings serializerSettings, Encoding encoding, HttpRequestMessage request)
            : base(problem, serializerSettings, encoding, request)
        {
            StatusCode = statusCode;
        }

        public ProblemDetailsActionResult(HttpStatusCode statusCode, ProblemDetails problem, JsonSerializerSettings serializerSettings, Encoding encoding, ApiController controller)
            : base(problem, serializerSettings, encoding, controller)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await base.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            response.StatusCode = StatusCode;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(ProblemDetails.MediaType) { CharSet = Encoding.WebName };

            return response;
        }
    }
}
