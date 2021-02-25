using System;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    public class ShippingTotalsModel
    {
        /// <summary>
        /// The shipping id.
        /// </summary>
        public Guid ShipmentId { get; set; }

        /// <summary>
        /// The shipping cost.
        /// </summary>
        public decimal ShippingCost { get; set; }

        /// <summary>
        /// The shipping tax.
        /// </summary>
        public decimal ShippingTax { get; set; }

        /// <summary>
        /// The extended price total for the line items in the shipment.
        /// </summary>
        public decimal ItemsTotal { get; set; }

        /// <summary>
        /// The line item prices.
        /// </summary>
        public IEnumerable<LineItemPricesModel> LineItemPrices{ get; set; }
    }
}