using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace EPiServer.ContentManagementApi.IntegrationTests.TestSetup
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent content)
        {
            var method = new HttpMethod("PATCH");

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            var response = new HttpResponseMessage();

            try
            {
                response = await client.SendAsync(request);

            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("ERROR: " + e);
            }

            return response;
        }

        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUrl, HttpContent content)
        {
            return await PatchAsync(client, new Uri(requestUrl, UriKind.RelativeOrAbsolute), content);
        }
    }
}
