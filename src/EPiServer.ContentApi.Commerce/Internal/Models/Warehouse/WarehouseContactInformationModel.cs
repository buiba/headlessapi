namespace EPiServer.ContentApi.Commerce.Internal.Models.Warehouse
{
    public class WarehouseContactInformationModel
    {
        /// <summary>Gets or sets the contacts first name.</summary>
        /// <value>The contacts first name.</value>
        public string FirstName { get; set; }

        /// <summary>Gets or sets the contacts last name.</summary>
        /// <value>The contacts last name.</value>
        public string LastName { get; set; }

        /// <summary>Gets or sets the contacts organization name.</summary>
        /// <value>The name of the organization to which the contact belongs.</value>
        public string Organization { get; set; }

        /// <summary>
        /// Gets or sets the primary line of the contacts physical address.
        /// </summary>
        /// <value>The contacts primary physical address information.</value>
        public string Line1 { get; set; }

        /// <summary>
        /// Gets or sets the secondary line of the contacts physical address.
        /// </summary>
        /// <value>Any additional information for the contacts physical address.</value>
        public string Line2 { get; set; }

        /// <summary>
        /// Gets or sets the city of the contacts physical address.
        /// </summary>
        /// <value>The city name of the contacts physical address.</value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state of the contacts physical address.
        /// </summary>
        /// <value>The state code or name of the contacts physical address.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the country code of the contacts physical address.
        /// </summary>
        /// <value>The country code of the contacts physical address.</value>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the country of the contacts physical address.
        /// </summary>
        /// <value>The name of the country in which the contacts physical address is located.</value>
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the postal code of the contacts physical address.
        /// </summary>
        /// <value>The postal code of the contacts physical address.</value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the region code of the contacts physical address.
        /// </summary>
        /// <value>The code identifying the region in which the contacts physical address is located.</value>
        public string RegionCode { get; set; }

        /// <summary>
        /// Gets or sets the region name of the contacts physical address.
        /// </summary>
        /// <value>The name of the region in which the contacts physical address is located.</value>
        public string RegionName { get; set; }

        /// <summary>
        /// Gets or sets the phone number at which the contact can be reached during regular work hours.
        /// </summary>
        /// <value>The contacts phone number during regular work hours.</value>
        public string DaytimePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the phone number at which the contact can be reached outside of regular work hours.
        /// </summary>
        /// <value>The contacts phone number outside regular work hours.</value>
        public string EveningPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the phone number at which the contact can be reached by fax.
        /// </summary>
        /// <value>The contacts fax line phone number.</value>
        public string FaxNumber { get; set; }

        /// <summary>Gets or sets the contacts email address.</summary>
        /// <value>The contacts email address.</value>
        public string Email { get; set; }
    }
}
