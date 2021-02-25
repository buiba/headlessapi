using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Pages
{
    [ContentType(DisplayName = "StartPage", GUID = "990f5975-c53b-4784-934d-019bf662bf86", Description = "")]
    public class StartPage : PageData
    {
        [CultureSpecific]
        public virtual XhtmlString MainBody { get; set; }

        [CultureSpecific]
        public virtual ContentArea MainContentArea { get; set; }
    }
}