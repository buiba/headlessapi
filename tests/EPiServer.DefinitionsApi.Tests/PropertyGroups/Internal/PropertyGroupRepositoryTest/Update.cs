using System;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DataAbstraction;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal.PropertyGroupRepositoryTest
{
    public sealed class Update : TestBase
    {
        [Fact]
        public void WhenPropertyGroupIsNull_ShouldThrowException()
        {
            var subject = Subject(TabDefinition);

            Assert.Throws<ArgumentNullException>(() => subject.Update(TabDefinition, null));
        }

        [Fact]
        public void WhenExistingTabDefinitionIsNull_ShouldThrowException()
        {
            var subject = Subject(TabDefinition);

            Assert.Throws<ArgumentNullException>(() => subject.Update(null, new PropertyGroupModel { Name = "property group name" }));
        }
        [Fact]
        public void WhenSystemGroupWasModified_ShouldThrowConflictProblemDetailsException()
        {
            var subject = Subject(TabDefinition);

            var problemDetail = Assert.Throws<ErrorException>(
                () => subject.Update(TabDefinition, new PropertyGroupModel { Name = "A system group", SystemGroup = !TabDefinition.IsSystemTab }));

            Assert.Equal("The system group property is read-only and cannot be modified.", problemDetail.ErrorResponse.Error.Message);
            Assert.Equal(HttpStatusCode.Conflict, problemDetail.StatusCode);
            Assert.Equal(ProblemCode.SystemGroup, problemDetail.ErrorResponse.Error.Code);
        }
    }
}
