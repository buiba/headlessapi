using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class CommerceIntegrationTestCollection : ICollectionFixture<CommerceServiceFixture>
    {
        public const string Name = "Commerce integration tests";
    }
}
