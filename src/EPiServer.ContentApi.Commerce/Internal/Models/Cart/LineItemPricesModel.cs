using System;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    public class LineItemPricesModel
    {
        /// <summary>
        /// The line item id.
        /// </summary>
        public Guid LineItemId { get; set; }

        /// <summary>
        /// The extended price.
        /// </summary>
        public decimal ExtendedPrice { get; set; }

        /// <summary>
        /// The discounted price.
        /// </summary>
        public decimal DiscountedPrice { get; set; }
    }
}