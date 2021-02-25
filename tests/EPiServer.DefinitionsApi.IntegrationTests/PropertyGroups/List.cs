using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.PropertyGroups;
using EPiServer.Security;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.PropertyGroups
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class List
    {
        private readonly ServiceFixture _fixture;

        public List(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ListAsync_ShouldReturnPropertyGroupInArrayWithCorrectSortOrder()
        {
            var tabDefinitions = new List<TabDefinition>
            {
                new TabDefinition { Name = "GroupTest1", RequiredAccess = AccessLevel.Create, SortIndex = 0 },
                new TabDefinition { Name = "GroupTest2", RequiredAccess = AccessLevel.Create, SortIndex = 1 }
            };

            await _fixture.WithTabs(tabDefinitions, async () =>
            {
                var response = await _fixture.Client.GetAsync(PropertyGroupsController.RoutePrefix);

                AssertResponse.OK(response);

                var content = JArray.Parse(await response.Content.ReadAsStringAsync());

                Assert.NotNull(content.SingleOrDefault(x => x.Value<string>("name") == tabDefinitions[0].Name));
                Assert.NotNull(content.SingleOrDefault(x => x.Value<string>("name") == tabDefinitions[1].Name));
            });
        }
    }
}
