using System;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Warehouse
{
    public class WarehouseApiModel
    {
        /// <summary>Gets or sets the warehouse name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the timestamp at which the warehouse record was most recently updated.
        /// </summary>
        /// <value>The most recent update timestamp.</value>
        public DateTime Modified { get; set; }

        /// <summary>Gets or sets the code value.</summary>
        /// <value>The code.</value>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the contact information for the warehouse.
        /// </summary>
        /// <value>The contact information for the warehouse.</value>
        public WarehouseContactInformationModel ContactInformation { get; set; }

        /// <summary>
        /// Indicates whether the warehouse is a fulfillment center. This means orders can be placed from this warehouse for outgoing shipments.
        /// </summary>
        /// <value>A boolean indicating whether the warehouse is a fulfillment center.</value>
        public bool IsFulfillmentCenter { get; set; }

        /// <summary>
        /// Indicates whether the warehouse is a pick-up location. This means orders can be placed from this warehouse for in-store pickups.
        /// </summary>
        /// <value>A boolean indicating whether the warehouse is a pickup location.</value>
        public bool IsPickupLocation { get; set; }

        /// <summary>
        /// Indicates whether the warehouse is a delivery location. This means this warehouse can be used as a delivery location (i.e. for future in-store pickups).
        /// </summary>
        /// <value>A boolean indicating whether the warehouse is a delivery location.</value>
        public bool IsDeliveryLocation { get; set; }
    }
}
