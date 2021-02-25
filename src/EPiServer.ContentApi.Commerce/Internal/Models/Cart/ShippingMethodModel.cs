using System;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents a shipping method.
    /// </summary>
    public class ShippingMethodModel
    {
        /// <summary>
        /// The shipping id.
        /// </summary>
        public Guid ShipmentId { get; set; }

        /// <summary>
        /// The id.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The display name.
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// The price.
        /// </summary>
        public decimal Price { get; set; }
    }
}