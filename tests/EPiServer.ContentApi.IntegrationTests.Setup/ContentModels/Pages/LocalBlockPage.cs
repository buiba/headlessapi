using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Pages
{
    [ContentType(DisplayName = "LocalBlockPage", GUID = "7BF3337F-E109-4863-AF91-06A14E453A7D", Description = "")]
    public class LocalBlockPage : PageData
    {
        public virtual TextBlock LocalBlock { get; set; }
    }
}
