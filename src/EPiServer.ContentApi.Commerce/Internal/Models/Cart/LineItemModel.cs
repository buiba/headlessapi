using System;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    /// <summary>
    /// Represents a line item.
    /// </summary>
    public class LineItemModel
    {
        /// <summary>
        /// The line item id.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The content id.
        /// </summary>
        [NotEmptyGuid]
        public Guid ContentId { get; set; }

        /// <summary>
        /// The sku code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The placed price.
        /// </summary>
        public decimal PlacedPrice { get; set; }

        /// <summary>
        /// The quantity.
        /// </summary>
        [Required]
        public decimal? Quantity { get; set; }

        /// <summary>
        /// The display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Indicates whether the line item is a gift item.
        /// </summary>
        public bool IsGift { get; set; }
    }
}