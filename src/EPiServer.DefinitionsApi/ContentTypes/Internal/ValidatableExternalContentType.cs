#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1604 // Element documentation should have summary
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    /// <exclude />
    public class ValidatableExternalContentType : ExternalContentType, IValidatableObject
    {
        private readonly ContentTypeValidator _contentTypeValidator;

        /// <exclude />
        public ValidatableExternalContentType()
            : this(ServiceLocator.Current.GetInstance<ContentTypeValidator>())
        {
        }

        internal ValidatableExternalContentType(ContentTypeValidator contentTypeValidator)
        {
            _contentTypeValidator = contentTypeValidator;
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => _contentTypeValidator.Validate(new[] { this });
    }
}
