using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.PropertyGroups;
using EPiServer.ServiceLocation;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.PropertyGroups
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed partial class Create
    {
        private readonly ServiceFixture _fixture;
        private readonly ITabDefinitionRepository _tabDefinitionRepository;
        public Create(ServiceFixture fixture)
        {
            _fixture = fixture;
            _tabDefinitionRepository = ServiceLocator.Current.GetInstance<ITabDefinitionRepository>();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10001)]
        public async Task WhenPropertyGroupSortIndexIsNotInValidRange_ShouldReturnValidationError(int index)
        {
            var propertyGroup = new { name = "group", sortIndex = index };
            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task WhenPropertyGroupIsNull_ShouldReturnBadRequestCode()
        {
            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, null);
            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task WithASystemGroup_ShouldReturnBadRequest()
        {
            var newPropertyGroup = new { name = "Admin", displayName = "admin2", systemGroup = true };

            //  Try to create a new property group with the same name
            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(newPropertyGroup));
            AssertResponse.BadRequest(response);
        }

        [Fact]
        public async Task WhenPropertyGroupWithTheSameNameAlreadyExists_ShouldReturnConflict()
        {
            try
            {
                var existing = new { name = "Admin", displayName = "admin" };

                // Create a new property group
                var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(existing));
                response.EnsureSuccessStatusCode();

                var newPropertyGroup = new { name = "Admin", displayName = "admin2" };

                //  Try to create a new property group with the same name
                response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(newPropertyGroup));
                AssertResponse.Conflict(response);
            }
            finally
            {
                //Remove the Tab after being created to avoid affecting other tests
                _tabDefinitionRepository.Delete(_tabDefinitionRepository.Load("Admin"));
            }
        }

        [Fact]
        public async Task WithNewPropertyGroupName_ShouldCreatePropertyGroup()
        {
            try
            {
                var propertyGroup = new { name = "Edit", displayName = "edit" };

                var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup));
                var name = response.Headers.Location.AbsolutePath.Substring(PropertyGroupsController.RoutePrefix.Length + 1);

                AssertResponse.Created(response);
                Assert.Equal(propertyGroup.name, name);

                response = await _fixture.Client.GetAsync(PropertyGroupsController.RoutePrefix + "Edit");
                AssertResponse.OK(response);
            }
            finally
            {
                //Remove the Tab after being created to avoid affecting other tests
                _tabDefinitionRepository.Delete(_tabDefinitionRepository.Load("Edit"));
            }
        }

        [Fact]
        public async Task WhenPropertyGroupNameIsEmpty_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = string.Empty, displayName = "edit" };
            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task WhenPropertyGroupNameIsNull_ShouldReturnValidationError()
        {
            var propertyGroup = new { displayName = "edit" };
            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task WhenPropertyGroupNameIsTooLong_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = new string('S', 101) };

            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task WhenPropertyGroupDisplayNameIsTooLong_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = "group", displayName = new string('S', 101) };

            var response = await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task WhenPropertyGroupNameContainsInvalidCharacters_ShouldReturnValidationError()
        {
            var propertyGroup = new { name = "invalid-#@", displayName = "invalid name" };
            AssertResponse.ValidationError(await _fixture.Client.PostAsync(PropertyGroupsController.RoutePrefix, new JsonContent(propertyGroup)));
        }
    }
}
