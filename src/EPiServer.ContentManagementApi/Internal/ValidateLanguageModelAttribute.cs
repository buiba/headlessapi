using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Internal
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class ValidateLanguageModelAttribute : ValidationAttribute
    {
        private readonly Injected<ILanguageBranchRepository> _languageBranchRepository;

        public ValidateLanguageModelAttribute()
        {
        }

        //Expose for test
        internal ValidateLanguageModelAttribute(ILanguageBranchRepository languageBranchRepository)
        {
            _languageBranchRepository.Service = languageBranchRepository;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is LanguageModel languageModel)
            {
                if (string.IsNullOrEmpty(languageModel.Name))
                {
                    return new ValidationResult("Language name cannot be null or empty.");
                }

                if (!CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Any(c => c.Name.Equals(languageModel.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ValidationResult($"The Language '{languageModel.Name}' doesn't exist.");                    
                }                

                if (!_languageBranchRepository.Service.ListEnabled().Any(l => l.LanguageID.Equals(languageModel.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ValidationResult($"The Language '{languageModel.Name}' is not enabled in CMS.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Property 'Language' should be a LanguageModel");
        }
    }
}
