using EPiServer.ContentApi.Commerce.Internal.Services;
using Mediachase.Commerce.Inventory;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Services
{
    public class WarehouseServiceTest
    {
        private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
        private readonly WarehouseService _subject;

        public WarehouseServiceTest()
        {
            _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
            _subject = new WarehouseService(_warehouseRepositoryMock.Object);
        }

        [Fact]
        public void GetWarehouse_WhenWarehouseExists_ShouldReturnWarehouse()
        {
            _warehouseRepositoryMock.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new Warehouse{ContactInformation = new WarehouseContactInformation(), IsActive = true});
            var result = _subject.GetWarehouse("warehouseCode");

            Assert.NotNull(result);
        }

        [Fact]
        public void GetWarehouse_WhenWarehouseDoesNotExists_ShouldReturnNull()
        {
            var result = _subject.GetWarehouse("warehouseCode");

            Assert.Null(result);
        }
    }
}