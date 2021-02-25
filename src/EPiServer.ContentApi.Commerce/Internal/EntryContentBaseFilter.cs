using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    [ServiceConfiguration(typeof(IContentFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class EntryContentBaseFilter : ContentFilter<EntryContentBase>
    {
        public override void Filter(EntryContentBase content, ConverterContext converterContext)
        {
            content.Property.Remove("Categories");
            content.Property.Remove("ParentEntries");
            content.Property.Remove("Associations");

            switch (content)
            {
                case VariationContent _:
                    content.Property.Remove("PriceReference");
                    content.Property.Remove("InventoryReference");
                    return;
                case ProductContent _:
                    content.Property.Remove("VariantsReference");
                    return;
                case BundleContent _:
                    content.Property.Remove("BundleReference");
                    return;
                case PackageContent _:
                    content.Property.Remove("PackageReference");
                    content.Property.Remove("PriceReference");
                    content.Property.Remove("InventoryReference");
                    return;
            }
        }
    }
}
