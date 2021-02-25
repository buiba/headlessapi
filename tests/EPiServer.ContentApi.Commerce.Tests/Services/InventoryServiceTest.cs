using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Moq;
using System.Collections.Generic;
using EPiServer.ContentApi.Commerce.Internal.Services;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Services
{
    public class InventoryServiceTest
    {
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
        private readonly InventoryService _subject;

        public InventoryServiceTest()
        {
            _inventoryServiceMock = new Mock<IInventoryService>();
            _warehouseRepositoryMock = new Mock<IWarehouseRepository>();        
            _subject = new InventoryService(_inventoryServiceMock.Object);
        }

        [Fact]
        public void GetInventories_ShouldReturnValue()
        {
            var inventoryRecords = new List<InventoryRecord>()
            {
                new InventoryRecord
                {
                }
            };

            _inventoryServiceMock.Setup(x => x.QueryByEntry(It.IsAny<IEnumerable<string>>())).Returns(inventoryRecords);
            _warehouseRepositoryMock.Setup(x => x.Get(It.IsAny<string>())).Returns(new Warehouse() { ContactInformation = new WarehouseContactInformation() });
            var inventories = _subject.GetInventories("default-entry-code");

            Assert.NotEmpty(inventories);
        }

    }
}
