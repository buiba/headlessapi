using Xunit;

namespace EPiServer.DefinitionsApi.Commerce.IntegrationTests.TestSetup
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class ContentManagementCommerceIntegrationTestCollection : ICollectionFixture<CommerceServiceFixture>
    {
        public const string Name = "Content Management - Commerce integration tests";
    }
}
