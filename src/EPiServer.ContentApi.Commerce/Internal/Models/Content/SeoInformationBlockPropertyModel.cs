using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Mapped property model for <see cref="SeoInformation"/>
    /// </summary>
    internal class SeoInformationBlockPropertyModel: PropertyModel<object, PropertyBlock>
    {
        public SeoInformationBlockPropertyModel(PropertyBlock propertyBlock)
            : base(propertyBlock)
        {
            var seoInfo = propertyBlock.Value as SeoInformation;
            Value = new
            {
                seoInfo.Title,
                seoInfo.Description,
                seoInfo.Keywords
            };
        }
    }
}
