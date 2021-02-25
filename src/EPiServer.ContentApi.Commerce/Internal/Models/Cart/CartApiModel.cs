using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents a cart.
    /// </summary>
    public class CartApiModel
    {
        /// <summary>
        /// The cart id.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// The cart name.
        /// </summary>
        [Required] 
        public string Name { get; set; }

        /// <summary>
        /// The customer the cart belongs to.
        /// </summary>
        [NotEmptyGuid]
        public Guid CustomerId { get; set; } 

        /// <summary>
        /// The market in which the cart is valid.
        /// </summary>
        [Required]
        public string Market { get; set; } 

        /// <summary>
        /// The currency of the market in which the cart is valid.
        /// </summary>
        [Required]
        public string Currency { get; set; }  

        /// <summary>
        /// The last updated timestamp.
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; }

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