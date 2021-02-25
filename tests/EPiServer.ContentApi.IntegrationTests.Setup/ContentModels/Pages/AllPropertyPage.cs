using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Pages
{
    [ContentType]
    public class AllPropertyPage : PageData
    {
        public virtual string String { get; set; }

        [BackingType(typeof(PropertyAppSettings))]
        public virtual string AppSettings { get; set; }

        [BackingType(typeof(PropertyDocumentUrl))]       
        public virtual Url DocumentUrl { get; set; }

        [BackingType(typeof(PropertyDropDownList))]
        public virtual string DropDownList { get; set; }

        public virtual int Number { get; set; }

        [BackingType(typeof(PropertyWeekDay))]
        public virtual int WeekDay { get; set; }

        public virtual bool Boolean { get; set; }

        public virtual IList<DateTime> DateList { get; set; }

        public virtual Blob Blob { get; set; }

        public virtual XhtmlString XhtmlString { get; set; }

        public virtual ContentArea ContentArea { get; set; }

        public virtual ContentReference ContentReference { get; set; }

        public virtual IList<ContentReference> ContentReferenceList { get; set; }

        public virtual LinkItemCollection Links { get; set; }

        public virtual Url Url { get; set; }

        public virtual PageType PageType { get; set; }

        [CultureSpecific]
        public virtual string CultureSpecific { get; set; }
    }
}
