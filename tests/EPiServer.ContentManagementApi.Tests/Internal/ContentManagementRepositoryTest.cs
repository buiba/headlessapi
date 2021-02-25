using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.ContentManagementApi.Serialization;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public partial class ContentManagementRepositoryTest
    {
        private readonly Mock<IContentRepository> _mockContentRepository;
        private readonly Mock<ISiteDefinitionRepository> _mockSiteDefinitionRepository;
        private readonly Mock<IStatusTransitionEvaluator> _mockStatusTransitionEvaluator;
        private readonly Mock<IContent> _mockContent;
        private readonly Mock<IContentVersionRepository> _mockContentVersionRepository;
        private readonly Mock<IPermanentLinkMapper> _mockPermanentLinkMapper;
        private readonly Mock<IContentLoader> _mockContentLoader;
        private readonly Mock<RequiredRoleEvaluator> _mockRequiredRoleEvaluator;
        private readonly string _language = "sv";
        private readonly Guid _contentGuidTest = Guid.NewGuid();
        private readonly ContentReference _pageID = new ContentReference(1);

        public ContentManagementRepositoryTest()
        {
            _mockContentRepository = new Mock<IContentRepository>();
            _mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
            _mockContent = new Mock<IContent>();
            _mockContentVersionRepository = new Mock<IContentVersionRepository>();
            _mockPermanentLinkMapper = new Mock<IPermanentLinkMapper>();
            _mockContentLoader = new Mock<IContentLoader>();
            _mockRequiredRoleEvaluator = new Mock<RequiredRoleEvaluator>();
            _mockRequiredRoleEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>())).Returns(true);
            _mockStatusTransitionEvaluator = new Mock<IStatusTransitionEvaluator>();
        }

        private ContentManagementRepository Subject(
        IContentRepository contentRepository = null,
        IContentTypeRepository contentTypeRepository = null,
        IPropertyDataValueConverterResolver propertyDataValueConverterResolver = null
        )
        {
            var mockContentTypeRepository = new Mock<IContentTypeRepository>();
            mockContentTypeRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(new ContentType() { ModelType = typeof(PageData) });

            return new ContentManagementRepository(
                contentRepository ?? new Mock<IContentRepository>().Object,
                _mockContentVersionRepository.Object,
                _mockPermanentLinkMapper.Object,
                _mockSiteDefinitionRepository.Object,
                contentTypeRepository ?? mockContentTypeRepository.Object,
                propertyDataValueConverterResolver ?? new Mock<IPropertyDataValueConverterResolver>().Object,
                _mockContentLoader.Object,
                _mockRequiredRoleEvaluator.Object,
                _mockStatusTransitionEvaluator.Object
                );
        }

        [Fact]
        public void ValidateLocalizableContent_WhenContentIsLocalizable_AndNotHaveLanguage_ShouldThrowsException()
        {
            var exception = Assert.Throws<ErrorException>(() => Subject().ValidateLocalizableContent(Mock.Of<PageData>(), null));
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        }

        [Fact]
        public void ValidateLocalizableContent_WhenContentIsNotLocalizable_AndHaveLanguage_ShouldThrowsException()
        {
            var error = Assert.Throws<ErrorException>(() => Subject().ValidateLocalizableContent(Mock.Of<ImageData>(), new LanguageModel { Name = "sv" }));
            Assert.Equal(ProblemCode.ContentNotLocalizable, error.ErrorResponse.Error.Code);
        }

        private static SiteDefinition SiteDefinitionMock()
        {
            var mockSiteDefinition = new Mock<SiteDefinition>();
            mockSiteDefinition.Setup(m => m.ContentAssetsRoot).Returns(new ContentReference(1));
            mockSiteDefinition.Setup(m => m.GlobalAssetsRoot).Returns(new ContentReference(2));
            mockSiteDefinition.Setup(m => m.RootPage).Returns(new ContentReference(3));
            mockSiteDefinition.Setup(m => m.WasteBasket).Returns(new ContentReference(4));
            mockSiteDefinition.Setup(m => m.SiteAssetsRoot).Returns(new ContentReference(5));
            mockSiteDefinition.Setup(m => m.StartPage).Returns(new ContentReference(6));
            return mockSiteDefinition.Object;
        }

        public static TheoryData InvalidParent => new TheoryData<ContentReferenceInputModel>
        {
            null,
            new ContentReferenceInputModel(),
            new ContentReferenceInputModel()
            {
                GuidValue = Guid.NewGuid()
            }
        };

        public static TheoryData InvalidData => new TheoryData<object, string>
        {
            {new ValidationException(), ProblemCode.ContentValidation },
            {new EPiServerException("You cannot save a content that is read-only. Call CreateWritableClone() on content and pass the cloned instance instead."), ProblemCode.ReadOnlyContent },
            {new EPiServerException("Content type Page is not allowed to be created under parent of content type Product"), ProblemCode.NotAllowedParent },
            {new ArgumentException("Cannot force new version and force current version at the same time."), ProblemCode.ForceVersion },
            {new ArgumentException("The delayed published flag must be used in combination with the check-in flag."), ProblemCode.DelayedPulish },
            {new ArgumentException("The action Save is not valid on this content (ContentLink='1', Status='published')"), ProblemCode.InvalidAction },
            {new InvalidOperationException("StartPublish must be set when content item is set for scheduled publishing"), ProblemCode.ScheduledPublishing },
            {new InvalidOperationException("The action Save is not valid on this content (ContentLink='1', Status='published')"), ProblemCode.InvalidAction },
            {new InvalidOperationException($"Product does not implement ILocalizable"), ProblemCode.ContentNotLocalizable },
            {new NotSupportedException(), ProblemCode.ContentProvider },
            {SqlExceptionCreator.Create(
                "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_tblWorkContentProperty_tblContent\". The conflict occurred in database \"EPiServerDB_9ec5cc11\", " +
                "table \"dbo.tblContent\", column 'pkID'.", 547), ProblemCode.PropertyReferenceNotFound },
            {SqlExceptionCreator.Create(
                "The UPDATE statement conflicted with the FOREIGN KEY constraint \"FK_tblWorkContentProperty_tblContent\". The conflict occurred in database \"EPiServerDB_9ec5cc11\", " +
                "table \"dbo.tblContent\", column 'pkID'.", 547), ProblemCode.PropertyReferenceNotFound }
        };

        public static TheoryData ValidStatus => new TheoryData<VersionStatus, SaveAction>
        {
            { VersionStatus.CheckedIn, SaveAction.CheckIn },
            { VersionStatus.CheckedOut, SaveAction.CheckOut },
            { VersionStatus.Published, SaveAction.Publish },
            { VersionStatus.DelayedPublish, SaveAction.Schedule },
            { VersionStatus.Rejected, SaveAction.Reject },
            { VersionStatus.AwaitingApproval, SaveAction.RequestApproval }
        };

        public static TheoryData InvalidStatus => new TheoryData<VersionStatus>
        {
            { VersionStatus.NotCreated},
            { VersionStatus.PreviouslyPublished}
        };

        public static TheoryData InvalidInputContentLink => new TheoryData<ContentReference>
        {
            null,
            ContentReference.EmptyReference
        };
    }

    public static class SqlExceptionCreator
    {
        public static SqlException Create(string message, int errorCode)
        {
            SqlException exception = Instantiate<SqlException>();
            SetProperty(exception, "_message", message);
            var errors = new ArrayList();
            var errorCollection = Instantiate<SqlErrorCollection>();
            SetProperty(errorCollection, "errors", errors);
            var error = Instantiate<SqlError>();
            SetProperty(error, "number", errorCode);
            errors.Add(error);
            SetProperty(exception, "_errors", errorCollection);
            return exception;
        }
        private static T Instantiate<T>() where T : class
        {
            return FormatterServices.GetUninitializedObject(typeof(T)) as T;
        }
        private static void SetProperty<T>(T targetObject, string fieldName, object value)
        {
            var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(targetObject, value);
            }
            else
            {
                throw new InvalidOperationException("No field with name " + fieldName);
            }
        }
    }

    public class NotReadOnlyContent : IContent
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ContentReference ContentLink { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ContentReference ParentLink { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid ContentGuid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ContentTypeID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsDeleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PropertyDataCollection Property => throw new NotImplementedException();
    }
}
