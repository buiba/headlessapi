using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Commerce.Internal.Services;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.Cart
{
    [Collection(CommerceIntegrationTestCollection.Name)]
    public class PutCart : CommerceIntegrationTestBase
    {
        private readonly CommerceServiceFixture _fixture;

        public PutCart(CommerceServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PutCart_WithEmptyId_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);

            var (response, _) = await _fixture.PutCartAsync(Guid.Empty, cartModel);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutCart_WithInvalidId_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);

            var (response, _) = await _fixture.PutCartAsync(Guid.NewGuid(), cartModel);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutCart_WhenCartDoesNotExist_ShouldReturn404()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_,savedCart) = await _fixture.PostCartAsync(cartModel);
            var validUnknownId = new CartIdConverter(new GuidEncoder()).ConvertToGuid(666);

            var (response, _) = await _fixture.PutCartAsync(validUnknownId, savedCart);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutCart_WhenCartExists_ShouldReturn200()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_,savedCart) = await _fixture.PostCartAsync(cartModel);
            savedCart.Currency = "SEK";
            
            var (response, updatedCart) = await _fixture.PutCartAsync(savedCart.Id.Value, savedCart);
          
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            CompareResponse(updatedCart, savedCart);
        }

        [Fact]
        public async Task PutCart_ShouldIgnoreLineItemPropertiesThatAreReadonly()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            savedCart.Shipments.Single().LineItems.Single().DisplayName = "other";
            savedCart.Shipments.Single().LineItems.Single().PlacedPrice = 666;
            savedCart.Shipments.Single().LineItems.Single().Code = "unknownskucode";

            var (response, updatedCart) = await _fixture.PutCartAsync(savedCart.Id.Value, savedCart);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedLineItem = new LineItemModel
            {
                Code = variant.Code,
                ContentId = variant.ContentGuid,
                DisplayName = variant.DisplayName,
                PlacedPrice = 0,
                Quantity = 1
            };
            updatedCart.Shipments.Single().LineItems.Single().Should()
                .BeEquivalentTo(expectedLineItem, o => o.Excluding(x => x.Id));
        }

        [Fact]
        public async Task PutCart_WhenContentIdIsInvalid_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            savedCart.Shipments.Single().LineItems.Single().ContentId = Guid.NewGuid();

            var (response, _) = await _fixture.PutCartAsync(savedCart.Id.Value, savedCart);
           
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutCart_WithNoPayload_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var (response, _) = await _fixture.PutCartAsync(savedCart.Id.Value, null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutCart_WhenAddingLineItems_ShouldReturn200()
        {
            var variant1 = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant1);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var variant2 = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            savedCart.Shipments.Single().LineItems = savedCart.Shipments.Single().LineItems.Concat(new [] 
            {
                new LineItemModel{ContentId = variant2.ContentGuid, Code = variant2.Code, DisplayName = variant2.DisplayName, Quantity = 1} 
            });

            var (response, updatedCart) = await _fixture.PutCartAsync(savedCart.Id.Value, savedCart);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            CompareResponse(updatedCart, savedCart);
        }

        [Fact]
        public async Task PutCart_WhenAddingLineItems_WithBundle_ShouldReturn200()
        {
            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var bundle = GetWithDefaultName<BundleContent>(CatalogContentLink);
            var variation1 = GetWithDefaultName<VariationContent>(CatalogContentLink);
            var variation2 = GetWithDefaultName<VariationContent>(CatalogContentLink);
            Relation relation = new BundleEntry();
            relation.Parent = bundle.ContentLink;
            relation.Child = variation1.ContentLink;
            
            Relation relation1 = new BundleEntry();
            relation1.Parent = bundle.ContentLink;
            relation1.Child = variation2.ContentLink;

            GetInstance<IRelationRepository>().UpdateRelations(new []{relation, relation1});

            var cart = savedCart;

            var lineItems = new LineItemModel[]
                {cart.Shipments.Single().LineItems.FirstOrDefault(), new LineItemModel(){ContentId = bundle.ContentGuid, Code = bundle.Code, Quantity = 1}};
            cart.Shipments.Single().LineItems = lineItems;

            var (response, updatedCart) = await _fixture.PutCartAsync(savedCart.Id.Value, cart);
           
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Expected cart should include all children which belongs to the bundle
            var expectedCart = savedCart;

            var expectedCartLineItems = new LineItemModel[]
            {
                cart.Shipments.Single().LineItems.FirstOrDefault(),
                new LineItemModel() {ContentId = bundle.ContentGuid, Code = bundle.Code, Quantity = 1},
                new LineItemModel {ContentId = variation1.ContentGuid, Code = variation1.Code, Quantity = 1},
                new LineItemModel {ContentId = variation2.ContentGuid, Code = variation2.Code, Quantity = 1}
            };

            expectedCart.Shipments.Single().LineItems = expectedCartLineItems;

            CompareResponse(updatedCart, expectedCart);
        }

        [Fact]
        public async Task PutCart_WhenAddingLineItems_WithBundleInsideBundle_ShouldReturn200()
        {
            var variant = GetWithDefaultName<VariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);

            var bundle = GetWithDefaultName<BundleContent>(CatalogContentLink);
            var variation1 = GetWithDefaultName<VariationContent>(CatalogContentLink);

            var bundle2 = GetWithDefaultName<BundleContent>(CatalogContentLink);
            var variation2 = GetWithDefaultName<VariationContent>(CatalogContentLink);
            var variation3 = GetWithDefaultName<VariationContent>(CatalogContentLink);

            Relation relation = new BundleEntry();
            relation.Parent = bundle.ContentLink;
            relation.Child = variation1.ContentLink;

            Relation relation1 = new BundleEntry();
            relation1.Parent = bundle.ContentLink;
            relation1.Child = bundle2.ContentLink;

            Relation relation2 = new BundleEntry();
            relation2.Parent = bundle2.ContentLink;
            relation2.Child = variation2.ContentLink;

            Relation relation3 = new BundleEntry();
            relation3.Parent = bundle2.ContentLink;
            relation3.Child = variation3.ContentLink;

            GetInstance<IRelationRepository>().UpdateRelations(new[] { relation, relation1, relation2, relation3 });

            var cart = savedCart;

            var lineItems = new LineItemModel[]
            {
                cart.Shipments.Single().LineItems.First(),
                new LineItemModel()
                {
                    ContentId = bundle.ContentGuid,
                    Code = bundle.Code,
                    Quantity = 1
                }
            };

            cart.Shipments.Single().LineItems = lineItems;

            var (response, updatedCart) = await _fixture.PutCartAsync(savedCart.Id.Value, cart);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Expected cart should include all children which belongs to the bundle
            var expectedCart = savedCart;
           
            var expectedCartLineItems = new LineItemModel[]
            {
                cart.Shipments.Single().LineItems.First(),
                new LineItemModel() { ContentId = bundle.ContentGuid, Code = bundle.Code, Quantity = 1},
                new LineItemModel() {ContentId = bundle2.ContentGuid, Code = bundle2.Code, Quantity = 1},
                new LineItemModel {ContentId = variation1.ContentGuid, Code = variation1.Code, Quantity = 1},
                new LineItemModel {ContentId = variation2.ContentGuid, Code = variation2.Code, Quantity = 1},
                new LineItemModel() {ContentId = variation3.ContentGuid, Code = variation3.Code, Quantity = 1}
            };

            expectedCart.Shipments.Single().LineItems = expectedCartLineItems;

            CompareResponse(updatedCart, expectedCart);
        
        }

        [Fact]
        public async Task PutCart_WhenCustomerIdIsMissing_ShouldReturn400()
        {
            var variant = GetWithDefaultName<SampleVariationContent>(CatalogContentLink);
            var cartModel = CreateCart(variant);
            var (_, savedCart) = await _fixture.PostCartAsync(cartModel);
            savedCart.CustomerId = Guid.Empty;

            var (response, _) = await _fixture.PutCartAsync(savedCart.Id.Value, savedCart);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private void CompareResponse(CartApiModel actualModel, CartApiModel expectedModel)
        {
            actualModel.Should().BeEquivalentTo(expectedModel, options => options
                .Excluding(x => x.SelectedMemberPath.EndsWith("Id"))
                .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTimeOffset>());
        }
    }
}
