using System.Collections.Generic;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Pricing
{
    /// <summary>
    /// Represents pricing data for a sku
    /// </summary>
    public class PricingApiModel
    {
        /// <summary>
        /// The entry code.
        /// </summary>
        public string EntryCode { get; set; }

        /// <summary>
        /// The prices.
        /// </summary>
        public IEnumerable<PriceModel> Prices { get; set; }

        /// <summary>
        /// The discounted prices.
        /// </summary>
        public IEnumerable<DiscountedPriceModel> DiscountedPrices { get; set; }
    }
}
