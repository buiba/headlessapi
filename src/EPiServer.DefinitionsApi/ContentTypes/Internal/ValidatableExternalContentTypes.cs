#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1604 // Element documentation should have summary
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    /// <exclude />
    public class ValidatableExternalContentTypes : List<ExternalContentType>, IValidatableObject
    {
        private readonly ContentTypeValidator _propertyValidator;

        /// <exclude />
        public ValidatableExternalContentTypes()
            : this(ServiceLocator.Current.GetInstance<ContentTypeValidator>())
        {
        }

        internal ValidatableExternalContentTypes(ContentTypeValidator propertyValidator)
        {
            _propertyValidator = propertyValidator;
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => _propertyValidator.Validate(this);
    }
}
