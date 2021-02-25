using System.Linq;
using EPiServer.DataAbstraction;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal.PropertyGroupRepositoryTest
{
    public sealed class List : TestBase
    {
        [Fact]
        public void ShouldReturnGroupsFromRepository()
        {
            var existing = new[]
            {
                new TabDefinition {  Name = "One" },
                new TabDefinition { Name = "Two" }
            };
            var subject = Subject(existing);

            var result = subject.List();

            Assert.Equal(existing.Select(x => x.Name), result.Select(x => x.Name));
        }
    }
}
