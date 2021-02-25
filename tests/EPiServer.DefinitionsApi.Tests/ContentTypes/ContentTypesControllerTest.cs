using System.Net;
using System.Net.Http;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    public class ContentTypesControllerTest
    {
        [Fact]
        public void List_WhenContinuationTokenHasBadFormat_ShouldReturnErrorResult()
        {
            var controller = new ContentTypesController(Mock.Of<ExternalContentTypeRepository>(), Mock.Of<ContentTypeAnalyzer>())
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/path")
            };

            var result = controller.List(100, "Not really a token string");
            Assert.Equal(HttpStatusCode.BadRequest, Assert.IsType<ErrorActionResult>(result).StatusCode);
        }

        [Fact]
        public void List_WhenProvidingBothTopAndContinuationToken_ShouldReturnErrorResult()
        {
            var controller = new ContentTypesController(Mock.Of<ExternalContentTypeRepository>(), Mock.Of<ContentTypeAnalyzer>())
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/path")
            };

            var result = controller.List(100, new ContinuationToken(1, 1).AsTokenString());
            Assert.Equal(HttpStatusCode.BadRequest, Assert.IsType<ErrorActionResult>(result).StatusCode);
        }
    }
}
