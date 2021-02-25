using EPiServer.Commerce.Marketing;
using EPiServer.ContentApi.Core.Security;
using EPiServer.Core;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Commerce.Internal.Services;
using Xunit;
using EPiServer.ContentApi.Core.Tracking;

namespace EPiServer.ContentApi.Commerce.Tests.Services
{
    public class PricingServiceTest
    {
        private Mock<IPriceService> _mockPriceService;
        private Mock<PromotionService> _mockPromotionService;
        private Mock<ReferenceConverter> _mockReferenceConverter;
        private Mock<IMarketService> _mockMarketService;
        private Mock<ContentApiAuthorizationService> _mockAuthorizationService;
        private readonly PricingService _subject;

        private ContentReference validProductContentReference;
        private ContentReference invalidProductContentReference;

        private const string ProductSKU1 = "SKU1";
        private const string ProductSKU2 = "SKU2";
        private const string MarketId = "US";
        private const string Currency = "USD";

        public PricingServiceTest()
        {
            validProductContentReference = new ContentReference(1);
            invalidProductContentReference = new ContentReference(2);

            _mockPriceService = new Mock<IPriceService>();
            _mockPriceService.Setup(x => x.GetPrices(It.IsAny<MarketId>(), It.IsAny<DateTime>(), 
                It.IsAny<IEnumerable<CatalogKey>>(), It.IsAny<PriceFilter>())).Returns(new List<PriceValue>() { new PriceValue(), new PriceValue() });

            _mockPromotionService = new Mock<PromotionService>(null);
            _mockPromotionService.Setup(x => x.GetDiscountPrices(It.IsAny<ContentReference>(), 
                It.IsAny<IMarket>(), It.IsAny<Currency>())).Returns(Mock.Of<IEnumerable<DiscountedEntry>>);

            _mockReferenceConverter = new Mock<ReferenceConverter>(null, null);
            _mockReferenceConverter.Setup(x => x.GetContentLink(It.Is<string>(s => s.Equals(ProductSKU1)))).Returns(validProductContentReference);
            _mockReferenceConverter.Setup(x => x.GetContentLink(It.Is<string>(s => s.Equals(ProductSKU2)))).Returns(invalidProductContentReference);

            _mockMarketService = new Mock<IMarketService>();
            _mockMarketService.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(Mock.Of<IMarket>);

            _mockAuthorizationService = new Mock<ContentApiAuthorizationService>(null, null, null, null, null, null, null);            
            _mockAuthorizationService.Setup(x => x.CanUserAccessContent(It.Is<ContentReference>(c => c.Equals(validProductContentReference)))).Returns(true);
            _mockAuthorizationService.Setup(x => x.CanUserAccessContent(It.Is<ContentReference>(c => c.Equals(invalidProductContentReference)))).Returns(false);

            _subject = new PricingService(_mockPriceService.Object, _mockPromotionService.Object, _mockReferenceConverter.Object,
                    _mockMarketService.Object, _mockAuthorizationService.Object);
        }

        [Fact]
        public void GetPricings_ShouldNotReturnPricing_WhenContentIsInvalid()
        {
            var result = _subject.GetPricings(new string[] { ProductSKU1, ProductSKU2 }, MarketId, Currency);
            Assert.Single(result);
        }

        [Fact]
        public void GetPricings_ShouldReturnPricing_WhenContentIsValid()
        {
            _mockReferenceConverter.Setup(x => x.GetContentLink(It.Is<string>(s => s.Equals(ProductSKU2)))).Returns(validProductContentReference);

            var result = _subject.GetPricings(new string[] { ProductSKU1, ProductSKU2 }, MarketId, Currency);
            Assert.Equal(2, result.Count());
        }
    }
}
