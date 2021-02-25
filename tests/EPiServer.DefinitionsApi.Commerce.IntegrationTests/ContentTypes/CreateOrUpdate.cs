using System;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog.ContentTypes.Internal;
using EPiServer.Commerce.Marketing.Internal;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.Commerce.IntegrationTests.TestSetup;
using FluentAssertions;
using FluentAssertions.Json;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using Xunit;
using EPiServer.Core;

namespace EPiServer.DefinitionsApi.Commerce.IntegrationTests.ContentTypes
{
    [Collection(ContentManagementCommerceIntegrationTestCollection.Name)]
    public sealed class CreateOrUpdate
    {
        private const string BaseControllerRoute = "api/episerver/v2.0/contenttypes/";
        private readonly CommerceServiceFixture _fixture;
        private MetaDataContext _metaDataContext;

        private MetaDataContext MetaDataContext
        {
            get => _metaDataContext ??= CatalogContext.MetaDataContext;
            set => _metaDataContext = value;
        }

        public CreateOrUpdate(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public async Task CreateOrUpdateAsync_NewMarketingContentType_ShouldCreateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = MarketingContentTypeBase.Promotion.ToString() };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            response.EnsureSuccessStatusCode();

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{MarketingContentTypeBase.Promotion}', properties:[] }}");
        }

        [Fact]
        public async Task CreateOrUpdateAsync_NewCatalogContentType_ShouldCreateContentType()
        {
            var contentType = new { id = Guid.NewGuid(), name = $"ContentType_{Guid.NewGuid():N}", baseType = CatalogContentTypeBase.Product.ToString() };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            response.EnsureSuccessStatusCode();

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            var metaClassSaved = MetaClass.Load(MetaDataContext, contentType.name);

            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo($"{{ id: '{contentType.id}', name: '{contentType.name}', baseType: '{CatalogContentTypeBase.Product}', properties:[] }}");
            metaClassSaved.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateOrUpdateAsync_NewCatalogContentType_WithMetaFields_ShouldCreateContentType()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = CatalogContentTypeBase.Product.ToString(),
                properties = new[] { new { name = "Prop1", dataType = nameof(PropertyNumber) },
                                    new { name = "Prop2", dataType = nameof(PropertyString) },
                }
            };

            var response = await CallCreateOrUpdateAsync(contentType.id, contentType);

            response.EnsureSuccessStatusCode();

            response = await GetContentType(contentType.id);
            var content = await response.Content.ReadAsStringAsync();
            var metaClassSaved = MetaClass.Load(MetaDataContext, contentType.name);

            metaClassSaved.Should().NotBeNull();
            metaClassSaved.MetaFields.Should().Contain(x => x.Name.Equals("Prop1"));
            metaClassSaved.MetaFields.Should().Contain(x => x.Name.Equals("Prop2"));
        }

        private Task<HttpResponseMessage> CallCreateOrUpdateAsync(Guid id, object contentType) => _fixture.Client.PutAsync(BaseControllerRoute + id, new JsonContent(contentType));

        private async Task<HttpResponseMessage> GetContentType(Guid id)
        {
            var response = await _fixture.Client.GetAsync(BaseControllerRoute + id);
            response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
