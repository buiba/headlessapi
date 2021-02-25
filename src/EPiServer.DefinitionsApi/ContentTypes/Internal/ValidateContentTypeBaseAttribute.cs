using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class ValidateContentTypeBaseAttribute : ValidationAttribute
    {
        public ValidateContentTypeBaseAttribute() : this(ServiceLocator.Current.GetInstance<IContentTypeBaseResolver>())
        { }

        public ValidateContentTypeBaseAttribute(IContentTypeBaseResolver contentTypeBaseResolver)
        {
            ContentTypeBaseResolver = contentTypeBaseResolver;
        }

        public IContentTypeBaseResolver ContentTypeBaseResolver { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string baseString)
            {
                var modelType = ContentTypeBaseResolver.Resolve(new ContentTypeBase(baseString));
                return modelType is null ?
                    new ValidationResult($"There is no base type registered with identifier '{baseString}'") :
                    ValidationResult.Success;
            }

            return new ValidationResult("Base on a ContentType should be a string");
        }
    }
}
