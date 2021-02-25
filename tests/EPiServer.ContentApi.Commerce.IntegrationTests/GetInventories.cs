using Castle.Core.Internal;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal.Models.Inventory;
using EPiServer.ContentApi.Commerce.Internal.Models.Warehouse;
using Xunit;
using Constants = EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup.Constants;
using System.Net.Http;

namespace EPiServer.ContentApi.IntegrationTests.Commerce
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class GetInventories : CommerceIntegrationTestBase
    {
        protected const string DefaultWarehouseCode = "default";
        private readonly CommerceServiceFixture _fixture;

        public GetInventories(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetInventories_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, CreateUrl(Guid.NewGuid()));
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task GetInventories_WhenExistsForSingleEntry_ShouldReturnInventory()
        {
            string warehouseCode = "warehouse1";
            decimal inStock = 3M;
            
            var variation = GetWithDefaultName<VariationContent>(CatalogContentLink);
            var warehouse = CreateWarehouseApiModelWithDefaults(warehouseCode);
            var expectedModel = CreateInventoryApiModelWithDefaults(variation.Code, warehouse, inStock);
            
            CreateInventoryFromModel(expectedModel, warehouse);

            var url = CreateUrl(variation.ContentGuid); 
            var inventoryResponse = await _fixture.Client.GetAsync(url);
            AssertResponse.OK(inventoryResponse);
            var inventoryModels = JsonConvert.DeserializeObject<IEnumerable<InventoryApiModel>>(await inventoryResponse.Content.ReadAsStringAsync());


            Assert.Single(inventoryModels);
            var actualModel = inventoryModels.Single();
            CompareResponse(actualModel, expectedModel);
        }
        
        [Fact]
        public async Task GetInventories_WhenEntryExistsInMultipleWareHouses_ShouldReturnMultipleInventories()
        {
            decimal inStock = 3M;
            var expectedModels = new List<InventoryApiModel>();

            var variation = GetWithDefaultName<VariationContent>(CatalogContentLink);

            var warehouse1 = CreateWarehouseApiModelWithDefaults("warehouse2");
            var warehouse2 = CreateWarehouseApiModelWithDefaults("warehouse3");
            expectedModels.Add(CreateInventoryApiModelWithDefaults(variation.Code, warehouse1, inStock));
            expectedModels.Add(CreateInventoryApiModelWithDefaults(variation.Code, warehouse2, inStock));
            CreateInventoryFromModel(expectedModels.First(), warehouse1);
            CreateInventoryFromModel(expectedModels.Last(), warehouse2);

            var url = CreateUrl(variation.ContentGuid); 
            var pricesResponse = await _fixture.Client.GetAsync(url);
            AssertResponse.OK(pricesResponse);
            var inventoryApiModels = JsonConvert.DeserializeObject<List<InventoryApiModel>>(await pricesResponse.Content.ReadAsStringAsync());

            inventoryApiModels.Count.Should().Be(expectedModels.Count);
            CompareResponse(inventoryApiModels.First(), expectedModels.First());
            CompareResponse(inventoryApiModels.Last(), expectedModels.Last());
        }


        [Fact]
        public async Task GetInventories_WhenEntryDoesNotExist_ShouldReturnNotFoundResponseStatusCode()
        {
            var unknownContentGuid = Guid.NewGuid();
            var url = CreateUrl(unknownContentGuid);
            var inventoryResponse = await _fixture.Client.GetAsync(url);
            inventoryResponse.StatusCode.Should().BeEquivalentTo(HttpStatusCode.NotFound);
        }

        string CreateUrl(Guid contentId) => Constants.InventoryApiBaseUrl + $"?contentId={contentId}";

        private void CompareResponse(InventoryApiModel actualModel, InventoryApiModel expectedModel)
        {
            actualModel.Should().BeEquivalentTo(expectedModel, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 100)).WhenTypeIs<DateTime>()
                .Using<String>(CheckNullOrEmpty).WhenTypeIs<String>());
        }

        private void CheckNullOrEmpty(IAssertionContext<string> obj)
        {
            if (obj.Expectation.IsNullOrEmpty())
            {
                obj.Subject.Should().BeNullOrWhiteSpace();
            }
            else
            {
                obj.Subject.Should().BeEquivalentTo(obj.Expectation);
            }
        }

        private WarehouseApiModel CreateWarehouseApiModelWithDefaults(string wareHouseCode = "")
        {
            var contactInformationModel = new WarehouseContactInformationModel
            {
                FirstName = "firstName",
                LastName = "lastName",
                Email = "email@ep.se"
            };
            return new WarehouseApiModel()
            {
                ContactInformation = contactInformationModel,
                Code = string.IsNullOrEmpty(wareHouseCode) ? DefaultWarehouseCode : wareHouseCode,
                Modified = DateTime.UtcNow
            };
        }

        private static InventoryApiModel CreateInventoryApiModelWithDefaults(string entryCode, WarehouseApiModel warehouseModel, decimal inStock = 10)
        {
            return new InventoryApiModel
            {
                EntryCode = entryCode,
                WarehouseCode = warehouseModel.Code,
                PurchaseAvailableQuantity = inStock,
                PurchaseAvailable = DateTime.UtcNow.AddDays(-7),
            };
        }
        private static void CreateInventoryFromModel(InventoryApiModel expectedModel, WarehouseApiModel warehouseModel)
        {
            var contactInformationModel = warehouseModel.ContactInformation;

            var wareHouseContactInformation = new WarehouseContactInformation()
            {
                FirstName = contactInformationModel.FirstName,
                LastName = contactInformationModel.LastName,
                Email = contactInformationModel.Email
            };
            var wareHouse = new Warehouse
            {
                Code = warehouseModel.Code,
                ContactInformation = wareHouseContactInformation,
                Modified = warehouseModel.Modified,
            };

            var inventoryRecord = new InventoryRecord()
            {
                CatalogEntryCode = expectedModel.EntryCode,
                WarehouseCode = warehouseModel.Code,
                PurchaseAvailableQuantity = expectedModel.PurchaseAvailableQuantity,
                PurchaseAvailableUtc = expectedModel.PurchaseAvailable
            };

            GetInstance<IWarehouseRepository>().Save(wareHouse);
            GetInstance<IInventoryService>().Insert(new List<InventoryRecord> { inventoryRecord });
        }
    }
}
