using System;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    public class ShipmentModel
    {
        /// <summary>
        /// The shipping id.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The shipping address.
        /// </summary>
        public AddressModel ShippingAddress { get; set; }

        /// <summary>
        /// The shipping method id.
        /// </summary>
        public Guid ShippingMethodId { get; set; }

        /// <summary>
        /// the line items that belong to the cart.
        /// </summary>
        public IEnumerable<LineItemModel> LineItems { get; set; }
    }
}