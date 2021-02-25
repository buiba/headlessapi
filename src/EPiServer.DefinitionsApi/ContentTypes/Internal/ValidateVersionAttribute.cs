using System;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal sealed class ValidateVersionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is string version)
            {
                return Version.TryParse(version, out var _) ? ValidationResult.Success : new ValidationResult($"The '{version}' is not valid version");
            }

            return new ValidationResult("Version on a ContentType should be a string");
        }
    }
}
