using CustomMappers;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.ServiceLocation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Serialization
{
    [Collection(IntegrationTestCollection.Name)]
    public class Custom_Registrations
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private ServiceFixture _fixture;

        public Custom_Registrations(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenAnObsoletePropertyConverterIsRegistered_ShouldBeAbleToHandleProperty(bool optimizeForDerlivery)
        {
            var propertyValue = "something";
            var page = _fixture.GetWithDefaultName<PageWithCustomHandledProperty>(ContentReference.StartPage, true, init: p => { p.CustomPropertyMapped = propertyValue; });
            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var customPropertyMappedValue = optimizeForDerlivery ? (string)content["customPropertyMapped"] : (string)content["customPropertyMapped"]["value"];

                    Assert.Equal($"{CustomPropertyModelConverter.ConverterAddedprefix}{propertyValue}", customPropertyMappedValue);
                }
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenAnObsoleteContentMapperIsRegistered_ShouldBeAbleToHandleContent(bool optimizeForDerlivery)
        {
            var propertyValue = "something";
            var page = _fixture.GetWithDefaultName<PageWithCustomHandledProperty>(ContentReference.StartPage, true, init: p => 
            {
                p.Name = CustomContentModelMapper.HandledContentName;
                p.CustomPropertyMapped = propertyValue; 
            });
            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var customPropertyMappedValue = optimizeForDerlivery ? (string)content["customPropertyMapped"] : (string)content["customPropertyMapped"]["value"];

                    Assert.Equal($"{CustomPropertyModelConverter.ConverterAddedprefix}{CustomContentModelMapper.ContentMapperAddedPrefix}{propertyValue}", customPropertyMappedValue);
                }
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_WhenAContentFilterIsRegistered_ShouldBeAbleToRemovePropertyContent(bool optimizeForDerlivery)
        {
            var propertyValue = "something";
            var page = _fixture.GetWithDefaultName<PageWithCustomHandledProperty>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = CustomContentModelMapper.HandledContentName;
                p.CustomPropertyMapped = propertyValue;
                p.PropertyToBeRemovedByFilter = "This will not be serialized";
            });
            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(optimizeForDerlivery))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    if (!optimizeForDerlivery)
                        Assert.Equal(string.Empty, (string)content["propertyToBeRemovedByFilter"]["value"]);
                    else
                        Assert.Null(content["propertyToBeRemovedByFilter"]);
                }
            });
        }

        [Fact]
        public async Task Get_WhenAContentModelFilterIsRegistered_ShouldBeAbleToChangeDataOnProperties()
        {
            var propertyValue = "something";
            var page = _fixture.GetWithDefaultName<PageWithCustomHandledProperty>(ContentReference.StartPage, true, init: p =>
            {
                p.Name = CustomContentModelMapper.HandledContentName;
                p.CustomPropertyMapped = propertyValue;
            });
            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(false))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.False((content["contentLink"]["id"] as JToken).HasValues);
                    Assert.False((content["contentLink"]["workId"] as JToken).HasValues);
                }
            });
        }
    }
}
