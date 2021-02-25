using System.Collections;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents cart totals.
    /// </summary>
    public class TotalsModel
    {
        /// <summary>
        /// The total.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// The subtotal.
        /// </summary>
        public decimal SubTotal { get; set; }

        /// <summary>
        /// The shipping total.
        /// </summary>
        public decimal ShippingTotal { get; set; }

        /// <summary>
        /// The handling total.
        /// </summary>
        public decimal HandlingTotal { get; set; }

        /// <summary>
        /// The tax total.
        /// </summary>
        public decimal TaxTotal { get; set; }

        /// <summary>
        /// The order form discount total.
        /// </summary>
        public decimal DiscountTotal { get; set; }

        /// <summary>
        /// The shipping totals.
        /// </summary>
        public IEnumerable<ShippingTotalsModel> ShippingTotals { get; set; }
    }
}