using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Problems.Internal
{
    internal class ProblemDetailsContent : StringContent
    {
        public ProblemDetailsContent(ProblemDetails problem)
            : base(JsonConvert.SerializeObject(problem), Encoding.UTF8, ProblemDetails.MediaType) { }
    }
}
