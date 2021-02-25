using System;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void HandleError_WhenAccessDeniedException_ShouldReturnForbidden()
        {
            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Throws<AccessDeniedException>();

            var subject = Subject(contentRepository.Object);

            var exception = Assert.Throws<ErrorException>(() => subject.HandleError(() => contentRepository.Object.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>())));

            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        }

        [Theory]
        [MemberData(nameof(InvalidData))]
        public void HandleError_WhenValidationException_ShouldReturnBadRequest(Exception inputException, string problemCode)
        {
            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Throws(inputException);

            var subject = Subject(contentRepository.Object);

            var exception = Assert.Throws<ErrorException>(() => subject.HandleError(() => contentRepository.Object.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>())));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal(problemCode, exception.ErrorResponse.Error.Code);
        }

        [Fact]
        public void HandleError_WhenActionIsValid_ShouldRunTheActionWell()
        {
            var contentRepository = new Mock<IContentRepository>();
            contentRepository.Setup(x => x.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>(), It.IsAny<AccessLevel>())).Returns(Mock.Of<ContentReference>());

            var subject = Subject(contentRepository.Object);

            var contentLink = subject.HandleError(() => contentRepository.Object.Save(It.IsAny<IContent>(), It.IsAny<SaveAction>()));

            Assert.NotNull(contentLink);
        }
    }
}
