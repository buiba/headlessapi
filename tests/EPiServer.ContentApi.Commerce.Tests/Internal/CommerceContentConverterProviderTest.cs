using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Internal
{
    public class CommerceContentConverterProviderTest
    {
        private CommerceContentConverterProvider Subject() => new CommerceContentConverterProvider(new CommerceContentModelMapper(null, null, null, (IPropertyConverterResolver)null, null, null, null, null, Enumerable.Empty<ICatalogContentModelBuilder>()));

        [Fact]
        public void Resolve_IfContentIsCatalogContent_ShouldResolve()
        {
            Assert.NotNull(Subject().Resolve(Mock.Of<CatalogContentBase>()));
        }

        [Fact]
        public void Resolve_IfContentIsNotCatalogContent_ShouldNotResolve()
        {
            Assert.Null(Subject().Resolve(Mock.Of<IContent>()));
        }
    }
}
