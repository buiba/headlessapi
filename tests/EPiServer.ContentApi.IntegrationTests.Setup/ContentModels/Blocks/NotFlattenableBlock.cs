using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks
{
    [ContentType(DisplayName = "NotFlattenableBlock", GUID = "ed70e2a6-1d80-4a51-9aa7-bb91609ccf1b")]
    public class NotFlattenableBlock : BlockData
    {
        public virtual string Title { get; set; }

        public virtual Url Image { get; set; }
    }
}
