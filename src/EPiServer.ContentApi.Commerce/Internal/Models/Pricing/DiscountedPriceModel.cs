namespace EPiServer.ContentApi.Commerce.Internal.Models.Pricing
{
    /// <summary>
    /// Represents a discounted price.
    /// </summary>
    public class DiscountedPriceModel
    {
        /// <summary>
        /// The description of the discount.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The discounted price.
        /// </summary>
        public decimal DiscountedPrice { get; set; }

        /// <summary>
        /// The default price that be used for calculating the discounted price.
        /// </summary>
        public decimal DefaultPrice { get; set; }
    }
}
