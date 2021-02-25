using Castle.Core.Internal;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Mediachase.Commerce.Inventory;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal.Models.Warehouse;
using Xunit;
using Constants = EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup.Constants;
using System.Net.Http;

namespace EPiServer.ContentApi.IntegrationTests.Commerce
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class GetWarehouse : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;

        public GetWarehouse(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetWarehouse_ShouldAllowOptionsMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Options,  $"{Constants.WarehousesApiBaseUrl}warehouse");
            var response = await _fixture.Client.SendAsync(request);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task GetWarehouse_WhenWarehouseExists_ShouldReturnWarehouse()
        {
            var warehouseModel = CreateWarehouseApiModelWithDefaults("warehouse4");
            CreateWarehouseFromModel(warehouseModel);

            var url = Constants.WarehousesApiBaseUrl + $"{warehouseModel.Code}";
            var response = await _fixture.Client.GetAsync(url);
            AssertResponse.OK(response);
            var resultModel = JsonConvert.DeserializeObject<WarehouseApiModel>(await response.Content.ReadAsStringAsync());

            CompareResponse(resultModel, warehouseModel);
        }

        [Fact]
        public async Task GetWarehouse_WhenWarehouseDoesNotExists_ShouldReturnNotFoundResponseStatusCode()
        {
            var url = Constants.WarehousesApiBaseUrl + $"nonexistantWarehouse";
            var response = await _fixture.Client.GetAsync(url);
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task GetWarehouse_WhenWarehouseIsInactive_ShouldReturnNotFoundResponseStatusCode()
        {
            const string warehouseCode = "warehouse5";
            var wareHouse = new Warehouse
            {
                Code = warehouseCode,
                IsActive = false,
                ContactInformation = new WarehouseContactInformation()
            };
            GetInstance<IWarehouseRepository>().Save(wareHouse);

            var url = Constants.WarehousesApiBaseUrl + $"{warehouseCode}";
            var response = await _fixture.Client.GetAsync(url);
            AssertResponse.NotFound(response);
        }

        private WarehouseApiModel CreateWarehouseApiModelWithDefaults(string warehouseCode)
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
                Code = warehouseCode,
                Modified = DateTime.UtcNow
            };
        }

        private static void CreateWarehouseFromModel(WarehouseApiModel warehouseModel)
        {
            var wareHouseContactInformation = new WarehouseContactInformation()
            {
                FirstName = warehouseModel.ContactInformation.FirstName,
                LastName = warehouseModel.ContactInformation.LastName,
                Email = warehouseModel.ContactInformation.Email
            };
            var wareHouse = new Warehouse
            {
                Code = warehouseModel.Code,
                ContactInformation = wareHouseContactInformation,
                Modified = warehouseModel.Modified,
                IsActive = true
            };
            GetInstance<IWarehouseRepository>().Save(wareHouse);
        }

        private void CompareResponse(WarehouseApiModel actualModel, WarehouseApiModel expectedModel)
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
    }
}
