using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Commerce.Internal
{
    [ServiceConfiguration(typeof(IContentFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class NodeContentBaseFilter : ContentFilter<NodeContentBase>
    {
        public override void Filter(NodeContentBase content, ConverterContext converterContext)
        {
            content.Property.Remove("Categories");
        }
    }
}
