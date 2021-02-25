using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EPiServer.ContentManagementApi.Internal
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class ValidateContentTypeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IEnumerable<string> contentTypes)
            {
                if (!contentTypes.Any())
                {
                    return new ValidationResult("Content Type is required.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Property 'ContentType' should be an array of strings.");
        }
    }
}
