using System;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog.ContentTypes.Internal;
using EPiServer.Commerce.Marketing.Internal;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.Commerce.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using Xunit;

namespace EPiServer.DefinitionsApi.Commerce.IntegrationTests.ContentTypes
{
    [Collection(ContentManagementCommerceIntegrationTestCollection.Name)]
    public sealed class Delete
    {
        private const string BaseControllerRoute = "api/episerver/v2.0/contenttypes/";
        private readonly CommerceServiceFixture _fixture;
        private MetaDataContext _metaDataContext;

        private MetaDataContext MetaDataContext
        {
            get => _metaDataContext ??= (_metaDataContext = CatalogContext.MetaDataContext);
            set => _metaDataContext = value;
        }

        public Delete(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DeleteAsync_WhenCmsContentTypeExists_ShouldReturnNoContent()
        {
            var id = Guid.NewGuid();
            var contentType = new { id, name = $"ContentType_{id:N}", baseType = ContentTypeBase.Page.ToString() };
            await CreateContentType(contentType);

            var response = await _fixture.Client.DeleteAsync(BaseControllerRoute + id);

            AssertResponse.NoContent(response);
        }

        [Fact]
        public async Task DeleteAsync_WhenMarketingContentTypeExists_ShouldReturnNoContent()
        {
            var id = Guid.NewGuid();
            var contentType = new { id, name = $"ContentType_{id:N}", baseType = MarketingContentTypeBase.Promotion.ToString() };
            await CreateContentType(contentType);

            var response = await _fixture.Client.DeleteAsync(BaseControllerRoute + id);

            AssertResponse.NoContent(response);
        }

        [Fact]
        public async Task DeleteAsync_WhenCatalogContentTypeExists_ShouldReturnNoContentAndDeleteMetaClass()
        {
            var id = Guid.NewGuid();
            var contentType = new { id, name = $"ContentType_{id:N}", baseType = CatalogContentTypeBase.Product.ToString() };
            await CreateContentType(contentType);

            var response = await _fixture.Client.DeleteAsync(BaseControllerRoute + id);

            var metaClassAfterDelete = MetaClass.Load(MetaDataContext, contentType.name);

            AssertResponse.NoContent(response);
            Assert.Null(metaClassAfterDelete);
        }

        private async Task CreateContentType(object contentType)
        {
            var response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));
            response.EnsureSuccessStatusCode();
        }

    }
}
