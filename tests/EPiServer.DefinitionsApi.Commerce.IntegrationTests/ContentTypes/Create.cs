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
    public sealed class Create
    {
        private const string BaseControllerRoute = "api/episerver/v2.0/contenttypes/";
        private readonly CommerceServiceFixture _fixture;
        private MetaDataContext _metaDataContext;

        private MetaDataContext MetaDataContext
        {
            get => _metaDataContext ??= CatalogContext.MetaDataContext;
            set => _metaDataContext = value;
        }

        public Create(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateAsync_NewCmsContentType_ShouldCreateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = ContentTypeBase.Page.ToString() };

            await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(BaseControllerRoute + contentType.id);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task CreateAsync_NewMarketingContentType_ShouldCreateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = MarketingContentTypeBase.Promotion.ToString() };

            await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(BaseControllerRoute + contentType.id);

            AssertResponse.OK(response);
        }

        [Fact]
        public async Task CreateAsync_NewCatalogContentType_ShouldCreateContentTypeAndMetaClass()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = CatalogContentTypeBase.Variation.ToString() };

            await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));

            var response = await _fixture.Client.GetAsync(BaseControllerRoute + contentType.id);
            var metaClassSaved = MetaClass.Load(MetaDataContext, contentType.name);

            AssertResponse.OK(response);
            Assert.NotNull(metaClassSaved);
            Assert.Equal(contentType.name, metaClassSaved.Name);
        }

        [Fact]
        public async Task CreateAsync_MarketingContentType_WithNewContentTypeID_WhenAnotherContentTypeWithTheSameNameAlreadyExists_ShouldReturnConflictError()
        {
            var existing = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = MarketingContentTypeBase.SalesCampaign.ToString() };

            var response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(existing));
            response.EnsureSuccessStatusCode();

            var contentType = new { id = Guid.NewGuid(), existing.name, existing.baseType };

            response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task CreateAsync_CatalogContentType_WithNewContentTypeID_WhenAnotherContentTypeWithTheSameNameAlreadyExists_ShouldReturnConflictError()
        {
            var existing = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = CatalogContentTypeBase.Product.ToString() };

            var response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(existing));
            response.EnsureSuccessStatusCode();

            var contentType = new { id = Guid.NewGuid(), existing.name, existing.baseType };
            response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task CreateAsync_MarketingContentType_WithoutContentTypeID_WhenAnotherContentTypeWithTheSameNameAlreadyExists_ShouldReturnConflictError()
        {
            var existing = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = MarketingContentTypeBase.SalesCampaign.ToString() };

            var response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(existing));
            response.EnsureSuccessStatusCode();

            var contentType = new { existing.name, existing.baseType };
            response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task CreateAsync_CatalogContentType_WithoutContentTypeID_WhenAnotherContentTypeWithTheSameNameAlreadyExists_ShouldReturnConflictError()
        {
            var existing = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = CatalogContentTypeBase.Product.ToString() };

            var response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(existing));
            response.EnsureSuccessStatusCode();

            var contentType = new { existing.name, existing.baseType };
            response = await _fixture.Client.PostAsync(BaseControllerRoute, new JsonContent(contentType));

            AssertResponse.Conflict(response);
        }

    }
}
