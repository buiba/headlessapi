using System.Linq;
using EPiServer.ContentApi.Error.Internal;
using Newtonsoft.Json.Linq;

namespace EPiServer.ContentManagementApi.IntegrationTests
{
    public static class ErrorExtensions
    {
        internal static string GetFirstValidationErrorMessage(this Error error)
        {
            if (error.Details.FirstOrDefault()?.InnerError is JArray array)
            {
                return string.Join(", ", array.Select(x => x.ToString()));
            }

            return string.Empty;
        }
    }
}
