using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentApi.Commerce.Internal
{
    [ServiceConfiguration(typeof(IPropertyConverterProvider), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class SeoInformationPropertyConverterProvider : IPropertyConverterProvider
    {
        private readonly SeoInformationPropertyModelConverter _seoInformationPropertyModelConverter;

        public SeoInformationPropertyConverterProvider(SeoInformationPropertyModelConverter seoInformationPropertyModelConverter)
        {
            _seoInformationPropertyModelConverter = seoInformationPropertyModelConverter;
        }
        public int SortOrder => 200;

        public IPropertyConverter Resolve(PropertyData propertyData)
        {
            return (propertyData is PropertyBlock block && block.Value is SeoInformation) ? _seoInformationPropertyModelConverter : null;
        }
    }
}
