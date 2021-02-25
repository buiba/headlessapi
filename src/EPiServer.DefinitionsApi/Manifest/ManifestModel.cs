using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.DefinitionsApi.Manifest.Internal;
using Newtonsoft.Json;

namespace EPiServer.DefinitionsApi.Manifest
{
    /// <summary>
    /// Defines a manifest containing sections with definitions.
    /// </summary>
    [JsonConverter(typeof(ManifestModelConverter))]
    [ApiDefinition(Name = "manifest")]
    public class ManifestModel : IValidatableObject
    {
        /// <summary>
        /// Gets the sections containing definitions.
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, IManifestSection> Sections { get; } = new Dictionary<string, IManifestSection>();

        /// <inherit-doc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Sections == null || !Sections.Any())
            {
                yield return new ValidationResult("The manifest doesn't contain any sections.");
            }

            foreach (var section in Sections)
            {
                var results = new Collection<ValidationResult>();

                if (!DataAnnotationsValidator.TryValidateObjectRecursive(section, results))
                {
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
