using EPiServer.DataAbstraction;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal.PropertyGroupRepositoryTest
{
    public sealed class Get : TestBase
    {
        [Fact]
        public void WhenMatchingTabdefinitionExists_ShouldReturnTheCorrespondingPropertyGroup()
        {
            var subject = Subject(TabDefinition);

            var result = subject.Get(TabDefinition.Name);

            Assert.NotNull(result);
            Assert.Equal(TabDefinition.Name, result.Name);
        }

        [Fact]
        public void WhenNoMatchingTabdefinitionExists_ShouldReturnNull()
        {
            var subject = Subject(TabDefinition);

            var result = subject.Get("Tab Not Found");

            Assert.Null(result);
        }
    }
}
