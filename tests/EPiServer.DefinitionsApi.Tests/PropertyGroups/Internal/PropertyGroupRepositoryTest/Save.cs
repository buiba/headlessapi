using System;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DataAbstraction;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal.PropertyGroupRepositoryTest
{
    public sealed class Save : TestBase
    {
        [Fact]
        public void WhenPropertyGroupIsNull_ShouldThrowArgumentNullException()
        {
            var subject = Subject(TabDefinition);
            Assert.Throws<ArgumentNullException>(() => subject.Save(null));
        }

        [Fact]
        public void WhenPropertyGroupNameIsEmpty_ShouldThrowProblemDetailException()
        {
            var subject = Subject(TabDefinition);
            Assert.Throws<ErrorException>(() => subject.Save(new PropertyGroupModel { }));
        }

        [Fact]
        public void WhenPropertyGroupNameDoesNotExist_ShouldReturnCreatedAction()
        {
            var subject = Subject(TabDefinition);

            TabDefinition tabDefinition = null;
            var newPropertyGroup = new PropertyGroupModel { Name = "new property group" };
            TabDefinitionRepository.Setup(t => t.Load(newPropertyGroup.Name)).Returns(tabDefinition);

            var action = subject.Save(newPropertyGroup);

            TabDefinitionRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
            TabDefinitionRepository.Verify(x => x.Save(It.IsAny<TabDefinition>()), Times.Once);
            Assert.Equal(SaveResult.Created, action);
        }

        [Fact]
        public void WhenPropertyGroupNameIsExisting_ShouldReturnUpdatedAction()
        {
            var subject = Subject(TabDefinition);
            var action = subject.Save(new PropertyGroupModel { Name = "Tab Name" });

            TabDefinitionRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
            TabDefinitionRepository.Verify(x => x.Save(It.IsAny<TabDefinition>()), Times.Once);
            Assert.Equal(SaveResult.Updated, action);
        }

    }
}
