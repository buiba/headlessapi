using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents a purchase order.
    /// </summary>
    public class OrderApiModel
    {
        /// <summary>
        /// The order number.
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// The customer the order belongs to.
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// The market in which the order was placed.
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// The currency of the market in which the order was placed.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The shipments of the order.
        /// </summary>
        public IEnumerable<ShipmentModel> Shipments { get; set; }

        /// <summary>
        /// The totals of the order.
        /// </summary>
        public TotalsModel Totals { get; set; }
    }
}
