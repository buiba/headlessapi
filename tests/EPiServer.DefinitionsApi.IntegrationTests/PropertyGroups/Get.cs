using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.PropertyGroups;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.PropertyGroups
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class Get
    {
        private readonly ServiceFixture _fixture;

        public Get(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAsync_WhenPropertyGroupDoesNotExits_ShouldReturnNotFound()
        {
            var response = await _fixture.Client.GetAsync(PropertyGroupsController.RoutePrefix + "GroupNotFound");
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task GetAsync_WhenPropertyGroupExists_ShouldReturnPropertyGroup()
        {
            var tabDefinition = new TabDefinition { Name = "GroupTest", SortIndex = 0, IsSystemTab = false };

            await _fixture.WithTab(tabDefinition, async () =>
            {
                var response = await _fixture.Client.GetAsync(PropertyGroupsController.RoutePrefix + tabDefinition.Name);

                AssertResponse.OK(response);

                var content = await response.Content.ReadAsStringAsync();
                content.Should().BeValidJson()
                    .Which.Should().BeEquivalentTo($"{{ name: 'GroupTest', sortIndex: 0, systemGroup: false}}");
            });
        }
    }
}
