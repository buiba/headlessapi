using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.Error.Internal;

namespace Xunit
{
    public partial class AssertResponse
    {
        internal static async Task<ErrorResponse> ErrorResponse(ErrorResponse expected, HttpResponseMessage response)
        {
            var error = await response.Content.ReadAsAsync<ErrorResponse>();

            if (expected.StatusCode.HasValue)
            {
                Assert.Equal(expected.StatusCode, error.StatusCode);
            }
            if (!string.IsNullOrEmpty(expected.Error.Code))
            {
                Assert.Equal(expected.Error.Code, error.Error.Code);
            }
            if (!string.IsNullOrEmpty(expected.Error.Message))
            {
                Assert.Equal(expected.Error.Message, error.Error.Message);
            }

            return error;
        }
    }
}
