using System.Collections.Generic;
using EPiServer.Commerce.Marketing;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    /// <summary>
    /// Wrapper service used to call <see cref="IPromotionEngineExtensions"/> methods.
    /// </summary>
    [ServiceConfiguration(typeof(PromotionService))]
    public class PromotionService
    {
        private readonly IPromotionEngine _promotionEngine;

        public PromotionService(IPromotionEngine promotionEngine)
        {
            _promotionEngine = promotionEngine;
        }

        /// <summary>
        /// Get discounted prices of catalog entry.
        /// </summary>       
        public virtual IEnumerable<DiscountedEntry> GetDiscountPrices(ContentReference entryLink, IMarket market, Currency currency)
        {
            return _promotionEngine.GetDiscountPrices(entryLink, market, currency);
        }
    }
}
