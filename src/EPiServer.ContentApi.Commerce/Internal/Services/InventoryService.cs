using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Commerce.Internal.Models.Inventory;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.InventoryService;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    /// <summary>
    /// Inventory service to get inventory information.
    /// </summary>
    [ServiceConfiguration(typeof(InventoryService))]
    public class InventoryService
    {
        private readonly IInventoryService _inventoryService;

        public InventoryService(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get inventory information in given catalog entry code.
        /// </summary>
        public virtual IEnumerable<InventoryApiModel> GetInventories(string catalogEntryCode)
        {
            var inventories = _inventoryService.QueryByEntry(new[] { catalogEntryCode });

            return inventories == null
                ? Enumerable.Empty<InventoryApiModel>()
                : inventories.Select(x => new InventoryApiModel
                {
                    EntryCode = x.CatalogEntryCode,
                    WarehouseCode = x.WarehouseCode,
                    PurchaseAvailableQuantity = x.PurchaseAvailableQuantity,
                    PurchaseRequestedQuantity = x.PurchaseRequestedQuantity,
                    PurchaseAvailable = x.PurchaseAvailableUtc,
                });
        }
    }
}
