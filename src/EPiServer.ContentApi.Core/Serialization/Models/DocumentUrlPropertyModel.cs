using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyDocumentUrl"/>
    /// </summary>
    public class DocumentUrlPropertyModel : PropertyModel<string, PropertyDocumentUrl>
    {
        [JsonConstructor]
        internal DocumentUrlPropertyModel() { }

        public DocumentUrlPropertyModel(PropertyDocumentUrl propertyDocumentUrl) : base(propertyDocumentUrl)
        {
            Value = propertyDocumentUrl.ToString();
        }
    }
}
