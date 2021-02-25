using EPiServer.ContentApi.Core.ContentResult.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Commerce.Internal.Controller;
using EPiServer.ContentApi.Commerce.Internal.Services;
using Xunit;
using Mediachase.Commerce.Catalog;

namespace EPiServer.ContentApi.Commerce.Tests.Controller
{
    public class PricingApiControllerTest
    {
        private readonly PricingApiController _controller;

        public PricingApiControllerTest()
        {
            var mockPricingService = new Mock<PricingService>();
            var referenConverterMock = new Mock<ReferenceConverter>(null, null);
            referenConverterMock.Setup(x => x.GetCodes(It.IsAny<IEnumerable<Guid>>())).Returns(Enumerable.Empty<string>());

            _controller = new PricingApiController(mockPricingService.Object, referenConverterMock.Object);
        }

        [Fact]
        public void GetPricings_WhenIdListIsEmpty_ShouldReturn400()
        {
            var result = (ContentApiResult<ErrorResponse>) _controller.GetPricings(new Guid[0], "US", "USD");

            Assert.True(result.StatusCode == HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GetPricings_WhenMarketIdIsInvalid_ShouldReturn400(string marketId)
        {
            var result = (ContentApiResult<ErrorResponse>) _controller.GetPricings(new[] {Guid.NewGuid()}, marketId, "USD");

            Assert.True(result.StatusCode == HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GetPricings_WhenCurrencyIsInvalid_ShouldReturn400(string currency)
        {
            var result = (ContentApiResult<ErrorResponse>) _controller.GetPricings(new[] {Guid.NewGuid()}, "US", currency);

            Assert.True(result.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}