using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMappers
{
    [ContentType]
    public class PageWithCustomHandledProperty : PageData
    {
        [BackingType(typeof(PropertyString))]
        public virtual string CustomPropertyMapped { get; set; }

        public virtual string PropertyToBeRemovedByFilter { get; set; }

        public virtual ContentReference TargetReference { get; set; }

        public virtual ContentArea MainArea { get; set; }

        public virtual LinkItemCollection Links { get; set; }

        public virtual IList<ContentReference> ContentReferences { get; set; }
    }
}
