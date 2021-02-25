using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.PropertyGroups;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.PropertyGroups
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class Delete
    {
        private readonly ServiceFixture _fixture;

        public Delete(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DeleteAsync_WhenPropertyGroupExists_ShouldReturnNoContent()
        {
            var propertyGroup = new { Name = "GroupTest", DisplayName = "GroupTest", SortIndex = 0, SystemGroup = false };
            await CreatePropertyGroup(propertyGroup);
            var response = await _fixture.Client.DeleteAsync(PropertyGroupsController.RoutePrefix + propertyGroup.Name);
            AssertResponse.NoContent(response);
        }

        [Fact]
        public async Task DeleteAsync_WhenPropertyGroupDoesNotExists_ShouldReturnNotFound()
        {
            var response = await _fixture.Client.DeleteAsync(PropertyGroupsController.RoutePrefix + "GroupNotFound");
            AssertResponse.NotFound(response);
        }

        [Fact]
        public async Task DeleteAsync_WithSystemGroup_ShouldReturnBadRequest()
        {
            var response = await _fixture.Client.DeleteAsync(PropertyGroupsController.RoutePrefix + "Information");
            AssertResponse.BadRequest(response);
        }

        private async Task CreatePropertyGroup(object propertyGroup)
        {
            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup));
            response.EnsureSuccessStatusCode();
        }
    }
}
