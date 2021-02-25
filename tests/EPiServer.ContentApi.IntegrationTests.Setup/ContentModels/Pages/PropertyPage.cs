using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Pages
{
    [ContentType]
    public class PropertyPage : PageData
    {
        public virtual bool Boolean { get; set; }

        public virtual ContentArea ContentArea { get; set; }

        public virtual ContentArea SecondaryContentArea { get; set; }

        public virtual Url Url { get; set; }
    }
}
