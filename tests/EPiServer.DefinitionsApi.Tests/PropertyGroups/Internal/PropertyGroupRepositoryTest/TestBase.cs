using System.Linq;
using EPiServer.DataAbstraction;
using Moq;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal.PropertyGroupRepositoryTest
{
    public abstract class TestBase
    {
        protected readonly TabDefinition TabDefinition;
        protected readonly Mock<ITabDefinitionRepository> TabDefinitionRepository;

        protected TestBase()
        {
            TabDefinition = new TabDefinition { ID = 1, Name = "Tab Name" };
            TabDefinitionRepository = new Mock<ITabDefinitionRepository>();
        }

        private protected PropertyGroupRepository Subject(params TabDefinition[] tabDefinitions)
        {
            TabDefinitionRepository.Setup(t => t.List()).Returns(tabDefinitions);
            TabDefinitionRepository.Setup(t => t.Load(TabDefinition.Name)).Returns<string>(name => tabDefinitions.FirstOrDefault(x => x.Name == name));
            return new PropertyGroupRepository(TabDefinitionRepository.Object);
        }

    }
}
