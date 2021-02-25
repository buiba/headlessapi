using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.Forms.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Forms.Internal
{
    [ServiceConfiguration(typeof(IContentConverterProvider), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class FormContentConverterProvider : IContentConverterProvider
    {
        private readonly FormContentModelMapper _formContentModelMapper;

        public FormContentConverterProvider(FormContentModelMapper formContentModelMapper)
        {
            _formContentModelMapper = formContentModelMapper;
        }
        public int SortOrder => 200;

        public IContentConverter Resolve(IContent content) => (content is IFormContainerBlock) ? _formContentModelMapper : null;
    }
}
