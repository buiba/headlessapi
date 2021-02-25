using System;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Inventory
{
    /// <summary>
    /// Represents inventory information for a single catalog entry at a single warehouse.
    /// </summary>
    public class InventoryApiModel
    {
        /// <summary>
        /// The catalog entry code.
        /// </summary>
        public string EntryCode { get; set; }

        /// <summary>
        /// The warehouse's information.
        /// </summary>
        public string WarehouseCode { get; set; }

        /// <summary>
        /// The quantity of items available for purchase.
        /// </summary>
        public decimal PurchaseAvailableQuantity { get; set; }

        /// <summary>
        /// The quantity of items requested for purchase and not yet completed.
        /// </summary>
        public decimal PurchaseRequestedQuantity { get; set; }

        /// <summary>
        /// The purchase availability date for the item and warehouse, in UTC.
        /// </summary>
        public DateTime PurchaseAvailable { get; set; }
    }
}
