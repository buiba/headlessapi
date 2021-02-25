using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks
{
    [ContentType(DisplayName = "TextBlock", GUID = "6b4ea1a1-ae2f-4ae1-b377-13cc137fb125", Description = "")]
    public class TextBlock : BlockData
    {
        [CultureSpecific]
        public virtual string Heading { get; set; }

        [CultureSpecific]
        public virtual XhtmlString MainBody { get; set; }

        public virtual Url TextLink { get; set; }

        public virtual NotFlattenableBlock NotFlattenableProperty { get; set; }

        public virtual NestedBlock NestedBlock { get; set; }
    }

    [ContentType(DisplayName = "NestedBlock")]
    public class NestedBlock : BlockData
    {
        [CultureSpecific]
        public virtual string Title { get; set; }
    }

    [ContentType(DisplayName = "NonBranchSpecificNestedBlock")]
    public class NonBranchSpecificNestedBlock : BlockData
    {
        public virtual string Title { get; set; }
    }
}
