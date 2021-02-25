using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;

namespace EPiServer.ContentManagementApi.Internal
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class ValidateVersionStatusAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is VersionStatus versionStatus)
            {
                if (!Enum.IsDefined(typeof(VersionStatus), versionStatus) || versionStatus.Equals(VersionStatus.NotCreated) || versionStatus.Equals(VersionStatus.PreviouslyPublished))
                {                    
                    return new ValidationResult($"The status '{versionStatus}' is invalid.");
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Property 'Status' should be a VersionStatus");
        }
    }
}
