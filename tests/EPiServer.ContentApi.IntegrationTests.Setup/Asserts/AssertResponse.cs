using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit
{
    public partial class AssertResponse
    {
        public static void StatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            if (response.StatusCode == expected)
            {
                return;
            }

            var responseContent = string.Join(Environment.NewLine, response.Headers);
            try
            {
                responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // No-op
            }

            throw new StatusCodeMismatchException
            {
                ExpectedStatusCode = expected,
                ActualStatusCode = response.StatusCode,
                ResponseContent = responseContent,
            };
        }

        public static async Task ResponseAsync(string expected, HttpResponseMessage response, IEqualityComparer<string> equalityComparer = null)
        {
            var content = (await response.Content.ReadAsStringAsync()).Trim();
            if (equalityComparer is null)
            {
                Assert.Equal(expected, content);
            }
            else
            {
                Assert.Equal(expected, content, equalityComparer);
            }
        }

        public static void OK(HttpResponseMessage response) => StatusCode(HttpStatusCode.OK, response);

        public static void NoContent(HttpResponseMessage response) => StatusCode(HttpStatusCode.NoContent, response);

        public static void NotFound(HttpResponseMessage response) => StatusCode(HttpStatusCode.NotFound, response);

        public static void NotModified(HttpResponseMessage response) => StatusCode(HttpStatusCode.NotModified, response);

        public static void Conflict(HttpResponseMessage response) => StatusCode(HttpStatusCode.Conflict, response);

        public static void BadRequest(HttpResponseMessage response) => StatusCode(HttpStatusCode.BadRequest, response);

        public static void Unauthorized(HttpResponseMessage response) => StatusCode(HttpStatusCode.Unauthorized, response);

        public static void Forbidden(HttpResponseMessage response) => StatusCode(HttpStatusCode.Forbidden, response);

        public static void Created(HttpResponseMessage response) => StatusCode(HttpStatusCode.Created, response);

        public static Guid Created(string expectedLocationBasePath, HttpResponseMessage response)
        {
            if (expectedLocationBasePath == null)
            {
                throw new ArgumentNullException(nameof(expectedLocationBasePath));
            }

            Created(response);

            var idString = AssertBasePath(expectedLocationBasePath, response.Headers.Location.AbsolutePath);

            Assert.True(Guid.TryParse(idString, out var id), "Unable to parse location identifier to a Guid");

            return id;
        }

        public static void ValidationError(HttpResponseMessage response)
        {
            BadRequest(response);
            // TODO: Check response type etc. when consistent validation errors has been implemented
        }

        private static string AssertBasePath(string expectedBasePath, string actualPath)
        {
            expectedBasePath = expectedBasePath.Trim('/');
            actualPath = actualPath.Trim('/');
            Assert.NotEqual(expectedBasePath, actualPath);
            Assert.StartsWith(expectedBasePath, actualPath);
            return actualPath.Substring(expectedBasePath.Length + 1);
        }
    }
}
