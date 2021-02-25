using System;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.Core;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void Delete_ByContentGuid_WhenContentIsNull_ShouldReturnFalse()
        {
            var result = Subject().TryDelete(It.IsAny<Guid>(), It.IsAny<bool>());
            Assert.False(result);
        }

        [Fact]
        public void Delete_ByContentGuid_WhenContentIsSystemContent_ShouldThrowException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(c => c.ContentLink).Returns(new ContentReference(2));

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);
            _mockSiteDefinitionRepository.Setup(m => m.List()).Returns(new[] { SiteDefinitionMock() });

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(It.IsAny<Guid>(), It.IsAny<bool>()));
            Assert.Equal(HttpStatusCode.BadRequest, problemDetail.StatusCode);
            Assert.Equal(ProblemCode.SystemContent, problemDetail.ErrorResponse.Error.Code);
        }

        [Fact]
        public void Delete_ByContentGuid_WhenContentIsDeleted_AndPermanentDeleteIsFalse_ShouldThrowException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(m => m.IsDeleted).Returns(true);
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);

            var exception = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(It.IsAny<Guid>(), false));
            Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);

            _mockContentRepository.Verify(m => m.Delete(It.IsAny<ContentReference>(), true, AccessLevel.Delete), Times.Never);
        }

        [Fact]
        public void Delete_ByContentGuid_WhenPermanentDeleteIsTrue_ShouldPermanentlyDelete()
        {
            var _ = _mockContent.Object;
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);

            var result = Subject(_mockContentRepository.Object).TryDelete(It.IsAny<Guid>(), true);
            Assert.True(result);
            _mockContentRepository.Verify(m => m.Delete(It.IsAny<ContentReference>(), true, AccessLevel.Delete), Times.Once);
        }

        [Fact]
        public void Delete_ByContentGuid_WhenContentIsNotDeleted_AndPermanentDeleteIsFalse_ShouldMoveToTrash()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(m => m.IsDeleted).Returns(false);
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);

            var result = Subject(_mockContentRepository.Object).TryDelete(It.IsAny<Guid>(), false);

            _mockContentRepository.Verify(m => m.MoveToWastebasket(It.IsAny<ContentReference>(), It.IsAny<string>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void Delete_ByContentGuid_WhenUnAuthorized_ShouldReturnException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(m => m.IsDeleted).Returns(false);
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);
            _mockContentRepository.Setup(m => m.MoveToWastebasket(It.IsAny<ContentReference>(), It.IsAny<string>())).Throws(new AccessDeniedException());

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(It.IsAny<Guid>(), It.IsAny<bool>()));

            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Delete_ByContentGuid_WhenContentIsNotExposedToApi_ShouldReturnException()
        {
            var _ = _mockContent.Object;
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(false);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(It.IsAny<Guid>(), It.IsAny<bool>()));

            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Delete_ByContentReference_WhenContentIsNull_ShouldReturnFalse()
        {
            var result = Subject().TryDelete(new ContentReference("123"), It.IsAny<bool>());
            Assert.False(result);
        }

        [Fact]
        public void Delete_ByContentReference_WhenContentIsSystemContent_ShouldThrowException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(c => c.ContentLink).Returns(new ContentReference(2));

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);
            _mockSiteDefinitionRepository.Setup(m => m.List()).Returns(new[] { SiteDefinitionMock() });

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(new ContentReference("123"), It.IsAny<bool>()));
            Assert.Equal(HttpStatusCode.BadRequest, problemDetail.StatusCode);
            Assert.Equal(ProblemCode.SystemContent, problemDetail.ErrorResponse.Error.Code);
        }

        [Fact]
        public void Delete_ByContentReference_WhenContentIsDeleted_AndPermanentDeleteIsFalse_ShouldThrowException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(m => m.IsDeleted).Returns(true);
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);

            var exception = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(new ContentReference("123"), false));
            Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);

            _mockContentRepository.Verify(m => m.Delete(It.IsAny<ContentReference>(), true, AccessLevel.Delete), Times.Never);
        }

        [Fact]
        public void Delete_ByContentReference_WhenPermanentDeleteIsTrue_ShouldPermanentlyDelete()
        {
            var _ = _mockContent.Object;
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);

            var result = Subject(_mockContentRepository.Object).TryDelete(new ContentReference("123"), true);
            Assert.True(result);
            _mockContentRepository.Verify(m => m.Delete(It.IsAny<ContentReference>(), true, AccessLevel.Delete), Times.Once);
        }

        [Fact]
        public void Delete_ByContentReference_WhenContentIsNotDeleted_AndPermanentDeleteIsFalse_ShouldMoveToTrash()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(m => m.IsDeleted).Returns(false);
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);

            var result = Subject(_mockContentRepository.Object).TryDelete(new ContentReference("123"), false);

            _mockContentRepository.Verify(m => m.MoveToWastebasket(It.IsAny<ContentReference>(), It.IsAny<string>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void Delete_ByContentReference_WhenUnAuthorized_ShouldReturnException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(m => m.IsDeleted).Returns(false);
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);
            _mockContentRepository.Setup(m => m.MoveToWastebasket(It.IsAny<ContentReference>(), It.IsAny<string>())).Throws(new AccessDeniedException());

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(new ContentReference("123"), It.IsAny<bool>()));

            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Delete_ByContentReference_WhenContentIsNotExposedToApi_ShouldReturnException()
        {
            var _ = _mockContent.Object;
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(false);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).TryDelete(new ContentReference("123"), It.IsAny<bool>()));

            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }
    }
}
