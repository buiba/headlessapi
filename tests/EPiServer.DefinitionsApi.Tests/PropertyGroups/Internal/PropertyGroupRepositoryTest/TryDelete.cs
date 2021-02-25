using EPiServer.DataAbstraction;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal.PropertyGroupRepositoryTest
{
    public sealed class TryDelete : TestBase
    {
        [Fact]
        public void WhenNoMatchingPropertyGroupExists_ShouldReturnFalse()
        {
            var subject = Subject(TabDefinition);

            Assert.False(subject.TryDelete("TabNotExits"));
        }

        [Fact]
        public void WhenMatchingPropertyGroupExists_ShouldReturnTrue()
        {
            var subject = Subject(TabDefinition);

            Assert.True(subject.TryDelete(TabDefinition.Name));
        }

        [Fact]
        public void WhenMatchingPropertyGroupExists_ShouldCallDeleteMethodOnTabDefinitionRepository()
        {
            var subject = Subject(TabDefinition);

            subject.TryDelete(TabDefinition.Name);

            TabDefinitionRepository.Verify(x => x.Delete(TabDefinition), Times.Once());
        }

        [Fact]
        public void WhenGiveNameOfPropertyGroupIsNull_ShouldReturnFalse()
        {
            var subject = Subject(TabDefinition);

            Assert.False(subject.TryDelete(""));
        }

    }
}
