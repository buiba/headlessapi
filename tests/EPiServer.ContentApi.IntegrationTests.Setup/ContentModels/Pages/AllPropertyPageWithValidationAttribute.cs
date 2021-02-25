using System.ComponentModel.DataAnnotations;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Pages
{
    [ContentType(DisplayName = "AllPropertyPageWithValidationAtribute", GUID = "a04411f8-427f-43ed-9e26-0b86782dec49", Description = "")]
    public class AllPropertyPageWithValidationAttribute : AllPropertyPage
    {
        [Required]
        public virtual string RequiredProperty { get; set; }

        public virtual NestedBlock NestedBlock { get; set; }
    }
}
