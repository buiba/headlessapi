using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.PropertyGroups;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.PropertyGroups
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed partial class CreateOrUpdate
    {
        private readonly ServiceFixture _fixture;
        private readonly ITabDefinitionRepository _tabDefinitionRepository;

        public CreateOrUpdate(ServiceFixture fixture)
        {
            _fixture = fixture;
            _tabDefinitionRepository = ServiceLocator.Current.GetInstance<ITabDefinitionRepository>();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10001)]
        public async Task WhenPropertyGroupSortIndexIsNotInValidRange_ShouldReturnValidationError(int index)
        {
            var propertyGroup = new { name = "Test", sortIndex = index };
            var response = await CallCreateOrUpdateAsync(propertyGroup.name, propertyGroup);

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task WhenPropertyGroupIsNull_ShouldReturnBadRequestCode()
        {
            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, null);
            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task WhenPropertyGroupIsMissingName_ShouldReturnValidationError()
        {
            var propertyGroup = new { displayName = "Test" };
            AssertResponse.ValidationError(await CallCreateOrUpdateAsync("test", propertyGroup));
        }

        [Fact]
        public async Task WhenPropertyGroupNameDoesNotMatchLocation_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = "view", displayName = "build" };
            AssertResponse.ValidationError(await CallCreateOrUpdateAsync("help", propertyGroup));
        }

        [Fact]
        public async Task WhenPropertyGroupNameContainsInvalidCharacters_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = "invalid-#@", displayName = "invalid name" };
            AssertResponse.ValidationError(await CallCreateOrUpdateAsync(propertyGroup.name, propertyGroup));
        }

        [Fact]
        public async Task WhenPropertyGroupAlreadyExists_ShouldUpdatePropertyGroup()
        {
            var tabDefinition = new TabDefinition { Name = "GroupTest", RequiredAccess = AccessLevel.Create, SortIndex = 0 };
            await _fixture.WithTab(tabDefinition, async () =>
            {
                var propertyGroup = new PropertyGroupModel { Name = "GroupTest", DisplayName = "Commerce group", SortIndex = 0, SystemGroup = false };

                // Change property group and submit
                propertyGroup.DisplayName = "Cms";
                var response = await CallCreateOrUpdateAsync(propertyGroup.Name, propertyGroup);

                AssertResponse.OK(response);

                response = await _fixture.Client.GetAsync(PropertyGroupsController.RoutePrefix + propertyGroup.Name);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                content.Should().BeValidJson()
                    .Which.Should().BeEquivalentTo(JsonConvert.SerializeObject(propertyGroup));
            });
        }

        [Fact]
        public async Task WhenSystemGroupPropertyIsUpdated_ShouldReturnConflict()
        {
            var propertyGroup = new PropertyGroupModel { Name = SystemTabNames.Content, DisplayName = "Content", SortIndex = 100, SystemGroup = false };

            var response = await CallCreateOrUpdateAsync(propertyGroup.Name, propertyGroup);

            AssertResponse.Conflict(response);
        }

        [Fact]
        public async Task WhenUpdatingSystemGroup_ShouldUpdatePropertyGroup()
        {
            var propertyGroup = new PropertyGroupModel { Name = SystemTabNames.Content, DisplayName = "Content", SortIndex = 1000 };

            var response = await CallCreateOrUpdateAsync(propertyGroup.Name, propertyGroup);

            AssertResponse.OK(response);

            response = await _fixture.Client.GetAsync(PropertyGroupsController.RoutePrefix + propertyGroup.Name);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            propertyGroup.SystemGroup = true; 
            content.Should().BeValidJson()
                .Which.Should().BeEquivalentTo(JsonConvert.SerializeObject(propertyGroup));
        }

        [Fact]
        public async Task WithNewSystemGroup_ShouldReturnBadRequest()
        {
            var propertyGroup = new { name = "PutSystemGroup", displayName = "file", systemGroup = true };
            var response = await CallCreateOrUpdateAsync("PutSystemGroup", propertyGroup);
            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task WithNewPropertyGroup_ShouldCreatePropertyGroup()
        {
            try
            {
                var propertyGroup = new { name = "File", displayName = "file" };
                var response = await CallCreateOrUpdateAsync("File", propertyGroup);
                AssertResponse.Created(response);
                Assert.Equal(propertyGroup.name, response.Headers.Location.AbsolutePath.Substring(PropertyGroupsController.RoutePrefix.Length + 1));
            }
            finally
            {
                //Remove the Tab after being created to avoid affecting other tests
                _tabDefinitionRepository.Delete(_tabDefinitionRepository.Load("File"));
            }
        }

        [Fact]
        public async Task WhenPropertyGroupNameIsTooLong_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = new string('S', 101) };

            var response = await CallCreateOrUpdateAsync(propertyGroup.name, propertyGroup);

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task WhenPropertyGroupDisplayNameIsTooLong_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = "Test", displayName = new string('S', 101) };

            var response = await CallCreateOrUpdateAsync(propertyGroup.name, propertyGroup);

            AssertResponse.ValidationError(response);
        }

        private Task<HttpResponseMessage> CallCreateOrUpdateAsync(string name, object propertyGroup) => _fixture.Client.PutAsync(PropertyGroupsController.RoutePrefix + name, new JsonContent(propertyGroup));
    }
}
