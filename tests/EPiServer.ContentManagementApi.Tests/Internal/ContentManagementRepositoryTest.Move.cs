using System;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.Core;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        [Fact]
        public void Move_ByContentGuid_WhenDestinationIsNull_ShouldThrowArgumentNullException()
        {
            var _ = _mockContent.Object;
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);

            Assert.Throws<ArgumentNullException>(() => Subject(_mockContentRepository.Object).Move(It.IsAny<Guid>(), null));
        }

        [Fact]
        public void Move_ByContentGuid_WhenSourceContentDoesNotExists_ShouldReturnFalse()
        {
            Assert.False(Subject().Move(It.IsAny<Guid>(), new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }));
        }

        [Fact]
        public void Move_ByContentGuid_WhenDestinationDoesNotExists_ShouldThrowException()
        {
            var sourceContentGuid = Guid.NewGuid();
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var output = content.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.Is<Guid>(s => s == sourceContentGuid), out output)).Returns(true);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(sourceContentGuid, new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }));
            Assert.Equal(HttpStatusCode.BadRequest, problemDetail.StatusCode);
        }

        [Fact]
        public void Move_ByContentGuid_WhenSourceContentIsNotExposedToApi_ShouldThrowException()
        {
            var sourceContent = new Mock<IContent>();
            sourceContent.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            sourceContent.Setup(c => c.ContentGuid).Returns(Guid.NewGuid());
            var source = sourceContent.Object;

            var destinationContent = new Mock<IContent>();
            destinationContent.Setup(c => c.ContentLink).Returns(new ContentReference(11));
            destinationContent.Setup(c => c.ContentGuid).Returns(Guid.NewGuid());
            var destination = destinationContent.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.Is<Guid>(s => s == source.ContentGuid), out source)).Returns(true);
            _mockContentRepository.Setup(m => m.TryGet(It.Is<Guid>(s => s == destination.ContentGuid), out destination)).Returns(true);

            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.Is<IContent>(s => s == source))).Returns(false);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.Is<IContent>(s => s == destination))).Returns(true);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(source.ContentGuid, new ContentReferenceInputModel { GuidValue = destination.ContentGuid }));
            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Move_ByContentGuid_WhenDestinationContentIsNotExposedToApi_ShouldThrowException()
        {
            var sourceContent = new Mock<IContent>();
            sourceContent.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            sourceContent.Setup(c => c.ContentGuid).Returns(Guid.NewGuid());
            var source = sourceContent.Object;

            var destinationContent = new Mock<IContent>();
            destinationContent.Setup(c => c.ContentLink).Returns(new ContentReference(11));
            destinationContent.Setup(c => c.ContentGuid).Returns(Guid.NewGuid());
            var destination = destinationContent.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.Is<Guid>(s => s == source.ContentGuid), out source)).Returns(true);
            _mockContentRepository.Setup(m => m.TryGet(It.Is<Guid>(s => s == destination.ContentGuid), out destination)).Returns(true);

            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.Is<IContent>(s => s == source))).Returns(true);
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.Is<IContent>(s => s == destination))).Returns(false);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(source.ContentGuid, new ContentReferenceInputModel { GuidValue = destination.ContentGuid }));
            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Move_ByContentGuid_WhenSourceContentIsSystemContent_ShouldThrowException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(c => c.ContentLink).Returns(new ContentReference(2));

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);
            _mockSiteDefinitionRepository.Setup(m => m.List()).Returns(new[] { SiteDefinitionMock() });

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(It.IsAny<Guid>(), new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }));
            Assert.Equal(HttpStatusCode.BadRequest, problemDetail.StatusCode);
            Assert.Equal(ProblemCode.SystemContent, problemDetail.ErrorResponse.Error.Code);
        }

        [Fact]
        public void Move_ByContentGuid_WhenSourceContentAndDestinationExists_ShouldReturnTrue()
        {
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var output = content.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out output)).Returns(true);
            _mockContentRepository.Setup(m => m.Move(It.IsAny<ContentReference>(), It.IsAny<ContentReference>(), It.IsAny<AccessLevel>(), It.IsAny<AccessLevel>())).Returns(new ContentReference());

            Assert.True(Subject(_mockContentRepository.Object).Move(It.IsAny<Guid>(), new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }));
        }

        [Fact]
        public void Move_ByContentGuid_WhenUnAuthorized_ShouldReturnAccessDeniedException()
        {
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var output = content.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<Guid>(), out output)).Returns(true);
            _mockContentRepository
                .Setup(m => m.Move(It.IsAny<ContentReference>(), It.IsAny<ContentReference>(), It.IsAny<AccessLevel>(), It.IsAny<AccessLevel>()))
                .Throws(new AccessDeniedException());

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(It.IsAny<Guid>(), new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }));

            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Move_ByContentReference_WhenDestinationIsNull_ShouldThrowArgumentNullException()
        {
            var _ = _mockContent.Object;
            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);

            Assert.Throws<ArgumentNullException>(() => Subject(_mockContentRepository.Object).Move(It.IsAny<ContentReference>(), null));
        }

        [Fact]
        public void Move_ByContentReference_WhenSourceContentDoesNotExists_ShouldReturnFalse()
        {
            Assert.False(Subject().Move(It.IsAny<ContentReference>(), new ContentReferenceInputModel { GuidValue = Guid.NewGuid() }));
        }

        [Fact]
        public void Move_ByContentReference_WhenDestinationDoesNotExists_ShouldThrowException()
        {
            var sourceContentLink = new ContentReference("123");
            _mockContent.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var output = _mockContent.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.Is<ContentReference>(s => s == sourceContentLink), out output)).Returns(true);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(sourceContentLink, new ContentReferenceInputModel { Id = 12345 }));
            Assert.Equal(HttpStatusCode.BadRequest, problemDetail.StatusCode);
        }

        [Fact]
        public void Move_ByContentReference_WhenSourceContentIsNotExposedToApi_ShouldThrowException()
        {
            var sourceContent = new Mock<IContent>();
            sourceContent.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var source = sourceContent.Object;

            var destinationContent = new Mock<IContent>();
            destinationContent.Setup(c => c.ContentLink).Returns(new ContentReference(11));
            var destination = destinationContent.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.Is<ContentReference>(s => s == source.ContentLink), out source)).Returns(true);
            _mockContentRepository.Setup(m => m.TryGet(It.Is<ContentReference>(s => s == destination.ContentLink), out destination)).Returns(true);

            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.Is<IContent>(s => s == source))).Returns(false);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(source.ContentLink, new ContentReferenceInputModel { Id = destination.ContentLink.ID }));
            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Move_ByContentReference_WhenDestinationContentIsNotExposedToApi_ShouldThrowException()
        {
            var sourceContent = new Mock<IContent>();
            sourceContent.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var source = sourceContent.Object;

            var destinationContent = new Mock<IContent>();
            destinationContent.Setup(c => c.ContentLink).Returns(new ContentReference(11));
            var destination = destinationContent.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.Is<ContentReference>(s => s == source.ContentLink), out source)).Returns(true);
            _mockContentRepository.Setup(m => m.TryGet(It.Is<ContentReference>(s => s == destination.ContentLink), out destination)).Returns(true);

            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.Is<IContent>(s => s == destination))).Returns(false);

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(source.ContentLink, new ContentReferenceInputModel { Id = destination.ContentLink.ID }));
            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }

        [Fact]
        public void Move_ByContentReference_WhenSourceContentIsSystemContent_ShouldThrowException()
        {
            var _ = _mockContent.Object;
            _mockContent.Setup(c => c.ContentLink).Returns(new ContentReference(2));

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out _)).Returns(true);
            _mockRequiredRoleEvaluator.Setup(m => m.HasAccess(It.IsAny<IContent>())).Returns(true);
            _mockSiteDefinitionRepository.Setup(m => m.List()).Returns(new[] { SiteDefinitionMock() });

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(It.IsAny<ContentReference>(), new ContentReferenceInputModel { Id = 2 }));
            Assert.Equal(HttpStatusCode.BadRequest, problemDetail.StatusCode);
            Assert.Equal(ProblemCode.SystemContent, problemDetail.ErrorResponse.Error.Code);
        }

        [Fact]
        public void Move_ByContentReference_WhenSourceContentAndDestinationExists_ShouldReturnTrue()
        {
            _mockContent.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var output = _mockContent.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out output)).Returns(true);
            _mockContentRepository.Setup(m => m.Move(It.IsAny<ContentReference>(), It.IsAny<ContentReference>(), It.IsAny<AccessLevel>(), It.IsAny<AccessLevel>())).Returns(new ContentReference());

            Assert.True(Subject(_mockContentRepository.Object).Move(It.IsAny<ContentReference>(), new ContentReferenceInputModel { Id = 123 }));
        }

        [Fact]
        public void Move_ByContentReference_WhenUnAuthorized_ShouldReturnAccessDeniedException()
        {
            var content = new Mock<IContent>();
            content.Setup(c => c.ContentLink).Returns(new ContentReference(10));
            var output = content.Object;

            _mockContentRepository.Setup(m => m.TryGet(It.IsAny<ContentReference>(), out output)).Returns(true);
            _mockContentRepository
                .Setup(m => m.Move(It.IsAny<ContentReference>(), It.IsAny<ContentReference>(), It.IsAny<AccessLevel>(), It.IsAny<AccessLevel>()))
                .Throws(new AccessDeniedException());

            var problemDetail = Assert.Throws<ErrorException>(() => Subject(_mockContentRepository.Object).Move(It.IsAny<ContentReference>(), new ContentReferenceInputModel { Id = 123 }));

            Assert.Equal(HttpStatusCode.Forbidden, problemDetail.StatusCode);
        }
    }
}
