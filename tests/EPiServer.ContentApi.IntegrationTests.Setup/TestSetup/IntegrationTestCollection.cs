using System;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class IntegrationTestCollection : ICollectionFixture<ServiceFixture>
    {
        public const string Name = "Integration tests";
        public static readonly Guid DefaultSiteId = Guid.NewGuid();
        public static readonly Guid StartPageGuId = Guid.NewGuid();
    }
}
