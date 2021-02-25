using System.Collections.Generic;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents the response data for cart operations. 
    /// </summary>
    public class CheckoutApiModel
    {
        /// <summary>
        /// The cart.
        /// </summary>
        public CartApiModel Cart { get; set; }

        /// <summary>
        /// The cart totals
        /// </summary>
        public TotalsModel Totals { get; set; }

        /// <summary>
        /// The available shipping methods per shipment.
        /// </summary>
        public IEnumerable<ShippingMethodModel> AvailableShippingMethods { get; set; }

        /// <summary>
        /// The validation issues per line item.
        /// </summary>
        public IEnumerable<LineItemValidationModel> ValidationIssues { get; set; }
    }
}
