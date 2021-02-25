using System;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DataAbstraction;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal.PropertyGroupRepositoryTest
{
    public sealed class Create : TestBase
    {
        [Fact]
        public void WhenPropertyGroupIsNull_ShouldThrowException()
        {
            var subject = Subject(TabDefinition);

            Assert.Throws<ArgumentNullException>(() => subject.Create(null));
        }

        [Fact]
        public void WhenPropertyGroupIsSystemGroup_ShouldThrowBadRequestErrorException()
        {
            var subject = Subject(TabDefinition);

            var problemDetail = Assert.Throws<ErrorException>(() => subject.Create(new PropertyGroupModel { SystemGroup = true, Name = "A system group" }));
            Assert.Equal("Cannot create the system group", problemDetail.ErrorResponse.Error.Message);
            Assert.Equal(HttpStatusCode.BadRequest, problemDetail.StatusCode);
            Assert.Equal(ProblemCode.SystemGroup, problemDetail.ErrorResponse.Error.Code);
        }

        [Fact]
        public void WhenPropertyGroupIsValid_ShouldReturnCreatedResult()
        {
            var subject = Subject(TabDefinition);
            var newPropertyGroup = new PropertyGroupModel { Name = "new property group" };

            var action = subject.Create(newPropertyGroup);

            TabDefinitionRepository.Verify(x => x.Save(It.IsAny<TabDefinition>()), Times.Once);
            Assert.Equal(SaveResult.Created, action);
        }
    }
}
