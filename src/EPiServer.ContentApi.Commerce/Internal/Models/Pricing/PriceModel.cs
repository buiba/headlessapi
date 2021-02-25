using System;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Pricing
{
    /// <summary>
    /// Represents a price.
    /// </summary>
    public class PriceModel
    {
        /// <summary>
        /// The price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The price type.
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// The price code.
        /// </summary>
        public string PriceCode { get; set; }

        /// <summary>
        /// The valid from date and time.
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// The valid until date and time.
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        /// <summary>
        /// The minimum quantity.
        /// </summary>
        public decimal MinQuantity { get; set; }
    }
}
