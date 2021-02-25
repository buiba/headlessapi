using EPiServer.ContentApi.Commerce.Internal.Models.Warehouse;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Inventory;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    [ServiceConfiguration(typeof(WarehouseService))]
    public class WarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public virtual WarehouseApiModel GetWarehouse(string warehouseCode)
        {
            var warehouse = _warehouseRepository.Get(warehouseCode);

            if (warehouse == null || !warehouse.IsActive)
            {
                return null;
            }

            var contactInformation = warehouse.ContactInformation;
            var warehouseContactInformationModel = new WarehouseContactInformationModel
            {
                FirstName = contactInformation.FirstName,
                LastName = contactInformation.LastName,
                Organization = contactInformation.Organization,
                Line1 = contactInformation.Line1,
                Line2 = contactInformation.Line2,
                City = contactInformation.City,
                State = contactInformation.State,
                CountryCode = contactInformation.CountryCode,
                CountryName = contactInformation.CountryName,
                PostalCode = contactInformation.PostalCode,
                RegionCode = contactInformation.RegionCode,
                RegionName = contactInformation.RegionName,
                DaytimePhoneNumber = contactInformation.DaytimePhoneNumber,
                EveningPhoneNumber = contactInformation.EveningPhoneNumber,
                FaxNumber = contactInformation.FaxNumber,
                Email = contactInformation.Email
            };

            return new WarehouseApiModel
            {
                Name = warehouse.Name,
                Modified = warehouse.Modified,
                Code = warehouse.Code,
                ContactInformation = warehouseContactInformationModel,
                IsFulfillmentCenter = warehouse.IsFulfillmentCenter,
                IsPickupLocation = warehouse.IsPickupLocation,
                IsDeliveryLocation = warehouse.IsDeliveryLocation
            };
        }
    }
}
