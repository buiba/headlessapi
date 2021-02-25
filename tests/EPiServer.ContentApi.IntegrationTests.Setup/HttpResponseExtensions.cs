using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.IntegrationTests
{
    internal static class HttpResponseExtensions
    {
        internal static async Task<T> ReadAs<T>(this HttpContent content, JsonSerializerSettings jsonSerializerSettings = null)
        {
            using (var contentStream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(contentStream))
            using (var tr = new JsonTextReader(sr))
            {
                return JsonSerializer.CreateDefault(jsonSerializerSettings).Deserialize<T>(tr);
            }
        }
    }
}
