namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents a customer address
    /// todo: associate with commerce customers via business foundation?
    /// todo: add more properties.
    /// </summary>
    public class AddressModel
    {
        /// <summary>
        /// The first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The line1.
        /// </summary>
        public string Line1 { get; set; }

        /// <summary>
        /// The line2.
        /// </summary>
        public string Line2 { get; set; }

        /// <summary>
        /// The city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The country name.
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// The postal code
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// The region name/state
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// The email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The phone number.
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}