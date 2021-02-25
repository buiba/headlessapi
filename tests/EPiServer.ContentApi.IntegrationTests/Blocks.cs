using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class Blocks
    {
        private const string V2Uri = "api/episerver/v2.0/content";
        private ServiceFixture _fixture;

        public Blocks(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_WhenRequestedContentHasLocalBlock_ShouldReturnLocalBlockContent()
        {
            var heading = "This is a heading";
            var text = "<p>and this is some content</p>";
            var page = _fixture.GetWithDefaultName<LocalBlockPage>(ContentReference.StartPage, true, init: p =>
            {
                p.LocalBlock.Heading = heading;
                p.LocalBlock.MainBody = new XhtmlString(text);
            });
            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Equal(heading, (string)content["localBlock"]["heading"]);
                Assert.Equal(text, (string)content["localBlock"]["mainBody"]);
            });
        }

        [Fact]
        public async Task Get_WhenPropertyOfLocalBlockHasFlattenedValueEqualNull_ShouldReturnNullAsFlattenedValue()
        {           
            var page = _fixture.GetWithDefaultName<LocalBlockPage>(ContentReference.StartPage, true);

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                Assert.Null(content["localBlock"]["textLink"]);                
            });
        }

        [Fact]
        public async Task Get_WhenPropertyOfLocalBlockIsNotImplementIFlattenable_ShouldNotFlattenThatProperty()
        {            
            var page = _fixture.GetWithDefaultName<LocalBlockPage>(ContentReference.StartPage, true);

            await _fixture.WithContent(page, async () =>
            {
                var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                AssertResponse.OK(contentResponse);
                var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());                
                Assert.Equal(nameof(PropertyBlock), content["localBlock"]["notFlattenableProperty"]["propertyDataType"]);
            });
        }

        [Fact]
        public async Task Get_WhenRequestedContentHasLocalBlockAndNotFlattened_ShouldReturnLocalBlockPropertiesWithType()
        {
            var heading = "This is a heading";
            var text = "<p>and this is some content</p>";
            var page = _fixture.GetWithDefaultName<LocalBlockPage>(ContentReference.StartPage, true, init: p =>
            {
                p.LocalBlock.Heading = heading;
                p.LocalBlock.MainBody = new XhtmlString(text);
            });
            await _fixture.WithContent(page, async () =>
            {
                using (var noFlattenScope = new OptionsScope(o => o.SetFlattenPropertyModel(false)))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Equal(heading, (string)content["localBlock"]["heading"]["value"]);
                    Assert.Equal(text, (string)content["localBlock"]["mainBody"]["value"]);
                    Assert.Equal(nameof(PropertyBlock), (string)content["localBlock"]["propertyDataType"]);
                    Assert.Equal(nameof(PropertyXhtmlString), (string)content["localBlock"]["mainBody"]["propertyDataType"]);
                    Assert.Equal(nameof(PropertyLongString), (string)content["localBlock"]["heading"]["propertyDataType"]);
                }
            });
        }        
    }
}

