using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using Xunit;
using EPiServer.DataAbstraction;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Framework.Modules;
using EPiServer.ContentApi.Core.Internal;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    [Obsolete]
    public class DefaultContentModelMapperTest : TestBase
    {
        private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
        private readonly Mock<ContentTypeModelRepository> _contentTypeModelRepository;
        private readonly IContentModelReferenceConverter contentModelReferenceConverter;
        private readonly Mock<ReflectionService> _reflectionService;
        private readonly Mock<IContentVersionRepository> _contentVersionRepository;
        private readonly Mock<UrlResolverService> _urlResolverService;
        private readonly DefaultContentConverter _mapper;
        private readonly Mock<ContentLoaderService> _contentLoaderService;

        public DefaultContentModelMapperTest()
        {
            _contentTypeModelRepository = new Mock<ContentTypeModelRepository>();

            _contentTypeRepositoryMock = new Mock<IContentTypeRepository>();
            var linkMapper = new Mock<IPermanentLinkMapper>();
            linkMapper
                .Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(() => new PermanentLinkMap(Guid.NewGuid(), ContentReference.EmptyReference));
            _urlResolverService = new Mock<UrlResolverService>();
            contentModelReferenceConverter = new DefaultContentModelReferenceConverter(linkMapper.Object, _urlResolverService.Object);

            _contentTypeRepositoryMock
                .Setup(x => x.Load(It.IsAny<int>()))
                .Returns((int _) => new ContentType())
                .Verifiable();

            _contentVersionRepository = new Mock<IContentVersionRepository>();

            _reflectionService = new Mock<ReflectionService>(null);

            var moduleResourceResolver = new Mock<IModuleResourceResolver>();
            moduleResourceResolver.Setup(m => m.ResolvePath("CMS", null)).Returns("/episerver/cms");
            var contextModeResolver = new Mock<ContextModeResolver>(moduleResourceResolver.Object);

            var propertyConverter = new Mock<IPropertyConverter>();
            propertyConverter.Setup(p => p.Convert(It.IsAny<PropertyData>(), It.IsAny<ConverterContext>())).Returns(Mock.Of<IPropertyModel>());
            var propertyConverterResolver = new Mock<IPropertyConverterResolver>();
            propertyConverterResolver.Setup(p => p.Resolve(It.IsAny<PropertyData>())).Returns(propertyConverter.Object);

            _contentLoaderService = new Mock<ContentLoaderService>(new Mock<IContentLoader>().Object, linkMapper.Object, Mock.Of<IUrlResolver>(), contextModeResolver.Object, Mock.Of<IContentProviderManager>());
            _mapper = new DefaultContentConverter(_contentTypeRepositoryMock.Object, _reflectionService.Object, contentModelReferenceConverter, _contentVersionRepository.Object,
                _contentLoaderService.Object, _urlResolverService.Object, _apiConfig, propertyConverterResolver.Object);
        }

        [Fact]
        public void TransformContent_ShouldThrowArgumentNullException_WhenContentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.TransformContent(null, false));
        }

        [Fact]
        public void TransformContent_ShouldReturnContentApiModel_WhenContentIsPageData()
        {
            PageData testPage = CreateMockPageData();
            var result = _mapper.TransformContent(testPage);
            Assert.IsType<ContentApiModel>(result);
        }

        [Fact]
        public void TransformContent_ShouldThrowException_WhenContentTypeDoesntExist()
        {
            _contentTypeRepositoryMock
                .Setup(x => x.Load(It.IsAny<int>()))
                .Returns((int contentTypeId) =>
                {
                    return null;
                })
                .Verifiable();

            PageData testPage = CreateMockPageData();

            Assert.Throws<Exception>(() => _mapper.TransformContent(testPage, false));
        }

        [Fact]
        public void TransformContent_ShouldMapContentLink_WhenNotNull()
        {
            var testPage = CreateMockPageData();
            var mappedContent = _mapper.TransformContent(testPage, false);

            Assert.NotNull(mappedContent.ContentLink);
            Assert.Equal(13, mappedContent.ContentLink.Id.Value);
            Assert.Equal(3, mappedContent.ContentLink.WorkId);
        }

        [Fact]
        public void TransformContent_ShouldMapParentLink_WhenNotNull()
        {
            var testPage = CreateMockPageData();

            // create parent pages
            var parentProperties = new PropertyDataCollection();
            parentProperties.Add("PageLink", new PropertyPageReference(new PageReference(16, 4)));
            PageData parentPage = new PageData(new AccessControlList(), parentProperties);

            // mock get parent page
            _contentLoaderService.Setup(cl => cl.Get(It.Is<ContentReference>(x => x.ID == 16), It.IsAny<string>(), true)).Returns(parentPage);

            var mappedContent = _mapper.TransformContent(testPage, false);

            Assert.NotNull(mappedContent.ContentLink);
            Assert.Equal(testPage.ParentLink.ID, mappedContent.ParentLink.Id.Value);
            Assert.Equal(testPage.ParentLink.WorkID, mappedContent.ParentLink.WorkId);
        }

        [Fact]
        public void TransformContent_ShouldMapBaseContentTypes_WhenTypeIsPage()
        {
            var testPage = CreateMockPageData();
            var mappedContent = _mapper.TransformContent(testPage, false);

            Assert.Contains("Page", mappedContent.ContentType);
        }

        [Fact]
        public void TransformContent_ShouldMapBaseContentTypes_WhenTypeIsBlock()
        {
            _contentTypeRepositoryMock
                .Setup(x => x.Load(It.IsAny<int>()))
                .Returns((int contentTypeId) =>
                {
                    return new ContentType()
                    {
                        Name = "TestBlock"
                    };
                })
                .Verifiable();

            var testBlock = CreateMockBlockData();
            var mappedContent = _mapper.TransformContent(testBlock, false);

            Assert.Contains("Block", mappedContent.ContentType);
            Assert.Contains("TestBlock", mappedContent.ContentType);
        }

        [Fact]
        public void TransformContent_ShouldMapBaseContentTypes_WhenTypeIsImage()
        {
            var image = CreateMedia<ImageData>();
            var mappedContent = _mapper.TransformContent(image, false);

            Assert.Contains("Image", mappedContent.ContentType);
            Assert.Contains("Media", mappedContent.ContentType);
        }

        [Fact]
        public void TransformContent_ShouldMapBaseContentTypes_WhenTypeIsVideo()
        {
            var video = CreateMedia<VideoData>();
            var mappedContent = _mapper.TransformContent(video, false);

            Assert.Contains("Video", mappedContent.ContentType);
            Assert.Contains("Media", mappedContent.ContentType);
        }

        [Fact]
        public void TransformContent_ShouldMapPublishedState_WhenTypeIsVersionable()
        {
            var page = CreateMockPageData();
            var expectedPublishUtc = page.StartPublish.Value;
            var expectedStopPublishUtc = page.StopPublish.Value;

            var mappedContent = _mapper.TransformContent(page, false);

            Assert.Equal(expectedPublishUtc, mappedContent.StartPublish.Value);
            Assert.Equal(expectedStopPublishUtc, mappedContent.StopPublish.Value);
            Assert.Equal(VersionStatus.Published.ToString(), mappedContent.Status);
        }

        [Fact]
        public void TransformContent_ShouldMapCreatedState_WhenTypeIsChangeTrackable()
        {
            var page = CreateMockPageData();
            var expectedCreatedUtc = page.Created;
            var expectedChangedUtc = page.Changed;
            var expectedSavedUtc = page.Saved;
            var mappedContent = _mapper.TransformContent(page, false);

            Assert.Equal(expectedCreatedUtc, mappedContent.Created.Value);
            Assert.Equal(expectedChangedUtc, mappedContent.Changed.Value);
            Assert.Equal(expectedSavedUtc, mappedContent.Saved.Value);
        }

        [Fact]
        public void TransformContent_ShouldMapRouteSegment_WhenTypeIsRoutable()
        {
            var page = CreateMockPageData();

            page.URLSegment = "testUrlSegment";
            var mappedContent = _mapper.TransformContent(page, false);

            Assert.Equal(page.URLSegment, mappedContent.RouteSegment);
        }

        private PageData CreateMockPageData()
        {
            var properties = new PropertyDataCollection();
            properties.Add("PageLink", new PropertyPageReference(new PageReference(13, 3)));
            properties.Add("PageParentLink", new PropertyPageReference(new PageReference(16, 4)));
            properties.Add("PageLanguageBranch", new PropertyString("en-US"));
            properties.Add("PageMasterLanguageBranch", new PropertyString("en-US"));
            properties.Add("PageCreated", new PropertyDate(DateTime.UtcNow.AddDays(-2)));
            properties.Add("PageChanged", new PropertyDate(DateTime.UtcNow));
            properties.Add("PageSaved", new PropertyDate(DateTime.UtcNow));
            properties.Add("PageStartPublish", new PropertyDate(DateTime.UtcNow));
            properties.Add("PageStopPublish", new PropertyDate(DateTime.UtcNow.AddDays(2)));
            properties.Add("PageWorkStatus", new PropertyNumber(4)); ;
            properties.Add("PageUrlSegment", new PropertyString("test"));
            PageData testPage = new PageData(new AccessControlList(), properties);
            testPage.ParentLink = new PageReference(1);

            testPage.ExistingLanguages = new List<CultureInfo>()
            {
                new CultureInfo("en-US"),
                new CultureInfo("fr-CA"),
            };

            var parent = new TestBlock()
            {
                ContentLink = testPage.ParentLink
            };

            _contentLoaderService.Setup(svc => svc.Get(It.Is<ContentReference>(cr => cr.CompareToIgnoreWorkID(testPage.ParentLink)), It.IsAny<string>(), It.IsAny<bool>()))
                              .Returns(parent);
            return testPage;
        }

        private IContent CreateMockBlockData()
        {
            var block = new TestBlock();
            block.Name = "Test";
            block.ContentLink = new ContentReference(42, 6);
            block.ParentLink = new ContentReference(43, 0);
            block.ContentTypeID = 4;
            block.ContentGuid = new Guid();
            block.IsDeleted = false;

            // create parent pages
            var parentProperties = new PropertyDataCollection();
            parentProperties.Add("PageLink", new PropertyPageReference(new PageReference(block.ParentLink.ID, 4)));
            PageData parentPage = new PageData(new AccessControlList(), parentProperties);


            _contentLoaderService.Setup(svc => svc.Get(It.Is<ContentReference>(cr => cr.CompareToIgnoreWorkID(block.ParentLink)), It.IsAny<string>(), It.IsAny<bool>()))
                              .Returns(parentPage);
            return block;
        }

        private IContent CreateMedia<T>() where T : MediaData, new()
        {
            var media = new T();
            media.ParentLink = new ContentReference(1);
            var parent = new TestBlock()
            {
                ParentLink = media.ParentLink
            };
            _contentLoaderService.Setup(svc => svc.Get(It.Is<ContentReference>(cr => cr.CompareToIgnoreWorkID(media.ParentLink)), It.IsAny<string>(), It.IsAny<bool>()))
                              .Returns(parent);

            return media;
        }
        public class TestBlock : BlockData, IContent
        {
            public string Name { get; set; }
            public ContentReference ContentLink { get; set; }
            public ContentReference ParentLink { get; set; }
            public Guid ContentGuid { get; set; }
            public int ContentTypeID { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}
