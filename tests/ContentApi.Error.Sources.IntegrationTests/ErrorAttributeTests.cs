using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.ContentApi.Error.Infrastructure;
using EPiServer.ContentApi.Error.Internal;
using Xunit;

namespace EPiServer.ContentApi.Error
{
    public class ErrorAttributeTests : IClassFixture<ServiceFixture>
    {
        private readonly ServiceFixture _fixture;

        public ErrorAttributeTests(ServiceFixture serviceFixture)
        {
            _fixture = serviceFixture;
        }

        [Fact]
        public async Task Request_WhenServiceThrowsBadRequestResponseException_ShouldReturnErrorWithStatus()
        {
            var response = await _fixture.HttpClient.GetAsync(ErrorController.ThrowBadRequestPath);
            AssertResponse.BadRequest(response);
            await AssertResponse.ErrorResponse(ErrorResponse.ForStatusCode(HttpStatusCode.BadRequest, title: "Crap input"), response);
        }

        [Fact]
        public async Task Request_WhenServiceUsesConflict_ShouldReturnErrorWithStatus()
        {
            var response = await _fixture.HttpClient.GetAsync(ErrorController.ConflictPath);
            AssertResponse.Conflict(response);
            await AssertResponse.ErrorResponse(ErrorResponse.ForStatusCode(HttpStatusCode.Conflict, title: "Conflict"), response);
        }

        [Fact]
        public async Task Request_WhenServiceAddsModelError_ShouldContainModelError()
        {
            var response = await _fixture.HttpClient.GetAsync(ErrorController.ValidationPath);
            AssertResponse.BadRequest(response);
            var errorResponse = await AssertResponse.ErrorResponse(ErrorResponse.ForStatusCode(HttpStatusCode.BadRequest, title: "The request is invalid."), response);
            Assert.NotNull(errorResponse.Error.Details.Single(e => e.Target.Equals(ErrorController.InvalidParameter, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task Request_WhenFilterThrowsResponseException_ShouldReturnError()
        {
            var response = await _fixture.HttpClient.GetAsync(ErrorController.FilterErrorPath);
            AssertResponse.StatusCode(HttpStatusCode.NotAcceptable, response);
            await AssertResponse.ErrorResponse(ErrorResponse.ForStatusCode(HttpStatusCode.NotAcceptable, title: "This filter didn't like this"), response);
        }

        [Fact]
        public async Task Request_WhenFilterThrowsUnhandledException_ShouldReturnError()
        {
            var response = await _fixture.HttpClient.GetAsync(ErrorController.UnhandledPath);
            AssertResponse.StatusCode(HttpStatusCode.InternalServerError, response);
            await AssertResponse.ErrorResponse(ErrorResponse.ForStatusCode(HttpStatusCode.InternalServerError, title: "kaboom"), response);
        }
    }
}
