using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents a cart.
    /// </summary>
    public class OrderInputModel
    {
        /// <summary>
        /// The order name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The customer the order belongs to.
        /// </summary>
        [NotEmptyGuid]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// The market in which the order is valid.
        /// </summary>
        [Required]
        public string Market { get; set; }

        /// <summary>
        /// The currency of the market in which the order is valid.
        /// </summary>
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// The shipments.
        /// </summary>
        public IEnumerable<ShipmentModel> Shipments { get; set; }

        /// <summary>
        /// The coupon codes.
        /// </summary>
        public IEnumerable<string> CouponCodes { get; set; }

    }
}
