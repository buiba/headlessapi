using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Validation;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Pages
{
    [ContentType(DisplayName = "PageWithCustomValidator")]
    public class PageWithCustomValidator : PageData
    {
    }

    internal class AlwaysFailContentValidator : IContentSaveValidate<PageWithCustomValidator>
    {
        public IEnumerable<ValidationError> Validate(PageWithCustomValidator instance, ContentSaveValidationContext context)
        {
            yield return new ValidationError() { ErrorMessage = "Always Fail!" };           
        }
    }
}
