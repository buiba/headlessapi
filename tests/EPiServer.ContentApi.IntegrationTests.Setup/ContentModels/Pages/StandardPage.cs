using System.Collections.Generic;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Properties;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Pages
{
    [ContentType(DisplayName = "StandardPage", GUID = "a04411f8-427f-43ed-9e26-0b86782dec31", Description = "")]
    public class StandardPage : PageData
    {
        [CultureSpecific]
        public virtual string Heading { get; set; }

        [CultureSpecific]
        public virtual XhtmlString MainBody { get; set; }

        [CultureSpecific]
        public virtual ContentArea MainContentArea { get; set; }

        [CultureSpecific]
        public virtual ContentReference TargetReference { get; set; }

        [CultureSpecific]
        public virtual IList<ContentReference> ContentReferenceList { get; set; }

        [CultureSpecific]
        public virtual LinkItemCollection Links { get; set; }

        [CultureSpecific]
        public virtual Url Uri { get; set; }
    }

    [ContentType(DisplayName = "StandardPageWithPageImage", GUID = "2EEA99E5-60B1-4F72-BBA5-47E3A000E6E0", Description = "")]
    public class StandardPageWithPageImage : StandardPage
    {
        [CultureSpecific]
        public virtual Url PageImage { get; set; }
    }

    [ContentType(DisplayName = "StandardPageWithBlock", GUID = "2EEA99E5-60B1-4F72-BBA5-47E3A000E6E1", Description = "")]
    public class StandardPageWithBlock : StandardPage
    {
        public virtual TextBlock LocalBlock { get; set; }
    }

    [ContentType(DisplayName = "StandardPageWithNonBranchSpecificProperty", GUID = "886E76BA-B68F-4A0B-96C5-528DB04A0C36", Description = "")]
    public class StandardPageWithNonBranchSpecificProperty : StandardPage
    {
        public virtual string NonBranchSpecificProperty { get; set; }
    }

    [ContentType(DisplayName = "StandardPageWithNestedBlock", GUID = "2EEA99E5-60B1-4F72-BBA5-47E3A000E6E4", Description = "")]
    public class StandardPageWithNestedBlock : StandardPage
    {
        public virtual NestedBlock NestedBlock { get; set; }
    }

    [ContentType(DisplayName = "GenericFile", GUID = "aa531491-cd9f-4c4f-90e8-5af59f43c328", Description = "")]
    public class GenericFile : MediaData
    {
        [CultureSpecific]
        public virtual string Description { get; set; }
    }

    [ContentType(DisplayName = "StandardPageWithNonBranchSpecificNestedBlock", GUID = "66a8015c-513e-4739-93a2-8806cba48d14", Description = "")]
    public class StandardPageWithNonBranchSpecificNestedBlock : StandardPage
    {
        public virtual NonBranchSpecificNestedBlock NonBranchSpecificNestedBlock { get; set; }
    }

    [ContentType(DisplayName = "StandardPageWithCustomPropertyLongString", GUID = "2EEA99E5-60B1-4F72-BBA5-47E3A010E6E0", Description = "")]
    public class StandardPageWithCustomPropertyLongString : PageData
    {
        [BackingType(typeof(CustomPropertyLongString))]
        public virtual string CustomProperty { get; set; }
    }
}
