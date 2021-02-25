using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Error.Internal
{
    internal class ErrorContent : StringContent
    {
        public ErrorContent(ErrorResponse error)
            : base(JsonConvert.SerializeObject(error), Encoding.UTF8, ErrorResponse.MediaType) { }
    }
}
