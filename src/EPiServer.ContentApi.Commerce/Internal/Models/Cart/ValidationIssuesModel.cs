using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Cart
{
    public class LineItemValidationModel
    {
        /// <summary>
        /// The content id.
        /// </summary>
        public Guid ContentId { get; set; }
        
        /// <summary>
        /// The sku code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The validation issues.
        /// </summary>
        public IEnumerable<string> ValidationIssues;
    }
}
